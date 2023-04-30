using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Logger = FTKAPI.Utils.Logger;

namespace FTKAPI.Objects
{
    /// <summary>
    /// Base class for storing character stats that do not exist in the base class.
    /// Make a class that inherits from this, and override the methods.
    /// </summary>
    public class CustomCharacterStats : MonoBehaviour
    {
        //public int imperviousArmor = 0;
        //public int imperviousResistance = 0;
        /// <summary>
        /// The characteroverworld that this customstats object corresponds to.
        /// </summary>
        public CharacterOverworld m_CharacterOverworld;
        /// <summary>
        /// Send stats to the battle API. Use this method for sending "static" stats to the battle API
        /// </summary>
        public virtual void SendCustomStats()
        {

        }
    }
}
