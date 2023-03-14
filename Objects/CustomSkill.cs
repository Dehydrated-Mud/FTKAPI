using FTKAPI.Managers;
using GridEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using FTKAPI.Utils;
using Logger = FTKAPI.Utils.Logger;
using HutongGames.PlayMaker.Actions;

namespace FTKAPI.Objects
{
    public class CustomSkill : FTK_characterSkill
    {
        internal FTK_characterSkill.ID myBase = FTK_characterSkill.ID.Discipline;
        internal string PLUGIN_ORIGIN = "null";

        public CustomSkill(ID baseSkill = FTK_characterSkill.ID.Discipline)
        {
            myBase = baseSkill;
            var source = SkillManager.GetSkill(baseSkill);
            foreach (FieldInfo field in typeof(FTK_characterSkill).GetFields())
            {
                //FTKAPI.Utils.Logger.LogMessage("Setting field: " + field.Name + " to value: " + field.GetValue(source));
                field.SetValue(this, field.GetValue(source));
            }
        }

        public FTK_characterSkill.ID BaseSkill
        {
            get => myBase;
            set => myBase = value;   
        }

        public new string ID
        {
            get => this.m_ID;
            set => this.m_ID = value;
        }

        public string ProcSoundID
        { 
          get => this.m_ProcSoundID;
          set => this.m_ProcSoundID= value;
        }

        public CharacterEventListener.CombatAnimTrigger ProcAnim
        {
            get => this.m_ProcAnim;
            set => this.m_ProcAnim = value;
        }

        public CharacterEventListener.CombatAnimTrigger AttackAnim
        {
            get => this.m_AttackAnim; 
            set => this.m_AttackAnim = value;
        }
        private CustomLocalizedString hudDisplay;
        public CustomLocalizedString HudDisplay 
        { 
            get => this.hudDisplay;
            set
            {
                this.hudDisplay = value;
                this.m_HudDisplay = this.hudDisplay.GetLocalizedString();
            }
        }
    }
}
