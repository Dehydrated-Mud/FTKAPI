using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GridEditor;
using Logger = FTKAPI.Utils.Logger;

namespace FTKAPI.Objects
{
    public class FTKAPI_CharacterSkill
    {
        public string m_ToolTip;
        public string m_DisplayName;
        public bool proc = false;
        internal FTK_characterSkill.ID m_skillInfo = FTK_characterSkill.ID.None;
        [Flags]
        public enum TriggerType : uint
        {
            None = 0,
            EndTurn = 1 << 0,
            SlotHardOverride = 1 << 1,
            SlotSoftOverride = 1 << 2,
            SlotNoOverride = 1 << 3,
            KillShot = 1 << 4,
            RespondToHit = 1 << 5,
            AnyLandedAttack = 1 << 6,
            TheyDodged = 1 << 7,
            IDodged = 1 << 8,
            TheyAnyDamage = 1 << 9,
            TheyLightDamage = 1 << 10,
            TheyHeavyDamage = 1 << 11,
            IAnyDamage = 1 << 12,
            ILightDamage = 1 << 13,
            IHeavyDamage = 1 << 14,
            SpecialAttackAnim = 1 << 15,
            ConvertFocusToAction = 1 << 16,
            RollSlots = 1 << 17
        }

        internal TriggerType m_TriggerType;
        public virtual void Skill(CharacterOverworld cow, TriggerType trig, AttackAttempt atk) { }
        public virtual void Skill(CharacterOverworld _cow, TriggerType _trig) 
        {
            Logger.LogWarning("If you are seeing this you probably did not overwrite the Skill function, or overwrote the incorrect overload.");
        }
        public TriggerType Trigger
        {
            get => m_TriggerType;
            set => m_TriggerType = value;
        }
        private CustomLocalizedString name;
        public CustomLocalizedString Name
        {
            get => this.name;
            set
            {
                this.name = value;
                this.m_DisplayName = this.name.GetLocalizedString();
            }
        }
        private CustomLocalizedString description;
        public CustomLocalizedString Description
        {
            get => this.description;
            set
            {
                this.description = value;
                this.m_ToolTip = this.description.GetLocalizedString();
            }
        }
        public FTK_characterSkill.ID SkillInfo
        {
            get => this.m_skillInfo;
            set => this.m_skillInfo = value;
        }
    }
}
