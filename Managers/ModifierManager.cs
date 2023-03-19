using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using FTKAPI.Objects;
using FTKAPI.Utils;
using FTKItemName;
using GridEditor;
using HarmonyLib;
using Photon;
using UnityEngine;
using Logger = FTKAPI.Utils.Logger;

namespace FTKAPI.Managers;

public class ModifierManager : BaseManager<ModifierManager>
{
    public int successfulLoads = 0;
    public Dictionary<string, int> enums = new();
    public Dictionary<int, CustomModifier> customDictionary = new();
    public Dictionary<int, CustomModifier> moddedDictionary = new();

    internal override void Init()
    {
        Plugin.Instance.Harmony.PatchNested<HarmonyPatches>();
    }

    public static FTK_characterModifier GetModifier(FTK_characterModifier.ID id)
    {
        FTK_characterModifierDB playerModifierDB = TableManager.Instance.Get<FTK_characterModifierDB>();
        return playerModifierDB.m_Array[(int)id];
    }

    public static int AddModifier(CustomModifier customModifier, BaseUnityPlugin plugin = null)
    {
        if (plugin != null) customModifier.PLUGIN_ORIGIN = plugin.Info.Metadata.GUID;

        ModifierManager ModifierManager = ModifierManager.Instance;
        TableManager tableManager = TableManager.Instance;

        try
        {
            ModifierManager.enums.Add(customModifier.m_ID,
                (int)Enum.GetValues(typeof(FTK_characterModifier.ID)).Cast<FTK_characterModifier.ID>().Max() + (ModifierManager.successfulLoads + 1)
            );
            ModifierManager.customDictionary.Add(ModifierManager.enums[customModifier.m_ID], customModifier);
            GEDataArrayBase geDataArrayBase = tableManager.Get(typeof(FTK_characterModifierDB));
            if (!geDataArrayBase.IsContainID(customModifier.m_ID))
            {
                geDataArrayBase.AddEntry(customModifier.m_ID);
                ((FTK_characterModifierDB)geDataArrayBase).m_Array[tableManager.Get<FTK_characterModifierDB>().m_Array.Length - 1] = customModifier;
                geDataArrayBase.CheckAndMakeIndex();
            }

            // sometimes the object does not get added into the dictionary if initialize was called more than once, this ensures it does
            if (!((FTK_characterModifierDB)geDataArrayBase).m_Dictionary.ContainsKey(tableManager.Get<FTK_characterModifierDB>().m_Array.Length - 1))
            {
                ((FTK_characterModifierDB)geDataArrayBase).m_Dictionary.Add(tableManager.Get<FTK_characterModifierDB>().m_Array.Length - 1, customModifier);
            }

            ModifierManager.successfulLoads++;
            Logger.LogInfo($"Successfully added modifier '{customModifier.ID}' from {customModifier.PLUGIN_ORIGIN}");
            return ModifierManager.enums[customModifier.m_ID];
        }
        catch (Exception e)
        {
            Logger.LogError(e);
            Logger.LogError($"Failed to add modifier '{customModifier.ID}' from {customModifier.PLUGIN_ORIGIN}");
            return -1;
        }
    }

    public static void ModifyModifier(FTK_characterModifier.ID id, CustomModifier customModifier)
    {
        FTK_characterModifierDB playerModifierDB = TableManager.Instance.Get<FTK_characterModifierDB>();
        playerModifierDB.m_Array[(int)id] = customModifier;
        playerModifierDB.m_Dictionary[(int)id] = customModifier;
        ModifierManager.Instance.moddedDictionary.Add((int)id, customModifier);
        Logger.LogInfo($"Successfully modified modifier '{id}'");
    }

    /// <summary>
    ///    <para>The patches necessary to make adding custom items possible.</para>
    ///    <para>Many of them are used to fix issues calling an invalid 'FTK_playerGameStart.ID' 
    ///    by substituting with 'ClassManager.enums' dictionary value.</para>
    /// </summary>
    class HarmonyPatches
    {
        [HarmonyPatch(typeof(TableManager), "Initialize")]
        // ReSharper disable once InconsistentNaming
        class TableManager_Initialize_Patch
        {
            static void Prefix()
            {
                Logger.LogInfo("Preparing to load custom modifiers");
                ModifierManager.Instance.successfulLoads = 0;
                ModifierManager.Instance.enums.Clear();
                ModifierManager.Instance.customDictionary.Clear();
                ModifierManager.Instance.moddedDictionary.Clear();
            }
        }

        [HarmonyPatch(typeof(FTK_characterModifierDB), "GetEntry")]
        // ReSharper disable once InconsistentNaming
        class FTK_characterModifierDB_GetEntry_Patch
        {
            static bool Prefix(ref FTK_characterModifier __result, FTK_characterModifier.ID _enumID)
            {
                if (ModifierManager.Instance.customDictionary.TryGetValue((int)_enumID, out CustomModifier customModifier))
                {
                    __result = customModifier;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(FTK_characterModifierDB), "GetIntFromID")]
        // ReSharper disable once InconsistentNaming
        class FTK_characterModifierDB_GetIntFromID_Patch
        {
            static bool Prefix(ref int __result, FTK_characterModifierDB __instance, string _id)
            {
                //Attempts to return our enum and calls the original function if it errors.
                if (ModifierManager.Instance.enums.ContainsKey(_id))
                {
                    __result = ModifierManager.Instance.enums[_id];
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(FTK_characterModifier), "GetEnum")]
        // ReSharper disable once InconsistentNaming
        class FTK_characterModifier_GetEnum_Patch
        {
            static bool Prefix(ref FTK_characterModifier.ID __result, string _id)
            {
                //Not 100% sure if this is required.
                if (ModifierManager.Instance.enums.ContainsKey(_id))
                {
                    __result = (FTK_characterModifier.ID)ModifierManager.Instance.enums[_id];
                    return false;
                }
                return true;
            }
        }

    }
}


