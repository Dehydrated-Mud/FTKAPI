using FTKAPI.Objects.SkillHooks;
using FTKAPI.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Logger = FTKAPI.Utils.Logger;
using FightOrderEntry = EncounterSessionMC.FightOrderEntry;
using MonoMod.Cil;
using UnityEngine;
using Mono.Cecil.Cil;
using GridEditor;
using TriggerType = FTKAPI.Objects.FTKAPI_CharacterSkill.TriggerType;

namespace FTKAPI.APIs.BattleAPI.BattleHooks
{
    public class EncounterSessionHooks : BaseModule
    {
        public override void Initialize()
        {
            Unload();
            //On.EncounterSession.AddTimelineSpacing += TimelineSpacingHook;
            On.EncounterSessionMC.ComputeFightOrder += ComputeFightOrderHook;
            //On.EncounterSession.UpdateAttackTimeline += UpdateHook;
            //On.EncounterSession.InitAttackTimeline += InitTimelineHook;
            On.EncounterSessionMC.FinalEncounterFinished += EncounterFinishedHook;
            On.EncounterSession.StartEncounterSession_Actual += EncounterStartHook;
            IL.EncounterSession.ApplyProficiencyEffect += ApplyHook;
            On.EncounterSession.SetEncounterSlotResults += SlotResultsHook;
        }

        private void SlotResultsHook(On.EncounterSession.orig_SetEncounterSlotResults _orig, EncounterSession _self, FTKPlayerID _playerID, FTK_slotOutput.ID _slotOutputID, string[] _slotResults, int _slotSuccess, int _spentFocus)
        {
            if (!_slotResults.Contains("miss"))
            {
                ApplySkills(FTKHub.Instance.GetCharacterOverworldByFID(_playerID), TriggerType.PerfectEncounterRoll);
            }
            ApplySkills(FTKHub.Instance.GetCharacterOverworldByFID(_playerID), TriggerType.SlotResults);
            _orig(_self, _playerID, _slotOutputID, _slotResults, _slotSuccess, _spentFocus);
        }

        private void ApplyHook(ILContext _il)
        {
            ILCursor c = new ILCursor(_il);
            c.Goto(2);
            c.EmitDelegate(RushDelegate);
            c.Emit(OpCodes.Stloc_0);
        }

        private List<FTKPlayerID> RushDelegate()
        {
            List<FTKPlayerID> res = new List<FTKPlayerID>();
            if (BattleAPI.Instance.ToRush.Count> 0)
            {
                foreach (FTKPlayerID id in BattleAPI.Instance.ToRush)
                {
                    res.Add(id);
                }
            }
            return res;
        }

        private void UpdateHook(On.EncounterSession.orig_UpdateAttackTimeline _orig, EncounterSession _this, bool _attacked, EncounterSessionMC.FightOrderEntry[] _foe, int _activeTimePortraitID, ContinueFSM _cfsm)
        {
            if (BattleAPI.Instance.SpecialCombatActions.Keys.Count > 0)
            {
                List<FTKPlayerID> list = new List<FTKPlayerID>(); //All engaged dummies
                float num = 0f;
                foreach (CharacterDummy value in _this.m_Dummies.Values)
                {
                    if (value.m_IsAlive && !value.m_DidFlee)
                    {
                        list.Add(value.FID);
                        num = Mathf.Max(num, Mathf.Max(value.GetCombatQuickness(_withProfMod: false), value.GetCombatQuickness(_withProfMod: true)));
                    }
                }
                //num is the quickness of the fastest character
                float quicknessToTime = FTKUtil.GetQuicknessToTime(num);
                List<EncounterSessionMC.FightOrderEntry> list2 = FTKUtil.ArrayToList(_foe);
                List<EncounterSessionMC.FightOrderEntry> list3 = new List<EncounterSessionMC.FightOrderEntry>(_this.m_FightOrderVisual);
                if (_attacked && !_this.m_SomeoneFledThisTurn)
                {
                    EncounterSession.RemoveAttacker(list3, list2);
                }
                EncounterSession.RemoveLeftCombatants(list3, list2);
                Dictionary<EncounterSession.AnimEntryID, EncounterSession.AnimEntry> dictionary = new Dictionary<EncounterSession.AnimEntryID, EncounterSession.AnimEntry>();
                foreach (EncounterSessionMC.FightOrderEntry item in list3) // Make a timeline animation for each fight order entry
                {
                    List<float> list4 = new List<float>();
                    list4.Add(item.m_TTA);
                    dictionary[new EncounterSession.AnimEntryID(item.m_Pid, item.m_EntryID)] = new EncounterSession.AnimEntry(list4);
                }
                List<EncounterSessionMC.FightOrderEntry> list5 = new List<EncounterSessionMC.FightOrderEntry>(list2);
                /*if (EncounterSession.CheckProficiencyEffects(list, list2, ref _activeTimePortraitID))
                {
                    bool flag = false;
                    if (list2.Count != list5.Count)
                    {
                        flag = true;
                    }
                    for (int i = 0; i < list2.Count; i++)
                    {
                        if (list2[i].m_Pid != list5[i].m_Pid)
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        list3 = EncounterSession.AddTimelineSpacing(list2, _keepFirst: true);
                        EncounterSession.AddAnim(dictionary, list3);
                    }
                }*/
                //FTKUtil.ExpandFightOrderList(list, quicknessToTime, list2, ref _activeTimePortraitID);
                EncounterSessionMC.Instance.m_ActiveTimePortraitID = _activeTimePortraitID;
                //list3 = EncounterSession.AddTimelineSpacing(list2, _keepFirst: true);
                EncounterSession.AddAnim(dictionary, list3);
                EncounterSessionMC.Instance.m_CombatTimeElapsed = EncounterSession.AdvanceTimeLine(list2);
                //list3 = EncounterSession.AddTimelineSpacing(list2, _keepFirst: false, _isFinal: true);
                EncounterSession.AddAnim(dictionary, list3);
                EncounterSession.ResetDummyProficiencyCheck();
                EncounterSession.RemoveOldAnim(dictionary, list3);
                EncounterSession.SetStunIcon(dictionary, list2);
                _this.m_FightOrderVisual = list3;
                _this.m_SomeoneFledThisTurn = false;
                FTKPlayerID[] array = new FTKPlayerID[list2.Count];
                int num2 = 0;
                foreach (EncounterSessionMC.FightOrderEntry item2 in list2)
                {
                    ref FTKPlayerID reference = ref array[num2++];
                    reference = item2.m_Pid;
                }
                EncounterSessionMC.Instance.m_FightOrder = list2;
                _this.UpdateAttackTimeline2(dictionary, array, _cfsm);
            }
            else
            {
                _orig(_this, _attacked, _foe, _activeTimePortraitID, _cfsm);
            }
        }

