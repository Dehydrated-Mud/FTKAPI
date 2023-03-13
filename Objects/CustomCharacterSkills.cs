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
        internal FTKAPI_ModifierTree m_ModifierTree;
        internal CharacterStats m_CharacterStats;
        internal FTKAPI_CharacterSkill[] m_IntrinsicSkills;
        internal FTKAPI_CharacterSkill[] m_Skills;

        public CustomCharacterSkills() : base() { }
        public CustomCharacterSkills(CharacterSkills _copy) : base(_copy) { }
        public CustomCharacterSkills(CustomCharacterSkills _copy, CharacterStats characterStats) : base(_copy) 
        {
            m_ModifierTree = _copy.m_ModifierTree;
            m_CharacterStats = characterStats;
            m_Skills = _copy.Skills;
            m_IntrinsicSkills = _copy.IntrinsicSkills;
        }

        public void AddSkills(CustomCharacterSkills _add)
        {
            base.AddSkills(_add);
            foreach (FTKAPI_CharacterSkill skill in _add.m_IntrinsicSkills)
            {
                m_Skills.AddItem(skill);
            }
        }

        public void ActivateMod(FTKAPI_ModifierTree.Milestone milestone)
        {
            m_CharacterStats.AddCharacterMod(m_ModifierTree.m_Dictionary[milestone]);
            m_CharacterStats.TallyCharacterMods();
        }

        public FTKAPI_ModifierTree ModTree
        {
            get => m_ModifierTree;
            set => m_ModifierTree = value; 
        }

        public FTKAPI_CharacterSkill[] Skills
        {
            get => m_Skills;
            set => m_Skills = value;
        }

        public FTKAPI_CharacterSkill[] IntrinsicSkills
        {
            get => m_IntrinsicSkills;
            set => m_IntrinsicSkills = value;
        }
    }
}
