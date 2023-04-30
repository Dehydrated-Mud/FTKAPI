using FullInspector;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Logger = FTKAPI.Utils.Logger;
using TriggerType = FTKAPI.Objects.FTKAPI_CharacterSkill.TriggerType;
using ModTriggerType = FTKAPI.Objects.CustomModifier.ModTriggerType;
using GridEditor;
using FTKAPI.Managers;
using static CharacterDummy;
using static Colorful.ChannelSwapper;
using FTKAPI.APIs.BattleAPI;

namespace FTKAPI.Objects.SkillHooks
{
    internal class HookDamageCalc : BaseModule
    {
        private static ILHook _hook;
        public override void Initialize()
        {
            Unload();
            On.DamageCalculator._calcDamage += HookCalcDamage;
            
            //On.CharacterDummy.RespondToHit += TestHook;
        }

        private static DummyDamageInfo HookCalcDamage(On.DamageCalculator.orig__calcDamage orig, AttackAttempt _atk, float _dmgMultiplier, bool _mainTarget, bool _itemAttack, SlotControl.AttackCheatType _cheatType)
        {
            CommonBattleHook(_atk.m_AttackingDummy, TriggerType.DamageCalcStart, _atk);
            CommonBattleHook(_atk.m_DamagedDummy, TriggerType.DamageCalcStart, _atk);
            bool attacker = false;
            CustomCharacterStats customStats = new CustomCharacterStats();
            if ((bool)_atk.m_DamagedDummy.m_CharacterOverworld)
            {
                attacker = false;
                Logger.LogWarning("Defender is character");
                if ((bool)_atk.m_DamagedDummy.m_CharacterOverworld.gameObject.GetComponent<CustomCharacterStats>())
                {
                    Logger.LogWarning("Defender has customcharacterstats");
                    customStats = _atk.m_DamagedDummy.m_CharacterOverworld.gameObject.GetComponent<CustomCharacterStats>();
                }
            }
            else if ((bool)_atk.m_AttackingDummy.m_CharacterOverworld)
            {
                attacker = true;
                Logger.LogWarning("Attacker is character");
                if ((bool)_atk.m_AttackingDummy.m_CharacterOverworld.gameObject.GetComponent<CustomCharacterStats>())
                {
                    Logger.LogWarning("Attacker has customcharacterstats");
                    customStats = _atk.m_AttackingDummy.m_CharacterOverworld.gameObject.GetComponent<CustomCharacterStats>();
                }
            }
            else
            {
                Logger.LogError("Could not find a player with a CustomCharacterStats object! Attacking Dummy: " + _atk.m_AttackingDummy.name + " and Defending Dummy: " + _atk.m_DamagedDummy.name);
            }

            DummyAttackProperties dummyAttackProperties = new DummyAttackProperties(_atk);
            if (_itemAttack)
            {
                GridEditor.FTK_proficiencyTable entry = GridEditor.FTK_proficiencyTableDB.GetDB().GetEntry(_atk.m_AttackProficiency);
                ProficiencyBase proficiencyBase = ProficiencyManager.Instance.Get(_atk.m_AttackProficiency);
                _atk.m_SpecialAttack = CharacterDummy.SpecialAttack.ItemAttack;
                dummyAttackProperties.m_EvadeRating = 0f;
                dummyAttackProperties.m_MaxWeaponDamage = (int)proficiencyBase.m_CustomValue;
            }
            else if (_atk.m_DamagedDummy is EnemyDummy)
            {
                EnemyDummy enemyDummy = (EnemyDummy)_atk.m_DamagedDummy;
                if (!enemyDummy.IsEvasive())
                {
                    for (int i = 0; i < _atk.m_AttackFocused; i++)
                    {
                        dummyAttackProperties.m_EvadeRating /= 2f;
                    }
                }
            }
            if (_atk.m_IgnoresArmor)
            {
                CombatFloat combatArmor = new CombatFloat
                {
                    Value = 0,
                    SetFloat = SetFloats.ImperviousArmor
                };
                CombatFloat combatResist = new CombatFloat
                {
                    Value = 0,
                    SetFloat = SetFloats.ImperviousResist
                };

                int minArmor = FTKUtil.RoundToInt(BattleAPI.Instance.GetFloat(_atk.m_DamagedDummy, combatArmor));
                int minResist = FTKUtil.RoundToInt(BattleAPI.Instance.GetFloat(_atk.m_DamagedDummy, combatResist));

                Logger.LogWarning($"Granting +{minArmor} impervious armor and +{minResist} impervious resistance");
                dummyAttackProperties.m_Resist = Mathf.Min(minResist, dummyAttackProperties.m_Resist);
                dummyAttackProperties.m_Armor = Mathf.Min(minArmor, dummyAttackProperties.m_Armor);

            }
            _atk.m_TotalDMG = FTKUtil.RoundToInt(_atk.m_SlotSuccessPercent * (float)dummyAttackProperties.m_MaxWeaponDamage * _dmgMultiplier);
            switch (_cheatType)
            {
                case SlotControl.AttackCheatType.KillSingle:
                case SlotControl.AttackCheatType.KillAll:
                    _atk.m_TotalDMG = 1000;
                    break;
                case SlotControl.AttackCheatType.Miss:
                    _atk.m_TotalDMG = 0;
                    break;
                default:
                    if (GameCheat.Instance.m_EnemyNoDamage && _atk.m_AttackingDummy is EnemyDummy)
                    {
                        _atk.m_TotalDMG = 0;
                    }
                    else if (CombatAutomation.Auto && !CombatAutomation.Instance.GetInstructions(_atk.m_DamagedDummy).m_AllowTakeDamage)
                    {
                        _atk.m_TotalDMG = 0;
                    }
                    break;
            }

            // Battle API call to modify total damage
            CombatFloat cFloat = new CombatFloat
            {
                SetFloat = SetFloats.OutgoingTotalDmg,
                Value = _atk.m_TotalDMG
            };
            _atk.m_TotalDMG = FTKUtil.RoundToInt(BattleAPI.Instance.GetFloat(_atk.m_AttackingDummy, cFloat));

            CombatFloat cFloat1 = new CombatFloat
            {
                SetFloat = SetFloats.IncomingTotalDmg,
                Value = _atk.m_TotalDMG
            };
            _atk.m_TotalDMG = FTKUtil.RoundToInt(BattleAPI.Instance.GetFloat(_atk.m_DamagedDummy, cFloat1));
            //

            if (_atk.m_DamageType == GridEditor.FTK_weaponStats2.DamageType.physical)
            {
                _atk.m_TotalDMGPhys = _atk.m_TotalDMG;
            }
            else if (_atk.m_DamageType == GridEditor.FTK_weaponStats2.DamageType.magic)
            {
                _atk.m_TotalDMGMag = _atk.m_TotalDMG;
            }
            else
            {
                Logger.LogError("Error, no damage type assigned");
            }
            
            if (_atk.m_AttackingDummy.m_CriticalStrike)
            {
                _atk.m_TotalDMGCrit = FTKUtil.RoundToInt((float)_atk.m_TotalDMG * GameFlow.Instance.m_CritDmgPercent);
            }
            _atk.m_VictimHealthStart = dummyAttackProperties.m_HealthStart;
            _atk.m_VictimHealthAfter = dummyAttackProperties.m_HealthStart;
            if (_atk.m_TotalDMGMag > 0)
            {
                if (_atk.m_DamagedDummy.Frozen)
                {
                    _atk.m_TotalDMGMag = FTKUtil.RoundToInt((float)_atk.m_TotalDMGMag * GameFlow.Instance.m_FrozenDmgPercent);
                }
                _atk.m_ReceivedDMGMag = _atk.m_TotalDMGMag + _atk.m_TotalDMGCrit - dummyAttackProperties.m_Resist;
                _atk.m_ReceivedDMGMag = Mathf.Clamp(_atk.m_ReceivedDMGMag, 0, int.MaxValue);
                _atk.m_DefendedMag = _atk.m_TotalDMGMag - _atk.m_ReceivedDMGMag;
                _atk.m_VictimHealthAfter = _atk.m_VictimHealthStart - _atk.m_ReceivedDMGMag;
                _atk.m_VictimHealthAfter = Mathf.Clamp(_atk.m_VictimHealthAfter, 0, int.MaxValue);
            }
            if (_atk.m_TotalDMGPhys > 0)
            {
                if (_atk.m_DamagedDummy.Frozen)
                {
                    _atk.m_TotalDMGPhys = FTKUtil.RoundToInt((float)_atk.m_TotalDMGPhys * GameFlow.Instance.m_FrozenDmgPercent);
                }
                _atk.m_ReceivedDMGPhys = _atk.m_TotalDMGPhys + _atk.m_TotalDMGCrit - dummyAttackProperties.m_Armor;
                _atk.m_ReceivedDMGPhys = Mathf.Clamp(_atk.m_ReceivedDMGPhys, 0, 1000);
                _atk.m_DefendedPhys = _atk.m_TotalDMGPhys - _atk.m_ReceivedDMGPhys;
                _atk.m_VictimHealthAfter = _atk.m_VictimHealthStart - _atk.m_ReceivedDMGPhys;
                _atk.m_VictimHealthAfter = Mathf.Clamp(_atk.m_VictimHealthAfter, 0, int.MaxValue);
            }
            _atk.m_TotalReceivedDMG += _atk.m_ReceivedDMGPhys;
            _atk.m_TotalReceivedDMG += _atk.m_ReceivedDMGMag;
            _atk.m_TotalDefendedDMG = _atk.m_DefendedMag + _atk.m_DefendedPhys;
            bool flag = true;
            CharacterDummy.SpecialAttack specialAttack = _atk.m_SpecialAttack;
            if (specialAttack == CharacterDummy.SpecialAttack.BlackHole)
            {
                _atk.m_AttackResponse = CharacterDummy.AttackResponse.BlackHole;
                flag = false;
            }
            else if (_atk.m_DamagedDummy.CanDodge() && _cheatType == SlotControl.AttackCheatType.None && UnityEngine.Random.value < dummyAttackProperties.m_EvadeRating)
            {
                _atk.m_TotalReceivedDMG = 0;
                _atk.m_VictimHealthAfter = _atk.m_VictimHealthStart;
                _atk.m_AttackResponse = CharacterDummy.AttackResponse.Dodge;
                //Hook Point: Dodge
                //CommonBattleHook(_atk.m_AttackingDummy, FTKAPI_CharacterSkill.TriggerType.TheyDodged, _atk);
                //CommonBattleHook(_atk.m_DamagedDummy, FTKAPI_CharacterSkill.TriggerType.IDodged, _atk);
                flag = false;
            }
            if (flag)
            {
                if (_mainTarget) 
                { 
                    CommonBattleHook(_atk.m_AttackingDummy, FTKAPI_CharacterSkill.TriggerType.AnyLandedAttack, _atk); 
                }
                if (_atk.m_VictimHealthAfter > 0)
                {
                    float num = (float)_atk.m_VictimHealthAfter / (float)_atk.m_VictimHealthStart;
                    if (num == 1f)
                    {
                        if (_atk.m_TotalDMG == 0 && _atk.m_TotalDefendedDMG == 0)
                        {
                            _atk.m_AttackResponse = CharacterDummy.AttackResponse.Block;
                            CommonBattleHook(_atk.m_DamagedDummy, TriggerType.BlockedAttack, _atk);
                        }
                        else if (_atk.m_DefendedPhys >= _atk.m_DefendedMag)
                        {
                            _atk.m_AttackResponse = CharacterDummy.AttackResponse.Block;
                            CommonBattleHook(_atk.m_DamagedDummy, TriggerType.BlockedAttack, _atk);
                        }
                        else
                        {
                            _atk.m_AttackResponse = CharacterDummy.AttackResponse.MagicBlock;
                            CommonBattleHook(_atk.m_DamagedDummy, TriggerType.BlockedMagicAttack, _atk);
                        }
                        if (_atk.m_Harmless)
                        {
                            _atk.m_AttackResponse = CharacterDummy.AttackResponse.HarmlessAttack;
                        }
                    }
                    else
                    {
                        CommonBattleHook(_atk.m_DamagedDummy, TriggerType.TakeAnyDamage, _atk);
                        CommonBattleHook(_atk.m_AttackingDummy, TriggerType.InflictAnyDamage, _atk);
                        if (num >= 0.5f)
                        {
                            _atk.m_AttackResponse = CharacterDummy.AttackResponse.Damaged;
                            CommonBattleHook(_atk.m_DamagedDummy, TriggerType.TakeLightDamage, _atk);
                            CommonBattleHook(_atk.m_AttackingDummy, TriggerType.InflictLightDamage, _atk);
                        }
                        else
                        {
                            _atk.m_AttackResponse = CharacterDummy.AttackResponse.DamagedHeavy;
                            CommonBattleHook(_atk.m_DamagedDummy, FTKAPI_CharacterSkill.TriggerType.TakeHeavyDamage, _atk);
                            CommonBattleHook(_atk.m_AttackingDummy, FTKAPI_CharacterSkill.TriggerType.InflictHeavyDamage, _atk);
                        }

                        float value = 1f - num;
                        value = Mathf.Clamp(value, 0.07f, 0.22f);

                        // BattleAPI call to get any modifications to the character's chance to proc steadfast.
                        if ((bool)_atk.m_DamagedDummy.m_CharacterOverworld){
                            // Basically a fancy pass by reference hehe
                            CombatFloat combatFloat = new CombatFloat
                            {
                                Value = value,
                                SetFloat = SetFloats.SteadFast
                            };
                            value = BattleAPI.Instance.GetFloat(_atk.m_DamagedDummy, combatFloat);
                        }

                        if ((bool)_atk.m_DamagedDummy.m_CharacterOverworld && _atk.m_DamagedDummy.m_CharacterOverworld.m_ShieldID != GridEditor.FTK_itembase.ID.None && UnityEngine.Random.value < value && _atk.m_TotalDMGMag == 0 && _atk.m_DamagedDummy.m_CharacterOverworld.m_CharacterStats.m_CharacterSkills.m_SteadFast)
                        {
                            _atk.m_AttackResponse = CharacterDummy.AttackResponse.SteadFast;
                            _atk.m_VictimHealthAfter = _atk.m_VictimHealthStart;
                            _atk.m_TotalReceivedDMG = 0;
                            _atk.m_TotalDMGCrit = 0;
                            _atk.m_AttackProficiency = GridEditor.FTK_proficiencyTable.ID.None;
                        }
                    }
                }
                else
                {
                    _atk.m_AttackResponse = CharacterDummy.AttackResponse.Death;
                    if ((bool)_atk.m_DamagedDummy.m_CharacterOverworld)
                    {
                        if (_atk.m_DamagedDummy.IsResistDeath)
                        {
                            _atk.m_VictimHealthAfter = 1;
                            _atk.m_AttackResponse = CharacterDummy.AttackResponse.ResistDeath;
                        }
                        else if (_atk.m_DamagedDummy.m_CharacterOverworld.m_CharacterStats.m_MySanctumID != GridEditor.FTK_sanctumStats.ID.None)
                        {
                            _atk.m_VictimHealthAfter = FTKUtil.RoundToInt((float)_atk.m_DamagedDummy.m_CharacterOverworld.m_CharacterStats.MaxHealth * GameFlow.Instance.GameDif.m_ReviveMaxHealthGain);
                            _atk.m_AttackResponse = CharacterDummy.AttackResponse.ResistDeathSanctum;
                        }
                    }
                    else if ((bool)_atk.m_AttackingDummy.m_CharacterOverworld && CharacterSkills.Discipline(_atk.m_AttackingDummy.m_CharacterOverworld, _atk.m_DamagedDummy, _killshot: true))
                    {
                        _atk.m_SpecialAttack = CharacterDummy.SpecialAttack.Discipline;
                    } // Can likely comment out this discipline block
                    else if ((bool)_atk.m_AttackingDummy.m_CharacterOverworld)
                    {
                        CommonBattleHook(_atk.m_AttackingDummy, FTKAPI_CharacterSkill.TriggerType.KillShot, _atk);
                    }
                    if (CombatAutomation.Auto && !CombatAutomation.Instance.GetInstructions(_atk.m_DamagedDummy).m_AllowDie)
                    {
                        _atk.m_VictimHealthAfter = 1;
                        _atk.m_AttackResponse = CharacterDummy.AttackResponse.ResistDeath;
                    }
                }
            }
            return new DummyDamageInfo(_atk, _mainTarget);
        }