        private List<EncounterSessionMC.FightOrderEntry> TimelineSpacingHook(On.EncounterSession.orig_AddTimelineSpacing _orig, List<EncounterSessionMC.FightOrderEntry> _fos, bool _keepFirst, bool _isFinal = false)
        {
            
            List<EncounterSessionMC.FightOrderEntry> list = FTKUtil.DupFightOrderList(_fos);
            float combatTimelineSpacing = VisualParams.Instance.m_CombatTimelineSpacing;
            float num = 0f - combatTimelineSpacing;
            for (int i = 0; i < list.Count; i++)
            {
                if (i == 0 && _keepFirst || list[i] is APIFightOrderEntry)
                {
                    Logger.LogWarning("Found a Custom fight order entry");
                    list[i].m_TTA = combatTimelineSpacing;
                }
                else
                {
                    list[i].m_TTA = num + combatTimelineSpacing;
                }
                num = list[i].m_TTA;
            }
            if (_isFinal)
            {
                for (int j = 1; j < list.Count; j++)
                {
                    list[j].m_TTA += VisualParams.Instance.m_CombatTimelineFirstOffset;
                }
            }
            return list;
        }

        private List<FightOrderEntry> ComputeFightOrderHook(On.EncounterSessionMC.orig_ComputeFightOrder _orig, EncounterSessionMC _this, FTKPlayerID[] _fightOrder, FTKPlayerID _firstAttacker, Dictionary<FTKPlayerID, float> _speedTable)
        {
            List<FightOrderEntry> orig = _orig(_this, _fightOrder, _firstAttacker, _speedTable);
            int i = 0;
            foreach(CharacterDummy dummy in BattleAPI.Instance.SpecialCombatActions.Keys)
            {
                if (i == 0)
                {
                    _this.m_ActiveTimePortraitID++;
                    //dummy.m_TimeToAttack = 0f;
                    orig.Insert(0, new APIFightOrderEntry(dummy.m_CharacterOverworld.m_FTKPlayerID, _this.m_ActiveTimePortraitID) { m_TTA = 0 });
                    orig.Sort();
                    i++;
                    continue;
                }
                dummy.AddProfToDummy(new FTK_proficiencyTable.ID[] { FTK_proficiencyTable.ID.musicRush }, true, true);
            }
            return orig;
        }

        private void InitTimelineHook(On.EncounterSession.orig_InitAttackTimeline _orig, EncounterSession _this, EncounterSessionMC.FightOrderEntry[] _fightOrder, ExitGames.Client.Photon.Hashtable _initialTTA, ContinueFSM _cfsm)
        {
            List<EncounterSessionMC.FightOrderEntry> newOrder = _fightOrder.ToList();
            foreach (CharacterDummy dummy in BattleAPI.Instance.SpecialCombatActions.Keys)
            {
                newOrder.Insert(0, new APIFightOrderEntry(dummy.m_CharacterOverworld.m_FTKPlayerID, EncounterSessionMC.Instance.m_ActiveTimePortraitID++) { m_TTA = 0 });
                newOrder.Sort();
            }
            _orig(_this, newOrder.ToArray(), _initialTTA, _cfsm);
        }

        private void EncounterFinishedHook(On.EncounterSessionMC.orig_FinalEncounterFinished _orig, EncounterSessionMC _this, bool _encounterVictory, bool _callFromDialog)
        {
            _orig(_this, _encounterVictory, _callFromDialog);
            BattleAPI.Instance.EndOfBattle();
        }

        private void EncounterStartHook(On.EncounterSession.orig_StartEncounterSession_Actual _orig, EncounterSession _this, string _ackID, int _masterSeed, FTKPlayerID _currentPlayer, FTKPlayerID[] _players, string _diorama, EncounterSessionMC.EncounterLocation _encounterLocation, HexLandID[] _hexIDs, string _dioramaExtra, MiniHexDungeon.EncounterType _encounterType, int _encounterContext, string[] _enemyTypes, OnlineText _lootTile, OnlineText _lootMsg, HexLandID _boatHex, MiniHexDungeon.EncounterType[] _encounterTypes, int[] _bossIndices, byte[] _engageEvents)
        {
            _orig(_this, _ackID, _masterSeed, _currentPlayer, _players, _diorama, _encounterLocation, _hexIDs, _dioramaExtra, _encounterType, _encounterContext, _enemyTypes, _lootTile, _lootMsg, _boatHex, _encounterTypes, _bossIndices, _engageEvents);
            BattleAPI.Instance.StartOfBattle();
        }
    }
}
