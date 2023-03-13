using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static CharacterDummy;
using static MonoMod.Cil.RuntimeILReferenceBag.FastDelegateInvokers;
using Logger = FTKAPI.Utils.Logger;

namespace FTKAPI.Objects.SkillHooks
{
    internal class HookPlayAttackSequence : BaseModule
    {
        public void Initialize()
        {
            Unload();
            IL.CharacterDummy.PlayAttackSequence += PlayAttackSequenceHook;
        }

        private static void PlayAttackSequenceHook(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchCallOrCallvirt<CharacterDummy>("PlayAttackSequenceComplete")
                ) ;

            c.Remove();
            c.EmitDelegate<Action<CharacterDummy, CharacterDummy.AttackAnim, CharacterEventListener.CombatAnimTrigger, DummyDamageInfo, DummyDamageInfo, DummyDamageInfo>>((_this, _attackAnim, _override, _ddi, _ddi1, _ddi2) =>
            {

                if((bool)(_this.m_CharacterOverworld?.m_CharacterStats.m_CharacterSkills is CustomCharacterSkills))
                {
                    CustomCharacterSkills tmp = (CustomCharacterSkills)_this.m_CharacterOverworld.m_CharacterStats.m_CharacterSkills;
                    Logger.LogWarning("custom skills");
                    if (tmp.Skills != null)
                    {
                        foreach (FTKAPI_CharacterSkill _skill in tmp.Skills)
                        {
                            if (((_skill.m_TriggerType & FTKAPI_CharacterSkill.TriggerType.SpecialAttackAnim) == FTKAPI_CharacterSkill.TriggerType.SpecialAttackAnim) && _skill.proc)
                            {
                                _this.StartCoroutine(_this.WaitPlayCharacterAbilityEvent(_skill.SkillInfo, _attackAnim, _ddi, _ddi1, _ddi2));
                                return;
                            }
                        }
                    }
                }

                _this.PlayAttackSequenceComplete(_attackAnim, _override, _ddi, _ddi1, _ddi2);

            });
        }

        public override void Unload()
        {
            IL.CharacterDummy.PlayAttackSequence -= PlayAttackSequenceHook;
        }
    }
}
