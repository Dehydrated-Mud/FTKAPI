using MonoMod.Cil;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil.Cil;
using GridEditor;
using FTKAPI.Managers;
using Logger = FTKAPI.Utils.Logger;

namespace FTKAPI.Objects.SkillHooks
{
    public class HookTallyCharacterMods : BaseModule
    {
        public override void Initialize()
        {
            Unload();
            On.CharacterStats.TallyCharacterMods += TallyCharacterModsHook;

        }

        internal void TallyCharacterModsHook(On.CharacterStats.orig_TallyCharacterMods orig, CharacterStats _this)
        {
            /*ILCursor c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdfld<CharacterStats>("m_ModGold"),
                x => x.MatchLdloc(2),
                x => x.MatchLdfld<FTK_characterModifier>("m_ModGold"),
                x => x.MatchAdd(),
                x => x.MatchStfld<CharacterStats>("m_ModGold")
                );
            c.Index += 5;
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldloc_2);
            c.EmitDelegate<Action<CharacterStats, FTK_characterModifier>>((_this, _entry) =>
            {*/
            orig(_this);

            CustomCharacterSkills newSkills = MakeSkills(_this); // New Skill object with default character skills
            if (SkillManager.Instance.globalSkills.Skills?.Count() > 0)
            {
                newSkills.AddSkillsCustom(SkillManager.Instance.globalSkills, _skills: true); // Add any characterskills that we want to be present for every character always.
                foreach (FTKAPI_CharacterSkill skill in SkillManager.Instance.globalSkills.Skills)
                {
                    Logger.LogWarning("Found global skill: " + skill.m_DisplayName);
                }
            }
            /*if (SkillManager.Instance.globalSkills.IntrinsicSkills?.Count() > 0)
            {
                foreach (FTKAPI_CharacterSkill skill in SkillManager.Instance.globalSkills.IntrinsicSkills)
                {
                    Logger.LogWarning("Found global skill: " + skill.m_DisplayName);
                }
            }*/
            
            foreach (FTK_characterModifier.ID characterMod in _this.m_CharacterMods)
            {
                FTK_characterModifier _entry = FTK_characterModifierDB.GetDB().GetEntry(characterMod);
                //Logger.LogWarning("Entered the tally delegate");
                //Logger.LogWarning(_entry.GetType().FullName);
                
                if (_entry.m_CharacterSkills is CustomCharacterSkills)
                {
                    CustomCharacterSkills tmpSkills = (CustomCharacterSkills)_entry.m_CharacterSkills;
                    newSkills.AddSkillsCustom(tmpSkills);
                }
                else
                {
                    newSkills.AddSkills(_entry.m_CharacterSkills);
                }
            }
            _this.m_CharacterSkills = newSkills;
        }

        public override void Unload()
        {
            On.CharacterStats.TallyCharacterMods -= TallyCharacterModsHook;
        }

        private CustomCharacterSkills MakeSkills(CharacterStats _stats)
        {
            CharacterSkills _copy = _stats.m_CharacterOverworld.GetDBEntry().m_CharacterSkills;
            if (_copy is CustomCharacterSkills)
            {
                CustomCharacterSkills _tmp = (CustomCharacterSkills)_copy;
                return new CustomCharacterSkills( _tmp );
            }
            return new CustomCharacterSkills(_copy);

        }
    }
}
