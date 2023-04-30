using FTKAPI.Objects.SkillHooks;
using Logger = FTKAPI.Utils.Logger;
using TriggerType = FTKAPI.Objects.FTKAPI_CharacterSkill.TriggerType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Mono.Cecil;
using GridEditor;
using FTKAPI.Objects;
using UnityEngine;
using static uiBattleStanceButtons;

namespace FTKAPI.APIs.BattleAPI.BattleHooks
{
    internal class HookUiBattleButtons : BaseModule
    {
        // Injects proficiencies into the list when building the battle options menu
        // This allows us to give custom character skills a list of proficiencies that they add.
        // This emulates the behavior of items that give abilities like Taunt or Party Heal, which are hardcoded buttons.
        public override void Initialize()
        {
            //IL.uiBattleStanceButtons.CreateWeaponProficiencyButtons += CreateWeaponProficiencyButtonsHook;
            On.uiBattleStanceButtons.CreateWeaponProficiencyButtons += CreateWeaponProficiencyButtonsHook;
            On.uiBattleStanceButtons.DisplayBattleActionInfo += DisplayBattleActionHook;
            IL.uiBattleStanceButtons.DisplayBattleActionInfo += DisplayBattleActionILHook;
        }
        private void CreateWeaponProficiencyButtonsHook(On.uiBattleStanceButtons.orig_CreateWeaponProficiencyButtons _orig, uiBattleStanceButtons _this, Weapon _weapon, bool _needsReload)
        {
            _orig(_this, _weapon, _needsReload);
            BattleAPI.Instance.AddButtons(_this, _weapon);
        }
        private void DisplayBattleActionILHook(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(
                x => x.MatchLdloc(21),
                x => x.MatchLdarg(0),
                x => x.MatchCallOrCallvirt<uiBattleStanceButtons>("get_CombatCow"),
                x => x.MatchCallOrCallvirt<FTK_proficiencyTable>("GetBattleButtonInfo")
                ))
            {
                c.Index += 10;
                c.Emit(OpCodes.Ldloc_S, (byte)21);
                c.EmitDelegate<Func<FTK_weaponStats2.SkillType, FTK_proficiencyTable, FTK_weaponStats2.SkillType>>(BattleHelpers.SkillOverride);
            }

            if (c.TryGotoNext(
                x => x.MatchLdloc(21),
                x => x.MatchLdfld<FTK_proficiencyTable>("m_DmgTypeOverride"),
                x => x.MatchStloc(10)
                ))
            {
                c.Index += 12;
                c.Emit(OpCodes.Ldloc_S, (byte)21);
                c.EmitDelegate<Func<string, FTK_proficiencyTable, string>>(BattleHelpers.SkillOverride2);
            }
        }

        private void DisplayBattleActionHook(On.uiBattleStanceButtons.orig_DisplayBattleActionInfo _orig, uiBattleStanceButtons _this, uiBattleButton _button, bool _on)
        {
            _orig(_this, _button, _on);
            if (_button.m_ButtonType == uiBattleButton.BattleButtonType.proficiency)
            {
                FTK_proficiencyTable.ID iD = _this.m_Proficiencies.Single(p => p.m_Button == _button).m_Prof;
                if (BattleAPI.Instance.ActiveBattleSkills.ContainsKey(iD))
                {
                    ProfInfoContainer container = BattleAPI.Instance.ActiveBattleSkills[iD];
                    if (container.CategoryDescription != "")
                    {
                        Logger.LogInfo("Trying to change infopanel");
                        _this.m_InfoPanel.m_Description[1].text = container.CategoryDescription;
                    }
                }
            }
        }
        /*private void CreateWeaponProficiencyButtonsHook(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchStloc(2),
                x => x.MatchLdcI4(0),
                x => x.MatchStloc(3)
                );
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate(ProfButtonDelegate);
        }

        private List<FTK_proficiencyTable.ID> ProfButtonDelegate(List<FTK_proficiencyTable.ID> _list, Weapon weapon)
        {
            Logger.LogWarning("Attempting to add proficiencies");
            CharacterOverworld player = GameLogic.Instance.GetCurrentCombatCOW();

            List<FTK_proficiencyTable.ID> customList = new List<FTK_proficiencyTable.ID>();
            List<ProfInfoContainer> tmpList = GetProfs(player, TriggerType.ProfButton, weapon);
            if (tmpList != null && tmpList.Count > 0)
            {
                customList.AddRange(tmpList.Select(o => o.AttackProficiency).ToList());
            }
            _list.AddRange(customList);
            return _list;
        }*/
        public override void Unload()
        {

        }

    }
}
