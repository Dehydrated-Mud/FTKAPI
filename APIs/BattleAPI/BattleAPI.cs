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
using System.ComponentModel;

namespace FTKAPI.APIs.BattleAPI
{
    using ProficiencyManager = FTKAPI.Managers.ProficiencyManager;
    public enum Query
    {
        None = 0,
        StartCombatTurn,
        EndCombat,
        StartCombat
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
        DamageReflection,
        LifestealFac,
        CalledShot,
        EncourageChance,
        DistractChance
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
        private List<FTK_itembase.ID> m_ItemsToAdd = new();
        private Dictionary<CharacterOverworld, int> m_BonusXp = new ();
        private Dictionary<CharacterOverworld, int> m_BonusGold = new();
        private Dictionary<CharacterDummy, List<SpecialCombatAction>> m_SpecialCombatActions = new();
        private List<FTKPlayerID> m_ToRush = new ();
        private Dictionary<CharacterDummy, List<FTK_proficiencyTable.ID[]>> m_StartTurnProfs = new ();
        public CharacterOverworld m_CharacterOverworld;
        public CharacterDummy m_CurrentDummy;
        // Hooks
        HookUiBattleButtons hookUiBattleButtons = new HookUiBattleButtons();
        CharacterDummyHooks hookCharacterDummy = new CharacterDummyHooks();
        DamageCalculationHooks hookDamageCalculation= new DamageCalculationHooks();
        SlotControlHooks hookSlotControl = new SlotControlHooks();
        EncounterSessionHooks hookEncounterSession = new();
        GameLogicHooks hookGameLogic = new();
        CharacterSkillsHooks hookCharacterSkills = new();
        /// <summary>
        /// Battle API initialize all hooks
        /// </summary>
        internal void Initialize()
        {
            Logger.LogInfo("Initializing the Battle API");
            // Add the proficiency that allows us to "skip" a turn.
            // Init hooks
            hookUiBattleButtons.Initialize();
            hookCharacterDummy.Initialize();
            hookDamageCalculation.Initialize();
            hookSlotControl.Initialize();
            hookEncounterSession.Initialize();
            hookGameLogic.Initialize();
            hookCharacterSkills.Initialize();
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
            hookGameLogic.Unload();
            hookCharacterSkills.Unload();
        }
        /// <summary>
        /// Actions that the battle api executes every time a player character starts their turn
        /// </summary>
        /// <param name="player"></param>
        public void CombatStartTurn(CharacterDummy player)
        {
            Logger.LogInfo("Battle API preforming start of combat turn actions.");
            m_ActiveBattleSkills.Clear();
            m_AttackerProfs.Clear();
            m_ToRush.Clear();
            m_BonusXp.Clear();
            m_BonusGold.Clear();
            RemoveSpecialAction(m_CurrentDummy);
            m_CurrentDummy = player;
            m_CharacterOverworld = player.m_CharacterOverworld;
            BattleHelpers.QuerySkills(m_CharacterOverworld, Query.StartCombatTurn);
            ApplyStartTurnProfs(player);
        }



