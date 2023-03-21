using MonoMod.Cil;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using GridEditor;

namespace FTKAPI.Objects.SkillHooks
{
    public class HookTallyCharacterMods : BaseModule
    {
        public override void Initialize()
        {
            Unload();
            IL.CharacterStats.TallyCharacterMods += TallyCharacterModsHook;
        }

        internal void TallyCharacterModsHook(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdfld<CharacterStats>("m_ModGold"),
                x => x.MatchLdloc(2),
                x => x.MatchLdfld<GridEditor.FTK_characterModifier>("m_ModGold"),
                x => x.MatchAdd(),
                x => x.MatchStfld<CharacterStats>("m_ModGold")
                );
            c.Index += 5;
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc_2);
            c.EmitDelegate<Action<CharacterStats, FTK_characterModifier>>((_this, _entry) =>
            {
                if (_entry is CustomModifier)
                {
                    CustomModifier _tmp = (CustomModifier)_entry;
                    _tmp.AddStatModifierToTally(_this);
                }
            });
        }

        public override void Unload()
        {
            IL.CharacterStats.TallyCharacterMods -= TallyCharacterModsHook;
        }
    }
}
