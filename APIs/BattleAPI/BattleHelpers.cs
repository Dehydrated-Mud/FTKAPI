using FTKAPI.Objects;
using GridEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Logger = FTKAPI.Utils.Logger;
using ProfValues = uiBattleStanceButtons.ProfValues;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.ParticleSystem;

namespace FTKAPI.APIs.BattleAPI
{
    public class BattleHelpers
    {
        internal static void QuerySkills(CharacterOverworld player, Query query)
        {
            CustomCharacterSkills skills = ValidateSkills(player.m_CharacterStats.m_CharacterSkills);
            if (skills != null && skills.Skills.Count > 0)
            {
                foreach (FTKAPI_CharacterSkill skill in skills.Skills)
                {
                    skill.Skill(player, query);
                }
            }
            else
            {
                Logger.LogWarning($"Battle API could not query skills of player {player.m_FTKPlayerID} because they do not have a CustomCharacterSkills object.");
            }
        }

        internal static CustomCharacterSkills ValidateSkills(CharacterSkills skills) // Issue here?
        {
            if (skills is CustomCharacterSkills)
            {
                return (CustomCharacterSkills)skills;
            }
            return null;
        }


        internal static ProfValues MakeBattleButton(CharacterOverworld _player, ProfInfoContainer _info, uiBattleStanceButtons _battleStance, Weapon _weapon)
        {
            ProfValues item = default(ProfValues);
            item.m_Prof = _info.AttackProficiency;
            item.m_Button = UnityEngine.Object.Instantiate(_battleStance.m_ProficiencyButtonMaster);
            item.m_Button.transform.SetParent(_battleStance.m_ProficiencyButtonMaster.transform.parent, worldPositionStays: false);
            item.m_Button.gameObject.SetActive(value: true);
            item.m_Button.gameObject.GetComponent<Image>().sprite = FTK_proficiencyTableDB.GetDB().GetEntry(_info.AttackProficiency).m_BattleButton;
            //bool canUse = !FTK_proficiencyTableDB.GetDB().GetEntry(_info.AttackProficiency).m_GunShot || !_needsReload; // This might be something we have to deal with in order to get
            // Get rid of the default focus tooltip as it is only visible when the player has focus. Replace with custom tooltip
            UnityEngine.Object.Destroy(item.m_Button.gameObject.GetComponent<uiToolTipFocusable>());
            uiToolTipFocusable tooltip = item.m_Button.gameObject.AddComponent<APIuiToolTipFocusable>();
            tooltip.m_ReturnRawInfo = true;
            tooltip.m_Info = "Special Skill";
            if (_info.IsCoolingDown)
            {
                tooltip.m_DetailInfo += FTKUI.GetKeyInfoRichText(Color.magenta, false, "On cool down.") + Environment.NewLine;
            }
            string tooltipReq;
            bool canUse = CanUseSpecialProf(_player, _info, _weapon, out tooltipReq);
            tooltip.m_DetailInfo += tooltipReq.ToString().TrimEnd(Environment.NewLine.ToCharArray());
            item.m_Button.SetCanUse(canUse && !_info.IsCoolingDown);
            return item;
        }

        /// <summary>
        /// Check that all the requirements from the special prof info are met by the character's current loadout
        /// </summary>
        /// <param name="_player"></param>
        /// <param name="_info"></param>
        /// <param name="_weapon"></param>
        /// <returns></returns>
        internal static bool CanUseSpecialProf(CharacterOverworld _player, ProfInfoContainer _info, Weapon _weapon, out string _tooltip)
        {
            FTK_weaponStats2 weaponStats =  FTK_weaponStats2DB.Get(_player.m_WeaponID);
            bool flag = false;
            bool canUse = true;
            string myToolTip = "";
            Color goodColor = new Color(.7f, 1, .7f);
            Color badColor = VisualParams.Instance.m_ColorTints.m_BadStatColor;
            if (_info.CheckWeaponSubType)
            {
                flag = (_weapon.m_WeaponSubType == _info.WeaponSubType);
                myToolTip += FTKUI.GetKeyInfoRichText(flag? goodColor:badColor, _bold: !flag , $"Must be using a {_info.WeaponSubType} type weapon.") + Environment.NewLine;
                canUse &= flag;
            }
            if (_info.WeaponType != Weapon.WeaponType.none)
            {
                flag = (_weapon.m_WeaponType == _info.WeaponType);
                myToolTip += FTKUI.GetKeyInfoRichText(flag ? goodColor : badColor, _bold: !flag, $"Must be using a {_info.WeaponType}") + Environment.NewLine;
                canUse &= flag;
            }
            if (_info.SkillType != FTK_weaponStats2.SkillType.none)
            {
                flag = (weaponStats._skilltest == _info.SkillType);
                myToolTip += FTKUI.GetKeyInfoRichText(flag ? goodColor : badColor, _bold: !flag, $"Must be using a {_info.SkillType} weapon.") + Environment.NewLine;
                canUse &= flag;
            }
            if (_info.WeaponSlot != ProfInfoContainer.WeaponGrip.Any)
            {
                if (weaponStats.m_ObjectSlot == FTK_itembase.ObjectSlot.oneHand)
                {
                    flag = (_info.WeaponSlot == ProfInfoContainer.WeaponGrip.OneHanded);
                    myToolTip += FTKUI.GetKeyInfoRichText(flag ? goodColor : badColor, _bold: !flag, "Must be using a one handed weapon.") + Environment.NewLine;
                    canUse &= flag;
                }
                else if (weaponStats.m_ObjectSlot == FTK_itembase.ObjectSlot.twoHands)
                {
                    flag = (_info.WeaponSlot == ProfInfoContainer.WeaponGrip.TwoHanded);
                    myToolTip += FTKUI.GetKeyInfoRichText(flag ? goodColor : badColor, _bold: !flag, "Must be using a two handed weapon.") + Environment.NewLine;
                    canUse &= flag;
                }
            }
            if (_info.DamageType != FTK_weaponStats2.DamageType.none)
            {
                flag = (weaponStats._dmgtype == _info.DamageType);
                myToolTip += FTKUI.GetKeyInfoRichText(flag ? goodColor : badColor, _bold: !flag, $"Must be using a weapon that does {_info.DamageType} damage.") + Environment.NewLine;
                canUse &= flag;
            }
            if (_info.NeedsShield)
            {
                flag = (_player.m_ShieldID != FTK_itembase.ID.None);
                myToolTip += FTKUI.GetKeyInfoRichText(flag ? goodColor : badColor, _bold: !flag, "Must have a shield equipped.") + Environment.NewLine;
                canUse &= flag;
            }
            _tooltip = myToolTip;
            return canUse;
        }

