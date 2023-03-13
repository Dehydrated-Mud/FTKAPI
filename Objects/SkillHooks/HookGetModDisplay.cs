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
using UnityEngine;
using GridEditor;


namespace FTKAPI.Objects.SkillHooks
{
    internal class HookGetModDisplay : BaseModule
    {
        public override void Initialize()
        {
            On.CharacterSkills.GetModDisplay += GetModDisplayHook;
        }

        private string GetModDisplayHook(On.CharacterSkills.orig_GetModDisplay orig, object _o, bool _format)
        {

            Logger.LogWarning("Entering our new GetModDisplay method!");
            string text = orig(_o, _format);
            if (_o is CustomCharacterSkills)
            {
                CustomCharacterSkills tmpSkills = (CustomCharacterSkills) _o;
                foreach (FTKAPI_CharacterSkill skill in tmpSkills.Skills)
                {
                    text = text + skill.m_DisplayName + "\n";
                }
                return text;
            }
            return text;

        }

        public override void Unload()
        {
            On.CharacterSkills.GetModDisplay -= GetModDisplayHook;
        }
    }
}