        /// <summary>
        /// Add the special proficiency info to the battle API's queue of possible abilities.
        /// Provide this function with a new ProfInfoContainer
        /// </summary>
        /// <param name="profInfo"></param>
        public void SendProfInfo(ProfInfoContainer profInfo)
        {
            Logger.LogInfo("Battle API has received a ProfInfoContainer: " + profInfo.AttackProficiency);
            if (!m_ActiveBattleSkills.ContainsKey(profInfo.AttackProficiency))
            {
                m_ActiveBattleSkills.Add(profInfo.AttackProficiency, profInfo);
            }
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
                foreach(ProfValues prof in _battleStance.m_Proficiencies)
                {
                    prof.m_Button.gameObject.SetActive(false);
                }
                // Make sure that all of the special actions for this dummy are consolidated and stored at index 0
                ConsolidateSpecialActions(dummy);
                // Always add the skip turn button
                SpecialCombatAction specialAction = m_SpecialCombatActions[dummy][0];
                if (specialAction.SpecialProfs.Count > 0)
                {
                    foreach (ProfInfoContainer container in specialAction.SpecialProfs)
                    {
                        // Send the prof info so that attack animation overrides work correctly
                        SendProfInfo(container);
                        _battleStance.m_Proficiencies.Add(BattleHelpers.MakeBattleButton(m_CharacterOverworld, container, _battleStance, _weapon));
                    }
                }
                _battleStance.m_FleeButton.gameObject.SetActive(false);
                _battleStance.m_AttackButton.gameObject.SetActive(false);
                _battleStance.m_ShieldTauntButton.gameObject.SetActive(specialAction.Taunt);
                _battleStance.m_ShieldTauntButton.SetCanUse(specialAction.Taunt);
                _battleStance.m_EquipWeaponButton.SetCanUse(_battleStance.m_EquipWeaponButton.m_CanUse && specialAction.EquipWeapon);
                // Remove the special actions for this character
                ProfInfoContainer skipProf = new SkipTurnInfo();
                _battleStance.m_Proficiencies.Add(BattleHelpers.MakeBattleButton(m_CharacterOverworld, skipProf, _battleStance, _weapon));
                SendProfInfo(skipProf);
                
                // If we made it this far, return so we don't end up adding the active battle skills.
                return;
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
        /// Give the battle API a proficiency that should be applied to the given dummy at the start of their next turn.
        /// </summary>
        /// <param name="_dummy"></param>
        /// <param name="_profs"></param>
        public void ApplyProfNextTurn(CharacterDummy _dummy, FTK_proficiencyTable.ID[] _profs)
        {
            if(m_StartTurnProfs.ContainsKey(_dummy))
            {
                m_StartTurnProfs[_dummy].Add(_profs);
            }
            else
            {
                m_StartTurnProfs[_dummy] = new List<FTK_proficiencyTable.ID[]> { _profs };
            }
        }

        internal void ApplyStartTurnProfs(CharacterDummy _dummy)
        {
            if (m_StartTurnProfs.ContainsKey(_dummy))
            {
                foreach (FTK_proficiencyTable.ID[] profs in m_StartTurnProfs[_dummy])
                {
                    _dummy.RPCAllSelf("AddProfToDummy", new object[3] { profs, true, true });
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
        /// Adds item drops to be given out at the end of battle
        /// </summary>
        /// <param name="_item"></param>
        public void AddDrop(FTK_itembase.ID _item)
        {
            m_ItemsToAdd.Add(_item);
        }

        /// <summary>
        /// Add bonus XP for the specified player. Applied in GameLogicHooks
        /// </summary>
        /// <param name="_player"></param>
        /// <param name="_amount"></param>
        public void AddXp(CharacterOverworld _player, int _amount)
        {
            if (m_BonusXp.Keys.Contains(_player))
            {
                m_BonusXp[_player] += _amount;
            }
            else
            {
                m_BonusXp[_player] = _amount;
            }
        }

        /// <summary>
        /// Add bonus gold for the specified player. Applied in GameLogicHooks
        /// </summary>
        /// <param name="_player"></param>
        /// <param name="_amount"></param>
        public void AddGold(CharacterOverworld _player, int _amount)
        {
            if (m_BonusGold.Keys.Contains(_player))
            {
                m_BonusGold[_player] += _amount;
            }
            else
            {
                m_BonusGold[_player] = _amount;
            }
        }

        /// <summary>
        /// Rush a player
        /// </summary>
        /// <param name="_player"></param>
        public void RushPlayer(CharacterOverworld _player)
        {
            m_ToRush.Add(_player.m_FTKPlayerID);
        }

        /// <summary>
        /// All dummies with multiple combat actions will have their special actions merged into one.
        /// </summary>
        internal void ConsolidateSpecialActions(CharacterDummy _dummy)
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

        internal void RemoveSpecialAction(CharacterDummy _dummy)
        {
            if (_dummy && m_SpecialCombatActions.ContainsKey(_dummy))
            {
                m_SpecialCombatActions.Remove(_dummy);
            }
        }

        internal void ApplyProfsToAttacker(CharacterDummy _dummy)
        {
            if (m_AttackerProfs.Count > 0)
            {
                FTK_proficiencyTable.ID[] profs = AttackerProfs.ToArray();
                _dummy.AddProfToDummy(profs, true, true);
                m_AttackerProfs.Clear();
            }
        }

        internal void StartOfBattle()
        {
            // Called by encounter session hook
            Logger.LogInfo("Battle API marks this as the start of battle. Preforming start of battle actions.");
            m_ItemsToAdd.Clear();
            m_StartTurnProfs.Clear();
            // Populate the customstat dictionaries with the custom stats that we want to track throughout the battle
            GetCustomStats();
            foreach(FTKPlayerID combatant in EncounterSessionMC.Instance.m_AllCombtatants)
            {
                CharacterOverworld dummy = FTKHub.Instance.GetCharacterOverworldByFID(combatant);
                BattleHelpers.QuerySkills(dummy, Query.StartCombat);
            }
            
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
            m_StartTurnProfs.Clear();
            m_AttackerProfs.Clear();
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
        /// <summary>
        /// A list of items that should be dropped to the players after battle
        /// </summary>
        public List<FTK_itembase.ID> ItemsToAdd { get { return m_ItemsToAdd; } }

        /// <summary>
        /// Give bonus XP to players that have earned it
        /// </summary>
        public Dictionary<CharacterOverworld, int> BonusXp { get { return m_BonusXp; } }

        /// <summary>
        /// Give bonus Gold to players that have earned it
        /// </summary>
        public Dictionary<CharacterOverworld, int> BonusGold {  get { return m_BonusGold;} }

        /// <summary>
        /// Queue of players to rush
        /// </summary>
        public List<FTKPlayerID> ToRush { get { return m_ToRush; } }

        /// <summary>
        /// Dictionary of proficiencies that should be applied to the character dummy at the start of their next turn
        /// </summary>
        public Dictionary<CharacterDummy, List<FTK_proficiencyTable.ID[]>> StartTurnProfs { get { return m_StartTurnProfs;} }
    }
}
