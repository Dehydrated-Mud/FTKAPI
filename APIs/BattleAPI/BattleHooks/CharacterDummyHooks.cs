using FTKAPI.Objects.SkillHooks;
using Logger = FTKAPI.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GridEditor;

namespace FTKAPI.APIs.BattleAPI.BattleHooks
{
    public class CharacterDummyHooks : BaseModule
    {
        public override void Initialize()
        {
            On.CharacterDummy._beginActualEngageAttack += OnStartCombatTurn;
            On.CharacterDummy.ActionCompleted += FinishedAttackHook;
        }

        private void OnStartCombatTurn(On.CharacterDummy.orig__beginActualEngageAttack _orig, CharacterDummy _this)
        {
            //Logger.LogWarning("This should show at the start of every combat turn!");
            BattleAPI.Instance.CombatStartTurn(_this);
            ApplySkills(_this.m_CharacterOverworld, Objects.FTKAPI_CharacterSkill.TriggerType.StartCombatTurn);
            _orig(_this);
        }

        private void FinishedAttackHook(On.CharacterDummy.orig_ActionCompleted _orig, CharacterDummy _this)
        {
            CombatFloat combatFloat = new CombatFloat
            {
                Value = _this.m_PostAttackHealthMod,
                SetFloat = SetFloats.DamageReflection
            };
            _this.m_PostAttackHealthMod = FTKUtil.RoundToInt(BattleAPI.Instance.GetFloat(_this, combatFloat));
            _orig(_this);
            BattleAPI.Instance.ApplyProfsToAttacker(_this);
        }
    }
}
