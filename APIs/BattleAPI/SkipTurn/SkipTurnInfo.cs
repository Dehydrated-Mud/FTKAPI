using FTKAPI.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GridEditor;
using IL;

namespace FTKAPI.APIs.BattleAPI
{
    using ProficiencyManager = FTKAPI.Managers.ProficiencyManager;
    internal class SkipTurnInfo : ProfInfoContainer
    {
        internal SkipTurnInfo() 
        {
            CategoryDescription = new CustomLocalizedString("Skip Turn").GetLocalizedString();
            AttackAnim = CharacterDummy.AttackAnim.DirectAttack;
            OverrideAttackAnim = true;
            AttackProficiency = (FTK_proficiencyTable.ID)ProficiencyManager.Instance.enums["DeclineSpecialActionProf"];
        }
    }
}
