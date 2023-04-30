using FTKAPI.Objects;
using FTKAPI.Objects.SkillHooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static FTKAPI.Objects.FTKAPI_CharacterSkill;
using Logger = FTKAPI.Utils.Logger;
using GridEditor;

namespace FTKAPI.APIs.BattleAPI.BattleHooks
{
    internal class DamageCalculationHooks : BaseModule
    {
        public override void Initialize()
        {
            On.DamageCalculator._generateAttackAttempt += HookAttackAttempt;
        }
        private AttackAttempt HookAttackAttempt(On.DamageCalculator.orig__generateAttackAttempt _orig, CharacterDummy.AttackAnim _attackAnim, CharacterEventListener.CombatAnimTrigger _attackAnimOverride, CharacterDummy _attackingDummy, CharacterDummy _damagedDummy, FTK_weaponStats2.DamageType _dmgType, SlotControl.AttackCheatType _chtType, float _slotSuccessPercent, int _focusedSlots, bool _harmless, FTK_proficiencyTable.ID _prof, bool _profSuccess)
        {
            AttackAttempt orig = _orig(_attackAnim, _attackAnimOverride, _attackingDummy, _damagedDummy, _dmgType, _chtType, _slotSuccessPercent, _focusedSlots, _harmless, _prof, _profSuccess);
            CharacterDummy dummy;
            if (_attackingDummy.m_CharacterOverworld)
            {
                dummy = _attackingDummy;
                if (BattleAPI.Instance.ActiveBattleSkills.ContainsKey(_prof)) 
                {
                    ProfInfoContainer container = BattleAPI.Instance.ActiveBattleSkills[_prof];
                    if (container.OverrideAttackAnim)
                    {
                        orig.m_AttackAnim = container.AttackAnim;
                    }
                }
            }
            return orig;
        }

        public override void Unload()
        {
            On.DamageCalculator._generateAttackAttempt -= HookAttackAttempt;
        }
    }
}