        /// <summary>
        /// Gets the current dummy's CharacterEventListner if in combat and the Avatar's CharacterEventListner if in the overworld.
        /// </summary>
        /// <param name="_player"></param>
        /// <returns></returns>
        public static CharacterEventListener GetListener(CharacterOverworld _player)
        {
            
            CharacterEventListener ret = null;
            if (_player.GetCurrentDummy())
            {
                ret = _player.GetCurrentDummy().m_EventListener;
            }
            else
            {
                try
                {
                    ret = _player.m_Avatar;
                }
                catch (Exception e)
                {
                    Logger.LogError(e.Message);
                    Logger.LogError(e.StackTrace);
                }
            }
            return ret;
        }

        internal static FTK_weaponStats2.SkillType SkillOverride(FTK_weaponStats2.SkillType _orig, FTK_proficiencyTable _table)
        {
            FTK_proficiencyTable.ID _iD = TryGetID(_table);
            if (BattleAPI.Instance.ActiveBattleSkills.ContainsKey(_iD))
            {
                Logger.LogInfo("API has key");
                ProfInfoContainer container = BattleAPI.Instance.ActiveBattleSkills[_iD];
                if (container.OverrideSkillCheck != FTK_weaponStats2.SkillType.none && !container.ByPassSlots)
                {
                    Logger.LogInfo("Attempting to override skillcheck");
                    return container.OverrideSkillCheck;
                }
            }
            return _orig;
        }
        internal static FTK_weaponStats2.SkillType SkillOverride(FTK_weaponStats2.SkillType _orig, FTK_proficiencyTable.ID _iD)
        {
            if (BattleAPI.Instance.ActiveBattleSkills.ContainsKey(_iD))
            {
                Logger.LogInfo("API has key");
                ProfInfoContainer container = BattleAPI.Instance.ActiveBattleSkills[_iD];
                if (container.OverrideSkillCheck != FTK_weaponStats2.SkillType.none && !container.ByPassSlots)
                {
                    Logger.LogInfo("Attempting to override skillcheck");
                    return container.OverrideSkillCheck;
                }
            }
            return _orig;
        }
        internal static string SkillOverride2(string _orig, FTK_proficiencyTable _table)
        {
            FTK_proficiencyTable.ID _iD = TryGetID(_table);
            if (BattleAPI.Instance.ActiveBattleSkills.ContainsKey(_iD))
            {
                Logger.LogInfo("API has key");
                ProfInfoContainer container = BattleAPI.Instance.ActiveBattleSkills[_iD];
                if (container.OverrideSkillCheck != FTK_weaponStats2.SkillType.none && !container.ByPassSlots)
                {
                    Logger.LogInfo("Attempting to override skillcheck");
                    return container.OverrideSkillCheck.ToString();
                }
            }
            return _orig;
        }
        internal static string SkillOverride2(string _orig, FTK_proficiencyTable.ID _iD)
        {
            if (BattleAPI.Instance.ActiveBattleSkills.ContainsKey(_iD))
            {
                Logger.LogInfo("API has key");
                ProfInfoContainer container = BattleAPI.Instance.ActiveBattleSkills[_iD];
                if (container.OverrideSkillCheck != FTK_weaponStats2.SkillType.none && !container.ByPassSlots)
                {
                    Logger.LogInfo("Attempting to override skillcheck");
                    return container.OverrideSkillCheck.ToString();
                }
            }
            return _orig;
        }

        internal static FTK_proficiencyTable.ID TryGetID(FTK_proficiencyTable _table)
        {
            if (Managers.ProficiencyManager.Instance.enums.ContainsKey(_table.m_ID))
            {
                return (FTK_proficiencyTable.ID)Managers.ProficiencyManager.Instance.enums[_table.m_ID];
            }
            return FTK_proficiencyTable.GetEnum(_table.m_ID);
        }

        public static Weapon GetWeapon(CharacterOverworld _cow)
        {
            return GetListener(_cow)?.m_Weapon;
        }

    }
}
