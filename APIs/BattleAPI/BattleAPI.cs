using FTKAPI.APIs.BattleAPI.BattleHooks;
using Logger = FTKAPI.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FTKAPI.Managers;
using FTKAPI.Objects;
using GridEditor;
using ProfValues = uiBattleStanceButtons.ProfValues;
namespace FTKAPI.APIs.BattleAPI
{
    public enum Query
    {
        None = 0,
        StartCombatTurn,
        EndCombat
    }

    public enum SetFlags
    {
        None = 0,
        Crit,
    }

    public enum SetFloats
    {
        None = 0,
        SteadFast,
        OutgoingTotalDmg,
        IncomingTotalDmg,
        ImperviousArmor,
        ImperviousResist,
        DamageReflection
    }
    public class BattleAPI : BaseManager<BattleAPI>
    {
        // Data
        /// <summary>
        /// Proficiencies that skills have added to the possible options for the player to pick from
        /// </summary>
        private Dictionary<FTK_proficiencyTable.ID, ProfInfoContainer> m_ActiveBattleSkills = new ();
        private Dictionary<CharacterDummy, List<CombatFlag>> m_CombatFlags = new ();
        private Dictionary<CharacterDummy, List<CombatFloat>> m_CombatFloats = new ();
        private Dictionary<CharacterDummy, List<CombatFloat>> m_CustomStats = new();
        private List<FTK_proficiencyTable.ID> m_AttackerProfs = new();
        private Dictionary<CharacterDummy, List<SpecialCombatAction>> m_SpecialCombatActions;

