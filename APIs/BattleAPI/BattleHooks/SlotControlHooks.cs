using FTKAPI.Objects.SkillHooks;
using FTKAPI.Utils;
using GridEditor;
using Logger = FTKAPI.Utils.Logger;
using FTKAPI.Objects;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static SlotSystemBase;

namespace FTKAPI.APIs.BattleAPI.BattleHooks
{
    public class SlotControlHooks : BaseModule
    {
        public override void Initialize()
        {
            On.SlotControl.SetSlotResults += SetSlotResultsHook;
            IL.SlotControl.ComputeAttackSlotResults += ComputeResultsHook;
            IL.SlotSystemBase.DidAttackerGetEncouragedSlot += EncourageHook;
            IL.SlotSystemBase.DidAttackerGetDistractedSlot+= DistractHook;
        }


        private void DistractHook(ILContext _il)
        {
            ILCursor c = new ILCursor(_il);
            if (c.TryGotoNext(
                x => x.MatchCallOrCallvirt<UnityEngine.Random>("get_value")
                ))
            {
                c.Index += 2;
                c.Emit(OpCodes.Ldloc_3);
                c.EmitDelegate<Func<float, CharacterDummy, float>>(DistractDelegate);
            }
        }

        private void EncourageHook(ILContext _il)
        {
            ILCursor c = new ILCursor(_il);
            if (c.TryGotoNext(
                x => x.MatchCallOrCallvirt<UnityEngine.Random>("get_value")
                ))
            {
                c.Index += 2;
                c.Emit(OpCodes.Ldloc_1);
                c.EmitDelegate<Func<float, CharacterDummy, float>>(EncourageDelegate);
            }
        }


        private void SetSlotResultsHook(On.SlotControl.orig_SetSlotResults _orig, SlotControl _this, bool _skip, FTKPlayerID _player, int _spentFocus, string[] _slotResults, int _slotSuccess, string _goodSlot, SlotType _slotType, int _prof)
        {
            // Perfect combat roll skill aplication
            bool isPerfect = true;
            bool wasEncouraged = false;
            bool wasDistracted = false;
            foreach(string _slotResult in _slotResults)
            {
                if (_slotResult.Contains("encourage"))
                {
                    wasEncouraged = true;
                }
                else if (_slotResult.Contains("distract"))
                {
                    wasDistracted = true;
                    isPerfect = false;
                }
                else if (_slotResult.ContainsAny(new List<string> { "miss", "darkness", "vexxed" }))
                {
                    isPerfect = false;
                }
            }
            CharacterOverworld cow = FTKHub.Instance.GetCharacterOverworldByFID(_player);
            if (isPerfect && cow.GetCurrentDummy())
            {
                Logger.LogWarning("Perfect roll, attempting to apply skills.");
                ApplySkills(cow, FTKAPI_CharacterSkill.TriggerType.PerfectCombatRoll);
            }
            _orig(_this, _skip, _player, _spentFocus, _slotResults, _slotSuccess, _goodSlot, _slotType, _prof);
        }

        private void SetProfDelegate(FTK_proficiencyTable.ID _prof)
        {
            BattleAPI.Instance.ActiveBattleSkill = _prof;
            if (BattleAPI.Instance.m_CharacterOverworld != null)
            {
                CharacterEventListener listener = BattleHelpers.GetListener(BattleAPI.Instance.m_CharacterOverworld);
                listener.HookupWeaponForBattle();
            }
        }

        private void ComputeResultsHook(ILContext il) 
        { 
            ILCursor c = new ILCursor(il);

            // Set the active battle prof to the one being computed
            c.Goto(0);
            c.Emit(OpCodes.Ldarg_3);
            c.EmitDelegate(SetProfDelegate);

            if (c.TryGotoNext(
                x => x.MatchLdloc(12),
                x => x.MatchLdfld<FTK_weaponStats2>("_skilltest"),
                x => x.MatchStloc(8)
                ))
            {
                c.Index += 2;
                c.Emit(OpCodes.Ldarg_3);
                c.EmitDelegate<Func<FTK_weaponStats2.SkillType, FTK_proficiencyTable.ID, FTK_weaponStats2.SkillType>>(BattleHelpers.SkillOverride);

                c.Index += 9;
                c.Emit(OpCodes.Ldarg_3);
                c.EmitDelegate<Func<string, FTK_proficiencyTable.ID, string>>(BattleHelpers.SkillOverride2);
            }

            if (c.TryGotoNext(
                x => x.MatchLdarg(1),
                x => x.MatchLdarg(3),
                x => x.MatchCallOrCallvirt<CharacterSkills>("CalledShot")
                ))
            {
                c.Index += 10;
                c.Emit(OpCodes.Ldarg_1);
                c.Emit(OpCodes.Ldloc_3);
                c.EmitDelegate(CriticalDelegate);
                c.Emit(OpCodes.Stloc_3);
            }
        }

        private float EncourageDelegate(float _orig, CharacterDummy _dummy)
        {
            CombatFloat combatFloat = new CombatFloat
            {
                Value = _orig,
                SetFloat = SetFloats.EncourageChance
            };
            
            return BattleAPI.Instance.GetFloat(_dummy, combatFloat);
        }

        private float DistractDelegate(float _orig, CharacterDummy _dummy)
        {
            CombatFloat combatFloat = new CombatFloat
            {
                Value = _orig,
                SetFloat = SetFloats.DistractChance
            };

            return BattleAPI.Instance.GetFloat(_dummy, combatFloat);
        }

        private bool CriticalDelegate(CharacterOverworld _cow, bool _flag)
        {
            // If the battleAPI has a flag it wants to set, it will send it here. Else, it will give back the original value
            CombatFlag critFlag = new CombatFlag { Value = _flag, Flag = SetFlags.Crit };
            CharacterDummy dummy = _cow.GetCombatDummy();
            bool flag = BattleAPI.Instance.GetFlag(dummy, critFlag);
            /*bool isDangerous = (dummy.m_SpecialAttack == CharacterDummy.SpecialAttack.Justice || dummy.m_SpecialAttack == CharacterDummy.SpecialAttack.CalledShot); // Is it dangerous to overwrite?

            // If the BattleAPI didn't change the flag, make sure we are not overriding justice or called shot
            bool cond = flag && (!isDangerous || critFlag.Override);



            */
            if (flag)
            {
                Logger.LogWarning("flag is true, sending crit");
                ApplySkills(_cow, FTKAPI_CharacterSkill.TriggerType.Critical);
                dummy.m_CriticalStrike = true;
            }
            return flag;
        }

        public override void Unload()
        {
            IL.SlotControl.ComputeAttackSlotResults -= ComputeResultsHook;
        }
    }
}
