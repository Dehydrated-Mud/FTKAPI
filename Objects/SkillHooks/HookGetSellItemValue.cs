using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using IL;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Logger = FTKAPI.Utils.Logger;
using UnityEngine;
using GridEditor;
using FTKAPI.Managers;

namespace FTKAPI.Objects.SkillHooks
{
    internal class HookGetSellItemValue : BaseModule
    {
        public override void Initialize()
        {
            On.uiPopupMenu.GetSellItemValue += GetSellItemValueHook;
            On.uiBuyMenuHud.GetShopItemCost += GetShopItemCostHook;
        }

        private int GetShopItemCostHook(On.uiBuyMenuHud.orig_GetShopItemCost orig, uiBuyMenuHud self, FTK_itembase.ID _item)
        {
            MiniHexInfo pOI = self.m_CurrentCow.GetPOI();
            if (ItemManager.Instance.customDictionary.TryGetValue((int)_item, out CustomItem customItem))
            {
                if (customItem.IsWeapon)
                {
                    return customItem.weaponDetails.GetCost(self.m_CurrentCow, pOI);
                }
                return customItem.itemDetails.GetCost(self.m_CurrentCow, pOI);
            }
            if (FTK_weaponStats2DB.GetDB().IsContain(_item)) // This is true on fresh start false otherwise? Yes
            {
                return FTK_weaponStats2DB.Get(_item).GetCost(self.m_CurrentCow, pOI);
            }
            if (FTK_itemsDB.Get(_item) != null)
            {
                return FTK_itemsDB.Get(_item).GetCost(self.m_CurrentCow, pOI);
            }
            return 100;
        }

        private int GetSellItemValueHook(On.uiPopupMenu.orig_GetSellItemValue orig, uiPopupMenu self, FTK_itembase.ID _item)
        {
            MiniHexInfo pOI = self.m_Cow.GetPOI();
            if (ItemManager.Instance.customDictionary.TryGetValue((int)_item, out CustomItem customItem))
            {
                if (customItem.IsWeapon)
                {
                    return customItem.weaponDetails.GetSellValue(self.m_Cow, pOI);
                }
                return customItem.itemDetails.GetSellValue(self.m_Cow, pOI);
            }
            if (FTK_weaponStats2DB.GetDB().IsContain(_item)) // This is true on fresh start false otherwise? Yes
            {
                return FTK_weaponStats2DB.Get(_item).GetSellValue(self.m_Cow, pOI);
            }
            if (FTK_itemsDB.Get(_item) != null)
            {
                return FTK_itemsDB.Get(_item).GetSellValue(self.m_Cow, pOI);
            }
            return 0;
        }

        public override void Unload()
        {
            On.uiPopupMenu.GetSellItemValue -= GetSellItemValueHook;
            On.uiBuyMenuHud.GetShopItemCost -= GetShopItemCostHook;
        }
    }
}
