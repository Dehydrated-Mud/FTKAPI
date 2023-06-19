using FTKAPI.Objects.SkillHooks;
using FTKAPI.Utils;
using GridEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTKAPI.APIs.BattleAPI.BattleHooks
{
    public class GameLogicHooks : BaseModule
    {
        public override void Initialize()
        {
            On.GameLogic.FillLootDropList += LootHook;
        }

        private void LootHook(On.GameLogic.orig_FillLootDropList _orig, ArrayList _arrayList, int _playerCount, FTK_enemyCombat.ItemDrops[] _itemDrops, int[] _itemDropLevels, RewardData _rewards, ref int _perPlayerGold, ref int _perPlayerXP, bool _endDungeon)
        {
            Logger.LogInfo("Number of additional drops: " + BattleAPI.Instance.ItemsToAdd.Count);
            if(BattleAPI.Instance.ItemsToAdd.Count > 0)
            {
                foreach(FTK_itembase.ID item in BattleAPI.Instance.ItemsToAdd)
                {
                    _rewards.AddItem(item);
                }
            }

            if(BattleAPI.Instance.BonusXp.Keys.Count > 0)
            {
                foreach(CharacterOverworld player in BattleAPI.Instance.BonusXp.Keys)
                {
                    player.m_CharacterStats.UpdateXP(BattleAPI.Instance.BonusXp[player], true, true);
                }
            }

            if (BattleAPI.Instance.BonusGold.Keys.Count > 0)
            {
                foreach (CharacterOverworld player in BattleAPI.Instance.BonusGold.Keys)
                {
                    player.m_CharacterStats.ChangeGold(BattleAPI.Instance.BonusGold[player]);
                }
            }

            _orig(_arrayList, _playerCount, _itemDrops, _itemDropLevels, _rewards, ref _perPlayerGold, ref _perPlayerXP, _endDungeon);
        }
    }
}
