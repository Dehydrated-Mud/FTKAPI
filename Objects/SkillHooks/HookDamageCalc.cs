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

namespace FTKAPI.Objects.SkillHooks
{
    internal class HookDamageCalc : BaseModule
    {
        private static ILHook _hook;
        public override void Initialize()
        {
            Unload();
            On.DamageCalculator._calcDamage += HookCalcDamage;
        }

        private static DummyDamageInfo HookCalcDamage(On.DamageCalculator.orig__calcDamage orig, AttackAttempt _atk, float _dmgMultiplier, bool _mainTarget, bool _itemAttack, SlotControl.AttackCheatType _cheatType)
        {
            Logger.LogWarning("I hooked it all!");
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
                int impervious = 0;
                if ((bool)_atk.m_DamagedDummy.m_CharacterOverworld && _atk.m_DamagedDummy.Taunting)
                {
                    impervious = _atk.m_DamagedDummy.m_CharacterOverworld.m_CharacterStats.m_PlayerLevel;
                }
                
                dummyAttackProperties.m_Resist = Mathf.Max(Mathf.Min(0, dummyAttackProperties.m_Resist), impervious);
                dummyAttackProperties.m_Armor = Mathf.Max(Mathf.Min(0, dummyAttackProperties.m_Armor), impervious);
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
                        }
                        else if (_atk.m_DefendedPhys >= _atk.m_DefendedMag)
                        {
                            _atk.m_AttackResponse = CharacterDummy.AttackResponse.Block;
                        }
                        else
                        {
                            _atk.m_AttackResponse = CharacterDummy.AttackResponse.MagicBlock;
                        }
                        if (_atk.m_Harmless)
                        {
                            _atk.m_AttackResponse = CharacterDummy.AttackResponse.HarmlessAttack;
                        }
                    }
                    else
                    {
                        if (num >= 0.5f)
                        {
                            _atk.m_AttackResponse = CharacterDummy.AttackResponse.Damaged;
                            //Hook Point: light damage reaction
                        }
                        else
                        {
                            _atk.m_AttackResponse = CharacterDummy.AttackResponse.DamagedHeavy;
                            //Hook Point: heavy damage reaction
                        }
                        float value = 1f - num;
                        value = Mathf.Clamp(value, 0.07f, 0.22f);
                        if ((bool)_atk.m_DamagedDummy.m_CharacterOverworld && _atk.m_DamagedDummy.m_CharacterOverworld.m_ShieldID != GridEditor.FTK_itembase.ID.None && UnityEngine.Random.value < value && _atk.m_TotalDMGMag == 0 && _atk.m_DamagedDummy.m_CharacterOverworld.m_CharacterStats.m_CharacterSkills.m_SteadFast)
                        {
                            _atk.m_AttackResponse = CharacterDummy.AttackResponse.SteadFast;
                            _atk.m_VictimHealthAfter = _atk.m_VictimHealthStart;
                            _atk.m_TotalReceivedDMG = 0;
                            _atk.m_TotalDMGCrit = 0;
                            _atk.m_AttackProficiency = GridEditor.FTK_proficiencyTable.ID.None;
                        }
                        //Hook Point: Any damage reaction
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
                    }
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
                Logger.LogWarning("char Overworld");
                if (_char.m_CharacterOverworld.m_CharacterStats.m_CharacterSkills is CustomCharacterSkills)
                {
                    Logger.LogWarning("custom skills");
                    CustomCharacterSkills _tmpSkills = (CustomCharacterSkills)_char.m_CharacterOverworld.m_CharacterStats.m_CharacterSkills;
                    if (_tmpSkills.Skills != null)
                    {
                        Logger.LogWarning("not null");
                        foreach (FTKAPI_CharacterSkill _skill in _tmpSkills.Skills)
                        {
                            Logger.LogWarning("found a skill");
                            if ((_skill.Trigger & _trig) == _trig)
                            {
                                Logger.LogWarning("Attempting Custom Skill");
                                _skill.Skill(_char.m_CharacterOverworld, _trig, _atk);
                            }
                        }
                    }
                }
            }
        }
    }
}
