using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using IL;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

using Logger = FTKAPI.Utils.Logger;

namespace FTKAPI.Objects.SkillHooks
{
    internal class HookEndTurnSkills : BaseModule
    {
        private static ILHook _hook;
        public override void Initialize()
        {
            Unload();
            //IL.CharacterStats.EndTurnActionSequence += EndTurnActionSequence;
            Type type = typeof(CharacterStats).GetNestedType("<EndTurnActionSequence>c__Iterator0", BindingFlags.NonPublic);
            MethodInfo methodInfo = type.GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            _hook = new ILHook
            (
                methodInfo,
                EndTurnActionSequenceHook
            );
        }
        private static void EndTurnActionSequenceHook(ILContext il)
        {
            Type type = typeof(CharacterStats).GetNestedType("<EndTurnActionSequence>c__Iterator0", BindingFlags.NonPublic);
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdfld<CharacterStats>("m_CharacterOverworld"),
                x => x.MatchCallOrCallvirt<CharacterSkills>("FindHerb")
                ) ;

            var loadCharStats = c.Body.Instructions[c.Index - 1];
            c.Index -= 2;
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(loadCharStats.OpCode, loadCharStats.Operand);
            c.EmitDelegate<Action<CharacterStats>>((me) =>
            {
                Logger.LogInfo(me.GetType().FullName);
                //Logger.LogWarning(me.m_CharacterSkills is null);
                //Logger.LogWarning((me.m_CharacterSkills)?.GetType());
                if (me.m_CharacterSkills is CustomCharacterSkills)
                {
                    CustomCharacterSkills tmp = (CustomCharacterSkills)me.m_CharacterSkills;
                    if ((bool)(tmp.m_Skills?.Count() > 0)) 
                    {
                        foreach (FTKAPI_CharacterSkill skill in tmp.m_Skills)
                        {
                            if ((skill.m_TriggerType & FTKAPI_CharacterSkill.TriggerType.EndTurn) == FTKAPI_CharacterSkill.TriggerType.EndTurn)
                            {
                                Logger.LogInfo("Running character skill function: " + skill.Name);
                                skill.Skill(me.m_CharacterOverworld, FTKAPI_CharacterSkill.TriggerType.EndTurn);
                            }
                        }
                    }
                    
                }
                else
                {
                    Logger.LogWarning("This characters m_CharacterSkills object is not a CustomCharacterSkills object");
                }

            });
        }
        public override void Unload()
        {
            _hook?.Dispose();
            //IL.CharacterStats.EndTurnActionSequence -= EndTurnActionSequence;
        }
    }
}
