using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Logger = FTKAPI.Utils.Logger;

namespace FTKAPI.Objects
{
    public class FTKAPI_CharacterSkill
    {
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
            IHeavyDamage = 1 << 14
        }

        internal TriggerType m_TriggerType;
        string m_Name;
        public virtual void Skill(CharacterOverworld cow, TriggerType trig, AttackAttempt atk) { }
        public virtual void Skill(CharacterOverworld _cow, TriggerType _trig) 
        {
            Logger.LogWarning("If you are seeing this you probably did not overwrite the Skill function, or overwrote the incorrect overload.");
        }
        public string Name
        {
            get => m_Name;
            set => m_Name = value;
        }
        public TriggerType Trigger
        {
            get => m_TriggerType;
            set => m_TriggerType = value;
        }
    }
}