        public CharacterOverworld m_CharacterOverworld;
        // Hooks
        HookUiBattleButtons hookUiBattleButtons = new HookUiBattleButtons();
        CharacterDummyHooks hookCharacterDummy = new CharacterDummyHooks();
        DamageCalculationHooks hookDamageCalculation= new DamageCalculationHooks();
        SlotControlHooks hookSlotControl = new SlotControlHooks();
        EncounterSessionHooks hookEncounterSession = new();
        /// <summary>
        /// Battle API initialize all hooks
        /// </summary>
        internal void Initialize()
        {
            Logger.LogInfo("Initializing the Battle API");
            hookUiBattleButtons.Initialize();
            hookCharacterDummy.Initialize();
            hookDamageCalculation.Initialize();
            hookSlotControl.Initialize();
            hookEncounterSession.Initialize();
        }
        /// <summary>
        /// Battle API unload all hooks
        /// </summary>
        internal void Unload()
        {
            hookUiBattleButtons.Unload();
            hookCharacterDummy.Unload();
            hookDamageCalculation.Unload();
            hookSlotControl.Unload();
            hookEncounterSession.Unload();
        }
        /// <summary>
        /// Actions that the battle api executes every time a player character starts their turn
        /// </summary>
        /// <param name="player"></param>
        public void CombatStartTurn(CharacterOverworld player)
        {
            Logger.LogInfo("Battle API preforming start of combat turn actions.");
            m_ActiveBattleSkills.Clear();
            m_AttackerProfs.Clear();
            m_CharacterOverworld = player;
            BattleHelpers.QuerySkills(m_CharacterOverworld, Query.StartCombatTurn);
        }
        /// <summary>
        /// Add the special proficiency info to the battle API's queue of possible abilities.
        /// Provide this function with a new ProfInfoContainer
        /// </summary>
        /// <param name="profInfo"></param>
        public void SendProfInfo(ProfInfoContainer profInfo)
        {
            Logger.LogInfo("Battle API has received a ProfInfoContainer: " + profInfo.AttackProficiency);
            m_ActiveBattleSkills.Add(profInfo.AttackProficiency, profInfo);
        }
        /// <summary>
        /// Takes ProfInfoContainers from m_ActiveBattleSkills, converts them to ProfValues, and adds them as buttons.
        /// Called in uiBattleStanceButtonHooks
        /// </summary>
        internal void AddButtons(uiBattleStanceButtons _battleStance, Weapon _weapon)
        {
            CharacterDummy dummy = m_CharacterOverworld.GetCurrentDummy();
            if (m_SpecialCombatActions.ContainsKey(dummy) && m_SpecialCombatActions[dummy].Count > 0)
            {
                SpecialCombatAction specialAction = m_SpecialCombatActions[dummy][0];
                foreach (ProfInfoContainer container in m_ActiveBattleSkills.Values)
                {
                    _battleStance.m_Proficiencies.Add(BattleHelpers.MakeBattleButton(m_CharacterOverworld, container, _battleStance, _weapon));
                }
                _battleStance.m_FleeButton.SetCanUse(false);
                _battleStance.m_ShieldTauntButton.SetCanUse(specialAction.Taunt);
                _battleStance.m_EquipWeaponButton.SetCanUse(_battleStance.m_EquipWeaponButton.m_CanUse && specialAction.EquipWeapon);
            }
            if (m_ActiveBattleSkills != null && m_ActiveBattleSkills.Count > 0) 
            {
                foreach (ProfInfoContainer container in m_ActiveBattleSkills.Values)
                {
                    _battleStance.m_Proficiencies.Add(BattleHelpers.MakeBattleButton(m_CharacterOverworld, container, _battleStance, _weapon));
                }
            }
        }
        /// <summary>
        /// Sets a combat flag to true or false. Will be removed form combat flag queue upon handling
        /// </summary>
        /// <param name="_dummy"></param>
        /// <param name="_value"></param>
        /// <param name="_flag"></param>
        /// 
        // TODO: End combat round query, and implement queue cleaning function
        public void SetAFlag(CharacterDummy _dummy, bool _value, SetFlags _flag, bool _override = false)
        {
            CombatFlag flag = new CombatFlag()
            {
                Flag = _flag,
                Value = _value,
                Override = _override
            };
            if (!m_CombatFlags.ContainsKey(_dummy)) 
            {
                m_CombatFlags.Add(_dummy, new List<CombatFlag> { flag });
            }
            else
            {
                m_CombatFlags[_dummy].Add(flag);
            }
        }
        /// <summary>
        /// Set a float in combat
        /// </summary>
        /// <param name="_dummy"></param>
        /// <param name="_value"></param>
        /// <param name="_setFloats"></param>
        public void SetAFloat(CharacterDummy _dummy, float _value, SetFloats _setFloats, CombatFloat.Operation operation)
        {
            CombatFloat combatFloat = new CombatFloat()
            {
                SetFloat = _setFloats,
                Value = _value,
                Operator = operation
                
            };
            if (!m_CombatFloats.ContainsKey(_dummy) ) 
            { 
                m_CombatFloats.Add(_dummy, new List<CombatFloat> { combatFloat });
            }
            else
            {
                m_CombatFloats[_dummy].Add(combatFloat);
            }
        }

        /// <summary>
        /// Registers a custom stat with the API. These stats last until the end of combat, but can be updated.
        /// </summary>
        /// <param name="_dummy"></param>
        /// <param name="_value"></param>
        /// <param name="_setFloats"></param>
        /// <param name="operation"></param>
        public void SetAStat(CharacterDummy _dummy, float _value, SetFloats _setFloats, CombatFloat.Operation operation)
        {
            CombatFloat combatFloat = new CombatFloat()
            {
                SetFloat = _setFloats,
                Value = _value,
                Operator = operation
            };
            if (!m_CustomStats.ContainsKey(_dummy))
            {
                m_CustomStats.Add(_dummy, new List<CombatFloat> { combatFloat });
            }
            else
            {
                m_CustomStats[_dummy].Add(combatFloat);
            }
        }

