using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Logger = FTKAPI.Utils.Logger;

namespace FTKAPI.Objects.SkillHooks
{
    internal class HookMainSkills : BaseModule
    {
        public override void Initialize()
        {
            Unload();
            IL.CharacterStats.TallyCharacterMods += HookTallyCharacterMods;
        }

        private static void HookTallyCharacterMods(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                x => x.Match(OpCodes.Ldarg_0),
                x => x.MatchLdfld<CharacterStats>("m_CharacterOverworld"),
                x => x.MatchCallOrCallvirt<CharacterOverworld>("GetDBEntry"),
                x => x.MatchLdfld<GridEditor.FTK_playerGameStart>("m_CharacterSkills"),
                x => x.MatchNewobj<CharacterSkills>()
                ) ;
            c.Index += 4;
            c.Remove();
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Func<CharacterSkills,CharacterStats,CustomCharacterSkills>>((_copy, _stats) =>
            {
                /*if(_copy is CustomCharacterSkills)
                {
                    CustomCharacterSkills tmp = (CustomCharacterSkills)_copy;
                    Logger.LogInfo(tmp.m_Skills is null);
                    Logger.LogInfo(_stats.GetType().Name);
                    Logger.LogInfo(new CustomCharacterSkills(tmp).m_Skills[0].Name);
                    return new CustomCharacterSkills(tmp);
                }*/
                return new CustomCharacterSkills(_copy);
            });
        }

        public override void Unload()
        {
            IL.CharacterStats.TallyCharacterMods -= HookTallyCharacterMods;
        }
    }
}
