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
        }

        private int GetSellItemValueHook(On.uiPopupMenu.orig_GetSellItemValue orig, uiPopupMenu self, FTK_itembase.ID _item)
        {
            MiniHexInfo pOI = self.m_Cow.GetPOI();
            Logger.LogWarning("pOI: " + pOI);
            Logger.LogWarning("Is weaponStats contain item? " + FTK_weaponStats2DB.GetDB().IsContain(_item));
            Logger.LogWarning("Item is: " + _item);
            Logger.LogWarning((int)_item);
            if (ItemManager.Instance.customDictionary.TryGetValue((int)_item, out CustomItem customItem))
            {
                return FTKUtil.RoundToInt((float)customItem.GoldValue * GameFlow.Instance.GameDif.m_ItemSellValue);
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
        }
    }
}
