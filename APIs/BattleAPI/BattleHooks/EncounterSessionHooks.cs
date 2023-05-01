using FTKAPI.Objects.SkillHooks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Logger = FTKAPI.Utils.Logger;
using FightOrderEntry = EncounterSessionMC.FightOrderEntry;

namespace FTKAPI.APIs.BattleAPI.BattleHooks
{
    public class EncounterSessionHooks : BaseModule
    {
        public override void Initialize()
        {
            Unload();
            On.EncounterSessionMC.ComputeFightOrder += ComputeFightOrderHook;
            On.EncounterSessionMC.FinalEncounterFinished += EncounterFinishedHook;
            On.EncounterSession.StartEncounterSession_Actual += EncounterStartHook;
        }

        private List<FightOrderEntry> ComputeFightOrderHook(On.EncounterSessionMC.orig_ComputeFightOrder _orig, EncounterSessionMC _this, FTKPlayerID[] _fightOrder, FTKPlayerID _firstAttacker, Dictionary<FTKPlayerID, float> _speedTable)
        {
            List<FightOrderEntry> orig = _orig(_this, _fightOrder, _firstAttacker, _speedTable);
            foreach(CharacterDummy dummy in BattleAPI.Instance.SpecialCombatActions.Keys)
            {
                orig.Insert(0, new FightOrderEntry(dummy.m_CharacterOverworld.m_FTKPlayerID, _this.m_ActiveTimePortraitID++));
            }
            return orig;
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
