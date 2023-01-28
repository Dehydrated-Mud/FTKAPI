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
using UnityEngine;
using Logger = FTKAPI.Utils.Logger;

namespace FTKAPI.Managers;

public class SkinsetManager : BaseManager<SkinsetManager>
{
    public int successfulLoads = 0;
    public Dictionary<string, int> enums = new();
    public Dictionary<int, CustomSkinset> customDictionary = new();
    public Dictionary<int, CustomSkinset> moddedDictionary = new();

    internal override void Init()
    {
        Plugin.Instance.Harmony.PatchNested<HarmonyPatches>();
    }

    public static FTK_skinset GetSkinset(FTK_skinset.ID id)
    {
        FTK_skinsetDB playerSkinsetsDB = TableManager.Instance.Get<FTK_skinsetDB>();
        return playerSkinsetsDB.m_Array[(int)id];
    }

    public static int AddSkinset(CustomSkinset customSkinset, BaseUnityPlugin plugin = null)
    {
        if (plugin != null) customSkinset.PLUGIN_ORIGIN = plugin.Info.Metadata.GUID;

        SkinsetManager skinsetManager = SkinsetManager.Instance;
        TableManager tableManager = TableManager.Instance;

        try
        {
            skinsetManager.enums.Add(customSkinset.m_ID,
                (int)Enum.GetValues(typeof(FTK_skinset.ID)).Cast<FTK_skinset.ID>().Max() + (skinsetManager.successfulLoads + 1)
            );
            skinsetManager.customDictionary.Add(skinsetManager.enums[customSkinset.m_ID], customSkinset);
            GEDataArrayBase geDataArrayBase = tableManager.Get(typeof(FTK_skinsetDB));
            if (!geDataArrayBase.IsContainID(customSkinset.m_ID))
            {
                geDataArrayBase.AddEntry(customSkinset.m_ID);
                ((FTK_skinsetDB)geDataArrayBase).m_Array[tableManager.Get<FTK_skinsetDB>().m_Array.Length - 1] = customSkinset;
                geDataArrayBase.CheckAndMakeIndex();
            }

            // sometimes the object does not get added into the dictionary if initialize was called more than once, this ensures it does
            if (!((FTK_skinsetDB)geDataArrayBase).m_Dictionary.ContainsKey(tableManager.Get<FTK_skinsetDB>().m_Array.Length - 1))
            {
                ((FTK_skinsetDB)geDataArrayBase).m_Dictionary.Add(tableManager.Get<FTK_skinsetDB>().m_Array.Length - 1, customSkinset);
            }

            skinsetManager.successfulLoads++;
            Logger.LogInfo($"Successfully added skinset '{customSkinset.ID}' from {customSkinset.PLUGIN_ORIGIN}");
            return skinsetManager.enums[customSkinset.m_ID];
        }
        catch (Exception e)
        {
            Logger.LogError(e);
            Logger.LogError($"Failed to add skinset '{customSkinset.ID}' from {customSkinset.PLUGIN_ORIGIN}");
            return -1;
        }
    }

    public static void ModifySkinset(FTK_skinset.ID id, CustomSkinset customSkinset)
    {
        FTK_skinsetDB playerSkinsetDB = TableManager.Instance.Get<FTK_skinsetDB>();
        playerSkinsetDB.m_Array[(int)id] = customSkinset;
        playerSkinsetDB.m_Dictionary[(int)id] = customSkinset;
        SkinsetManager.Instance.moddedDictionary.Add((int)id, customSkinset);
        Logger.LogInfo($"Successfully modified skinset '{id}'");
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
                Logger.LogInfo("Preparing to load custom skinsets");
                SkinsetManager.Instance.successfulLoads = 0;
                SkinsetManager.Instance.enums.Clear();
                SkinsetManager.Instance.customDictionary.Clear();
                SkinsetManager.Instance.moddedDictionary.Clear();
            }
        }

        [HarmonyPatch(typeof(FTK_skinsetDB), "GetEntry")]
        // ReSharper disable once InconsistentNaming
        class FTK_skinsetDB_GetEntry_Patch
        {
            static bool Prefix(ref FTK_skinset __result, FTK_skinset.ID _enumID)
            {
                if (SkinsetManager.Instance.customDictionary.TryGetValue((int)_enumID, out CustomSkinset customSkinset))
                {
                    __result = customSkinset;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(FTK_skinsetDB), "GetIntFromID")]
        // ReSharper disable once InconsistentNaming
        class FTK_skinsetDB_GetIntFromID_Patch
        {
            static bool Prefix(ref int __result, FTK_skinsetDB __instance, string _id)
            {
                //Attempts to return our enum and calls the original function if it errors.
                if (SkinsetManager.Instance.enums.ContainsKey(_id))
                {
                    __result = SkinsetManager.Instance.enums[_id];
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(FTK_skinset), "GetEnum")]
        // ReSharper disable once InconsistentNaming
        class FTK_skinset_GetEnum_Patch
        {
            static bool Prefix(ref FTK_skinset.ID __result, string _id)
            {
                //Not 100% sure if this is required.
                if (SkinsetManager.Instance.enums.ContainsKey(_id))
                {
                    __result = (FTK_skinset.ID)SkinsetManager.Instance.enums[_id];
                    return false;
                }
                return true;
            }
        }

    }
}


