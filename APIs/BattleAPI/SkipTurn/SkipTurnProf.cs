using FTKAPI.Objects;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTKAPI.APIs.BattleAPI
{
    public class SkipTurnProf : CustomProficiency
    {
        public SkipTurnProf() 
        {
            Target = CharacterDummy.TargetType.None;
            TargetFriendly = true;
            Category = Category.Cure;
            ProficiencyPrefab = this;
            SlotOverride = 1;
            PerSlotSkillRoll = 1f;
            //BattleButton = CommunityDLC.assetBundleIcons.LoadAsset<Sprite>("Assets/Icons/weaponBlade.png");
            Name = new("Pass");
            ID = "DeclineSpecialActionProf";
            //Tint = new Color(.8f, .6f, 1f, 1f);
            Harmless = true;
            IsEndOnTurn = true;
        }

        public override void AddToDummy(CharacterDummy _dummy)
        {
            return;
        }

        public override void End(CharacterDummy _dummy)
        {
            return;
        }
    }
}
