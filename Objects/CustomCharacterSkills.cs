using BepInEx.Logging;
using GridEditor;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Logger = FTKAPI.Utils.Logger;

namespace FTKAPI.Objects {
    public class CustomCharacterSkills : CharacterSkills
    {
        //internal List<FTKAPI_CharacterSkill> m_IntrinsicSkills = new List<FTKAPI_CharacterSkill>();
        internal List<FTKAPI_CharacterSkill> m_Skills = new List<FTKAPI_CharacterSkill>();

        public CustomCharacterSkills() : base() { }
        public CustomCharacterSkills(CharacterSkills _copy) : base(_copy) { }
        public CustomCharacterSkills(CustomCharacterSkills _copy) : base(_copy) 
        {
            AddSkillsCustom(_copy);
            //m_IntrinsicSkills = _copy.IntrinsicSkills;
        }

        public void AddSkillsCustom(CustomCharacterSkills _add, bool _skills = false)
        {
            base.AddSkills(_add);
            /*foreach (FTKAPI_CharacterSkill skill in _add.IntrinsicSkills)
            {
                this.Skills.Add(skill);
            }*/

            if (_add.Skills != null && _add.Skills.Count > 0)
            {
                foreach (FTKAPI_CharacterSkill skill in _add.Skills)
                {
                    this.Skills.Add(skill);
                }
            }
        }

        public List<FTKAPI_CharacterSkill> Skills
        {
            get => m_Skills;
            set => m_Skills = value;
        }

        /*public List<FTKAPI_CharacterSkill> IntrinsicSkills
        {
            get => m_IntrinsicSkills;
            set => m_IntrinsicSkills = value;
        }*/
    }
}