        public override void Unload()
        {
            _hook?.Dispose();
            On.DamageCalculator._calcDamage += HookCalcDamage;
        }

        public static void CommonBattleHook(CharacterDummy _char, FTKAPI_CharacterSkill.TriggerType _trig, AttackAttempt _atk)
        {
            if ((bool)_char.m_CharacterOverworld)
            {
                //Logger.LogInfo("char Overworld");
                if (_char.m_CharacterOverworld.m_CharacterStats.m_CharacterSkills is CustomCharacterSkills)
                {
                    //Logger.LogInfo("custom skills");
                    CustomCharacterSkills _tmpSkills = (CustomCharacterSkills)_char.m_CharacterOverworld.m_CharacterStats.m_CharacterSkills;
                    if (_tmpSkills.Skills != null)
                    {
                        //Logger.LogInfo("not null");
                        foreach (FTKAPI_CharacterSkill _skill in _tmpSkills.Skills)
                        {
                            Logger.LogInfo("found a skill: " + _skill.m_DisplayName);
                            if ((_skill.Trigger & _trig) == _trig)
                            {
                                Logger.LogInfo("Attempting Custom Skill: " + _skill.m_DisplayName);
                                _skill.Skill(_char.m_CharacterOverworld, _trig, _atk);
                            }
                        }
                    }
                }
            }
        }
    }
}
