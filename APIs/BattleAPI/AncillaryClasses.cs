using FTKAPI.Objects;
using GridEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static HauntManager;

namespace FTKAPI.APIs.BattleAPI
{
    /// <summary>
    /// Stores a flag and a value for it.
    /// BattleAPI will iterate over these flags at various points to set values.
    /// </summary>
    public class CombatFlag : IEquatable<CombatFlag>
    {
        /// <summary>
        /// Which flag should the API set
        /// </summary>
        public SetFlags Flag { get; set;}
        /// <summary>
        /// Set true or false
        /// </summary>
        public bool Value { get; set;}

        /// <summary>
        /// If this is set to false, the BattleAPI will delete the combat flag if it is unused by the end of the target dummy's combat turn
        /// </summary>
        public bool Permanent { get; set;} = false;

        /// <summary>
        /// If true, the battle API should set the flag to the given value even if it may cause unexpected behaviors
        /// </summary>
        public bool Override { get; set; } = false;

        public bool Equals(CombatFlag other)
        {
            if (ReferenceEquals(other, null))
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return Flag.Equals(other.Flag) && Value.Equals(other.Value) && Permanent.Equals(other.Permanent);
        }

        public override int GetHashCode()
        {
            int hashFlag = Flag.GetHashCode();
            int hashValue = Value.GetHashCode();
            int hashPermanent = Permanent.GetHashCode();

            return hashFlag ^ hashValue ^ hashPermanent;
        }
    }

    public class CombatFloat 
    {
        /// <summary>
        /// The float that is to be modified
        /// </summary>
        public SetFloats SetFloat { get; set;}

        /// <summary>
        /// The value of the float
        /// </summary>
        public float Value { get; set;}

        /// <summary>
        /// 
        /// </summary>
        public bool Permanent { get; set;} = false;

        public delegate float Operation(float _orig, float _value);

        /// <summary>
        /// The operation that the battle API should preform with the given float. Defaults to addition
        /// </summary>
        public Operation Operator { get; set; } = CombatValueOperators.Add;
    }

    public class CombatValueOperators
    {
        public static float Add(int _orig, int _value) { return _orig + _value; }
        public static float Add(float _orig, float _value) { return _orig + _value; }

        public static float Subtract(int _orig, int _value) { return _orig - _value; }
        public static float Subtract(float _orig, float _value) { return _orig - _value; }
    }

    /// <summary>
    /// The battle API will use this object to create a special action.
    /// A special action is a turn inserted at the beginning of combat.
    /// Use this object to specify what options the player should have during that turn.
    /// The player will always be provided a pass option.
    /// </summary>
    public class SpecialCombatAction
    {
        private List<FTK_proficiencyTable.ID> m_RegularProfs = new();
        private List<ProfInfoContainer> m_SpecialProfs = new();
        /// <summary>
        /// Regular proficiency IDs that should be displayed as options in the special action
        /// </summary>
        public List<FTK_proficiencyTable.ID> RegularProfs { get { return m_RegularProfs; } }

        /// <summary>
        /// Special proficiencies that should be displayed as options in the special action
        /// </summary>
        public List<ProfInfoContainer> SpecialProfs { get { return m_SpecialProfs; } }

        /// <summary>
        /// Should taunt be displayed as an option?
        /// </summary>
        public bool Taunt { get; set; } = false;

        /// <summary>
        /// Should the switch weapon button be displayed as an option?
        /// </summary>
        public bool EquipWeapon { get; set; } = false;

        /// <summary>
        /// Use to merge two special combat actions
        /// </summary>
        /// <param name="_other"></param>
        public void Merge(SpecialCombatAction _other)
        {
            if (_other.RegularProfs.Count > 0)
            {
                foreach (FTK_proficiencyTable.ID id in _other.RegularProfs)
                {
                    if (!RegularProfs.Contains(id))
                    {
                        m_RegularProfs.Add(id);
                    }
                }
            }

            if (_other.SpecialProfs.Count > 0)
            {
                foreach (ProfInfoContainer prof in _other.SpecialProfs)
                {
                    // No check here because two skills should not be inserting identical profs...
                    m_SpecialProfs.Add(prof);
                }
            }
            Taunt |= _other.Taunt;
            EquipWeapon |= _other.EquipWeapon;
        }

    }
    /// <summary>
    /// Fight order entries that have been added by the API
    /// </summary>
    public class APIFightOrderEntry : EncounterSessionMC.FightOrderEntry
    {
        public APIFightOrderEntry(FTKPlayerID _pid, int _entryID) : base(_pid, _entryID) { }
    }
}