        /// <summary>
        /// Retrieve boolean value for specified dummy and flag
        /// </summary>
        /// <param name="_dummy"></param>
        /// <param name="_combatFlag"></param>
        /// <returns></returns>
        public bool GetFlag(CharacterDummy _dummy, CombatFlag _combatFlag)
        {
            if (m_CombatFlags.ContainsKey(_dummy))
            {
                CombatFlag combatFlag = CombatFlags[_dummy].FirstOrDefault(f => f.Flag == _combatFlag.Flag);
                if (combatFlag != null)
                {
                    // Return found value and remove flag
                    CombatFlags[_dummy].Remove(combatFlag);
                    return combatFlag.Value;
                }
            }
            // If we do not have a matching flag for this dummy, return back the value that was given
            return _combatFlag.Value;
        }
        /// <summary>
        /// Retrieve a float value for a specified dummy and CombatFlag
        /// </summary>
        /// <param name="_dummy"></param>
        /// <param name="_combatFloat"></param>
        /// <returns></returns>
        public float GetFloat(CharacterDummy _dummy, CombatFloat _combatFloat)
        {
            _combatFloat.Value = GetStat(_dummy, _combatFloat);
            if (m_CombatFloats.ContainsKey(_dummy))
            {
                CombatFloat combatFloat = CombatFloats[_dummy].FirstOrDefault(f => f.SetFloat == _combatFloat.SetFloat);
                if (combatFloat != null)
                {
                    // Return found value and remove flag
                    CombatFloats[_dummy].Remove(combatFloat);
                    // Modify the original float with the new one based on the operator that the set CombatFloat has.
                    return combatFloat.Operator(_combatFloat.Value, combatFloat.Value);
                }
            }
            // If we do not have a matching float for this dummy, return back the value that was given
            return _combatFloat.Value;
        }

        /// <summary>
        /// Gets the value of a custom stat. If multiple modifications are being made to the same stat, they will be compounded based on the given operators
        /// If not all operators in the list commute, this will likely give strange results.
        /// </summary>
        /// <param name="_dummy"></param>
        /// <param name="_combatFloat"></param>
        /// <returns></returns>
        private float GetStat(CharacterDummy _dummy, CombatFloat _combatFloat)
        {
            if (m_CustomStats.ContainsKey(_dummy))
            {
                List<CombatFloat> combatFloats = CustomStats[_dummy].Where(f => f.SetFloat == _combatFloat.SetFloat).ToList();
                if (combatFloats != null && combatFloats.Count > 0)
                {
                    if (combatFloats.Count > 1)
                    {
                        // Compound each combatFloat in the array with the first based on each CombatFloats operator.
                        for (int i = 1; i < combatFloats.Count; i++)
                        {
                            combatFloats[0].Value = combatFloats[i].Operator(combatFloats[0].Value, combatFloats[i].Value);
                        }
                    }
                    // Modify the original float with the new one based on the operator that the set CombatFloat has.
                    // Replace all the custom stats of this type with the consolidated CombatFloat
                    m_CustomStats[_dummy].RemoveAll(f => f.SetFloat == _combatFloat.SetFloat);
                    m_CustomStats[_dummy].Add(combatFloats[0]);
                    return combatFloats[0].Operator(_combatFloat.Value, combatFloats[0].Value);
                }
            }
            // If we do not have a matching float for this dummy, return back the value that was given
            return _combatFloat.Value;
        }
        /// <summary>
        /// Give the Battle API a proficiency that should be applied to the attacker once they finish their attack.
        /// </summary>
        public void AddProfToAttacker(FTK_proficiencyTable.ID prof)
        {
            m_AttackerProfs.Add(prof);
        }

        /// <summary>
        /// Registers a special combat action with the API.
        /// </summary>
        /// <param name="_dummy"></param>
        /// <param name="_action"></param>
        public void DeclareSpecialAction(CharacterDummy _dummy, SpecialCombatAction _action)
        {
            if (!m_SpecialCombatActions.ContainsKey(_dummy))
            {
                m_SpecialCombatActions[_dummy] = new List<SpecialCombatAction> { _action };
            }
            else
            {
                m_SpecialCombatActions[_dummy].Add(_action);
            }
        }

