using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTKAPI.Objects.SkillHooks
{
    public class HookDamageInfo : BaseModule
    {
        public override void Initialize()
        {
            Unload();
            //On.DummyAttackProperties.ctor += DamageInfoHook;
        }

        private void DamageInfoHook(On.DummyAttackProperties.orig_ctor _orig, DummyAttackProperties _this, AttackAttempt _atk)
        {
            _orig(_this, _atk);
            //bool attacker = _atk.m_AttackingDummy.m_CharacterOverworld;
            //_this.m_EvadeRating += HookDamageCalc.CombatModSkillHook(_atk.m_DamagedDummy, CustomModifier.ModTriggerType.EvasionAdd, _atk, attacker);
        }

        public override void Unload()
        {
            On.DummyAttackProperties.ctor -= DamageInfoHook;
        }
    }
}
