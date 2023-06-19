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

public class TierManager : BaseManager<TierManager>
{
    public int successfulLoads = 0;
    public Dictionary<string, int> enums = new();
    public Dictionary<int, CustomTier> customDictionary = new();
    public Dictionary<int, CustomTier> moddedDictionary = new();

    internal override void Init()
    {
        Plugin.Instance.Harmony.PatchNested<HarmonyPatches>();
    }

    public static FTK_progressionTier GetTier(FTK_progressionTier.ID id)
    {
        FTK_progressionTierDB playerTierDB = TableManager.Instance.Get<FTK_progressionTierDB>();
        return playerTierDB.m_Array[(int)id];
    }

    public static int AddTier(CustomTier customTier, BaseUnityPlugin plugin = null)
    {
        if (plugin != null) customTier.PLUGIN_ORIGIN = plugin.Info.Metadata.GUID;

        TierManager TierManager = TierManager.Instance;
        TableManager tableManager = TableManager.Instance;

        try
        {
            TierManager.enums.Add(customTier.m_ID,
                (int)Enum.GetValues(typeof(FTK_progressionTier.ID)).Cast<FTK_progressionTier.ID>().Max() + (TierManager.successfulLoads + 1)
            );
            TierManager.customDictionary.Add(TierManager.enums[customTier.m_ID], customTier);
            GEDataArrayBase geDataArrayBase = tableManager.Get(typeof(FTK_progressionTierDB));
            if (!geDataArrayBase.IsContainID(customTier.m_ID))
            {
                geDataArrayBase.AddEntry(customTier.m_ID);
                ((FTK_progressionTierDB)geDataArrayBase).m_Array[tableManager.Get<FTK_progressionTierDB>().m_Array.Length - 1] = customTier;
                geDataArrayBase.CheckAndMakeIndex();
            }

            // sometimes the object does not get added into the dictionary if initialize was called more than once, this ensures it does
            if (!((FTK_progressionTierDB)geDataArrayBase).m_Dictionary.ContainsKey(tableManager.Get<FTK_progressionTierDB>().m_Array.Length - 1))
            {
                ((FTK_progressionTierDB)geDataArrayBase).m_Dictionary.Add(tableManager.Get<FTK_progressionTierDB>().m_Array.Length - 1, customTier);
            }

            TierManager.successfulLoads++;
            Logger.LogInfo($"Successfully added tier '{customTier.ID}' from {customTier.PLUGIN_ORIGIN}");
            return TierManager.enums[customTier.m_ID];
        }
        catch (Exception e)
        {
            Logger.LogError(e);
            Logger.LogError($"Failed to add tier '{customTier.ID}' from {customTier.PLUGIN_ORIGIN}");
            return -1;
        }
    }

    public static void ModifyTier(FTK_progressionTier.ID id, CustomTier customTier)
    {
        FTK_progressionTierDB playerTierDB = TableManager.Instance.Get<FTK_progressionTierDB>();
        playerTierDB.m_Array[(int)id] = customTier;
        playerTierDB.m_Dictionary[(int)id] = customTier;
        TierManager.Instance.moddedDictionary.Add((int)id, customTier);
        Logger.LogInfo($"Successfully modified tier '{id}'");
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
                Logger.LogInfo("Preparing to load custom tiers");
                TierManager.Instance.successfulLoads = 0;
                TierManager.Instance.enums.Clear();
                TierManager.Instance.customDictionary.Clear();
                TierManager.Instance.moddedDictionary.Clear();
            }
        }

        [HarmonyPatch(typeof(FTK_progressionTierDB), "GetEntry")]
        // ReSharper disable once InconsistentNaming
        class FTK_progressionTierDB_GetEntry_Patch
        {
            static bool Prefix(ref FTK_progressionTier __result, FTK_progressionTier.ID _enumID)
            {
                if (TierManager.Instance.customDictionary.TryGetValue((int)_enumID, out CustomTier customTier))
                {
                    __result = customTier;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(FTK_progressionTierDB), "GetIntFromID")]
        // ReSharper disable once InconsistentNaming
        class FTK_progressionTierDB_GetIntFromID_Patch
        {
            static bool Prefix(ref int __result, FTK_progressionTierDB __instance, string _id)
            {
                //Attempts to return our enum and calls the original function if it errors.
                if (TierManager.Instance.enums.ContainsKey(_id))
                {
                    __result = TierManager.Instance.enums[_id];
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(FTK_progressionTier), "GetEnum")]
        // ReSharper disable once InconsistentNaming
        class FTK_progressionTier_GetEnum_Patch
        {
            static bool Prefix(ref FTK_progressionTier.ID __result, string _id)
            {
                //Not 100% sure if this is required.
                if (TierManager.Instance.enums.ContainsKey(_id))
                {
                    __result = (FTK_progressionTier.ID)TierManager.Instance.enums[_id];
                    return false;
                }
                return true;
            }
        }

    }
}