        /// <summary>
        /// All dummies with multiple combat actions will have their special actions merged into one.
        /// </summary>
        internal void ConsolidateSpecialActions()
        {
            foreach(CharacterDummy _dummy in m_SpecialCombatActions.Keys)
            {
                List<SpecialCombatAction> specialActions = m_SpecialCombatActions[_dummy];
                if (specialActions.Count > 1)
                {
                    for (int i = 1; i < specialActions.Count; i++)
                    {
                        specialActions[0].Merge(specialActions[i]);
                    }
                    m_SpecialCombatActions[_dummy] = new List<SpecialCombatAction> { specialActions[0] };
                }
            }
        }

        internal void ApplyProfsToAttacker(CharacterDummy _dummy)
        {
            if (AttackerProfs.Count > 0)
            {
                FTK_proficiencyTable.ID[] profs = AttackerProfs.ToArray();
                _dummy.AddProfToDummy(profs, true, true);
            }
        }

        internal void StartOfBattle()
        {
            // Called by encounter session hook
            Logger.LogInfo("Battle API marks this as the start of battle. Preforming start of battle actions.");
            // Populate the customstat dictionaries with the custom stats that we want to track throughout the battle
            GetCustomStats();
        }
        internal void EndOfBattle()
        {
            // Called by encounter session hook
            Logger.LogInfo("Battle API marks this as the end of battle. Preforming end of battle actions.");
            foreach(FTKPlayerID combatant in EncounterSessionMC.Instance.m_AllCombtatants)
            {
                BattleHelpers.QuerySkills(FTKHub.Instance.GetCharacterOverworldByFID(combatant), Query.EndCombat);
            }
            ClearCombatFlags();
            ClearCombatFloats();
            ClearStats();
            AttackerProfs.Clear();
        }
        internal void ClearCombatFlags()
        {
            Logger.LogInfo("Battle API is clearing combat flags");
            m_CombatFlags.Clear();
        }
        internal void ClearCombatFloats()
        {
            Logger.LogInfo("Battle API is clearing combat floats");
            m_CombatFloats.Clear();
        }

        internal void ClearStats()
        {
            Logger.LogInfo("Battle API is clearing custom stats");
            m_CustomStats.Clear();
        }

        private void GetCustomStats()
        {
            foreach(FTKPlayerID id in EncounterSessionMC.Instance.m_AllCombtatantsAlive)
            {
                foreach(CustomCharacterStats stats in FTKHub.Instance.GetCharacterOverworldByFID(id).gameObject.GetComponents<CustomCharacterStats>())
                {
                    stats.SendCustomStats();
                }
            }
        }

        /// <summary>
        /// Read only list of the API's current registered special profs
        /// </summary>
        public Dictionary<FTK_proficiencyTable.ID, ProfInfoContainer> ActiveBattleSkills { get { return m_ActiveBattleSkills;} }
        /// <summary>
        /// Queue of combat flags
        /// </summary>
        public Dictionary<CharacterDummy, List<CombatFlag>> CombatFlags { get { return m_CombatFlags;} }

        /// <summary>
        /// Queue of combat floats
        /// </summary>
        public Dictionary<CharacterDummy, List<CombatFloat>> CombatFloats { get { return m_CombatFloats;} }

        /// <summary>
        /// Custom stats that are registered with the battle API.
        /// </summary>
        public Dictionary<CharacterDummy, List<CombatFloat>> CustomStats { get { return m_CustomStats; } }

        /// <summary>
        /// The list of proficiencies to apply to the attacker this turn.
        /// </summary>
        public List<FTK_proficiencyTable.ID> AttackerProfs { get { return m_AttackerProfs;} }

        /// <summary>
        /// A collection of special combat actions that a character dummy will get at the beginning of combat.
        /// </summary>
        public Dictionary<CharacterDummy, List<SpecialCombatAction>> SpecialCombatActions { get { return m_SpecialCombatActions;} }
    }
}
