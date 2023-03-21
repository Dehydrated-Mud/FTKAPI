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

public class SanctumStatsManager : BaseManager<SanctumStatsManager>
{
    public int successfulLoads = 0;
    public Dictionary<string, int> enums = new();
    public Dictionary<int, CustomSanctumStats> customDictionary = new();
    public Dictionary<int, CustomSanctumStats> moddedDictionary = new();

    internal override void Init()
    {
        Plugin.Instance.Harmony.PatchNested<HarmonyPatches>();
    }

    public static FTK_sanctumStats GetSanctum(FTK_sanctumStats.ID id)
    {
        FTK_sanctumStatsDB playerSanctumDB = TableManager.Instance.Get<FTK_sanctumStatsDB>();
        return playerSanctumDB.m_Array[(int)id];
    }

    public static int AddSanctum(CustomSanctumStats customSanctum, BaseUnityPlugin plugin = null)
    {
        if (plugin != null) customSanctum.PLUGIN_ORIGIN = plugin.Info.Metadata.GUID;

        SanctumStatsManager SanctumStatsManager = SanctumStatsManager.Instance;
        TableManager tableManager = TableManager.Instance;

        try
        {
            SanctumStatsManager.enums.Add(customSanctum.m_ID,
                (int)Enum.GetValues(typeof(FTK_sanctumStats.ID)).Cast<FTK_sanctumStats.ID>().Max() + (SanctumStatsManager.successfulLoads + 1)
            );
            SanctumStatsManager.customDictionary.Add(SanctumStatsManager.enums[customSanctum.m_ID], customSanctum);
            GEDataArrayBase geDataArrayBase = tableManager.Get(typeof(FTK_sanctumStatsDB));
            if (!geDataArrayBase.IsContainID(customSanctum.m_ID))
            {
                geDataArrayBase.AddEntry(customSanctum.m_ID);
                ((FTK_sanctumStatsDB)geDataArrayBase).m_Array[tableManager.Get<FTK_sanctumStatsDB>().m_Array.Length - 1] = customSanctum;
                geDataArrayBase.CheckAndMakeIndex();
            }

            // sometimes the object does not get added into the dictionary if initialize was called more than once, this ensures it does
            if (!(bool)((FTK_sanctumStatsDB)geDataArrayBase).m_Dictionary?.ContainsKey(tableManager.Get<FTK_sanctumStatsDB>().m_Array.Length - 1))
            {
                ((FTK_sanctumStatsDB)geDataArrayBase).m_Dictionary?.Add(tableManager.Get<FTK_sanctumStatsDB>().m_Array.Length - 1, customSanctum);
            }

            SanctumStatsManager.successfulLoads++;
            Logger.LogInfo($"Successfully added sanctum '{customSanctum.ID}' from {customSanctum.PLUGIN_ORIGIN}");
            return SanctumStatsManager.enums[customSanctum.m_ID];
        }
        catch (Exception e)
        {
            Logger.LogError(e);
            Logger.LogError($"Failed to add sanctum '{customSanctum.ID}' from {customSanctum.PLUGIN_ORIGIN}");
            return -1;
        }
    }

    public static void ModifySanctum(FTK_sanctumStats.ID id, CustomSanctumStats customSanctum)
    {
        FTK_sanctumStatsDB playerSanctumDB = TableManager.Instance.Get<FTK_sanctumStatsDB>();
        playerSanctumDB.m_Array[(int)id] = customSanctum;
        //null
        //playerSanctumDB.m_Dictionary[(int)id] = customSanctum;
        SanctumStatsManager.Instance.moddedDictionary.Add((int)id, customSanctum);
        Logger.LogInfo($"Successfully modified sanctum '{id}'");
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
                Logger.LogInfo("Preparing to load custom sanctums");
                SanctumStatsManager.Instance.successfulLoads = 0;
                SanctumStatsManager.Instance.enums.Clear();
                SanctumStatsManager.Instance.customDictionary.Clear();
                SanctumStatsManager.Instance.moddedDictionary.Clear();
            }
        }

        [HarmonyPatch(typeof(FTK_sanctumStatsDB), "GetEntry")]
        // ReSharper disable once InconsistentNaming
        class FTK_sanctumStatsDB_GetEntry_Patch
        {
            static bool Prefix(ref FTK_sanctumStats __result, FTK_sanctumStats.ID _enumID)
            {
                if (SanctumStatsManager.Instance.customDictionary.TryGetValue((int)_enumID, out CustomSanctumStats customSanctum))
                {
                    __result = customSanctum;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(FTK_sanctumStatsDB), "GetIntFromID")]
        // ReSharper disable once InconsistentNaming
        class FTK_sanctumStatsDB_GetIntFromID_Patch
        {
            static bool Prefix(ref int __result, FTK_sanctumStatsDB __instance, string _id)
            {
                //Attempts to return our enum and calls the original function if it errors.
                if (SanctumStatsManager.Instance.enums.ContainsKey(_id))
                {
                    __result = SanctumStatsManager.Instance.enums[_id];
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(FTK_sanctumStats), "GetEnum")]
        // ReSharper disable once InconsistentNaming
        class FTK_sanctumStats_GetEnum_Patch
        {
            static bool Prefix(ref FTK_sanctumStats.ID __result, string _id)
            {
                //Not 100% sure if this is required.
                if (SanctumStatsManager.Instance.enums.ContainsKey(_id))
                {
                    __result = (FTK_sanctumStats.ID)SanctumStatsManager.Instance.enums[_id];
                    return false;
                }
                return true;
            }
        }

    }
}


