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

public class SkillManager : BaseManager<SkillManager>
{
    public int successfulLoads = 0;
    public Dictionary<string, int> enums = new();
    public Dictionary<int, CustomSkill> customDictionary = new();
    public Dictionary<int, CustomSkill> moddedDictionary = new();

    internal override void Init()
    {
        Plugin.Instance.Harmony.PatchNested<HarmonyPatches>();
    }

    public static FTK_characterSkill GetSkill(FTK_characterSkill.ID id)
    {
        FTK_characterSkillDB playerSkillDB = TableManager.Instance.Get<FTK_characterSkillDB>();
        return playerSkillDB.m_Array[(int)id];
    }

    public static int AddSkill(CustomSkill customSkill, BaseUnityPlugin plugin = null)
    {
        if (plugin != null) customSkill.PLUGIN_ORIGIN = plugin.Info.Metadata.GUID;

        SkillManager SkillManager = SkillManager.Instance;
        TableManager tableManager = TableManager.Instance;

        try
        {
            SkillManager.enums.Add(customSkill.m_ID,
                (int)Enum.GetValues(typeof(FTK_characterSkill.ID)).Cast<FTK_characterSkill.ID>().Max() + (SkillManager.successfulLoads + 1)
            );
            SkillManager.customDictionary.Add(SkillManager.enums[customSkill.m_ID], customSkill);
            GEDataArrayBase geDataArrayBase = tableManager.Get(typeof(FTK_characterSkillDB));
            if (!geDataArrayBase.IsContainID(customSkill.m_ID))
            {
                geDataArrayBase.AddEntry(customSkill.m_ID);
                ((FTK_characterSkillDB)geDataArrayBase).m_Array[tableManager.Get<FTK_characterSkillDB>().m_Array.Length - 1] = customSkill;
                geDataArrayBase.CheckAndMakeIndex();
            }

            // sometimes the object does not get added into the dictionary if initialize was called more than once, this ensures it does
            if (!((FTK_characterSkillDB)geDataArrayBase).m_Dictionary.ContainsKey(tableManager.Get<FTK_characterSkillDB>().m_Array.Length - 1))
            {
                ((FTK_characterSkillDB)geDataArrayBase).m_Dictionary.Add(tableManager.Get<FTK_characterSkillDB>().m_Array.Length - 1, customSkill);
            }

            SkillManager.successfulLoads++;
            Logger.LogInfo($"Successfully added skill '{customSkill.ID}' from {customSkill.PLUGIN_ORIGIN}");
            return SkillManager.enums[customSkill.m_ID];
        }
        catch (Exception e)
        {
            Logger.LogError(e);
            Logger.LogError($"Failed to add skill '{customSkill.ID}' from {customSkill.PLUGIN_ORIGIN}");
            return -1;
        }
    }

    public static void ModifySkill(FTK_characterSkill.ID id, CustomSkill customSkill)
    {
        FTK_characterSkillDB playerSkillDB = TableManager.Instance.Get<FTK_characterSkillDB>();
        playerSkillDB.m_Array[(int)id] = customSkill;
        playerSkillDB.m_Dictionary[(int)id] = customSkill;
        SkillManager.Instance.moddedDictionary.Add((int)id, customSkill);
        Logger.LogInfo($"Successfully modified skill '{id}'");
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
                Logger.LogInfo("Preparing to load custom skills");
                SkillManager.Instance.successfulLoads = 0;
                SkillManager.Instance.enums.Clear();
                SkillManager.Instance.customDictionary.Clear();
                SkillManager.Instance.moddedDictionary.Clear();
            }
        }

        [HarmonyPatch(typeof(FTK_characterSkillDB), "GetEntry")]
        // ReSharper disable once InconsistentNaming
        class FTK_characterSkillDB_GetEntry_Patch
        {
            static bool Prefix(ref FTK_characterSkill __result, FTK_characterSkill.ID _enumID)
            {
                if (SkillManager.Instance.customDictionary.TryGetValue((int)_enumID, out CustomSkill customSkill))
                {
                    __result = customSkill;
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(FTK_characterSkillDB), "GetIntFromID")]
        // ReSharper disable once InconsistentNaming
        class FTK_characterSkillDB_GetIntFromID_Patch
        {
            static bool Prefix(ref int __result, FTK_characterSkillDB __instance, string _id)
            {
                //Attempts to return our enum and calls the original function if it errors.
                if (SkillManager.Instance.enums.ContainsKey(_id))
                {
                    __result = SkillManager.Instance.enums[_id];
                    return false;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(FTK_characterSkill), "GetEnum")]
        // ReSharper disable once InconsistentNaming
        class FTK_characterSkill_GetEnum_Patch
        {
            static bool Prefix(ref FTK_characterSkill.ID __result, string _id)
            {
                //Not 100% sure if this is required.
                if (SkillManager.Instance.enums.ContainsKey(_id))
                {
                    __result = (FTK_characterSkill.ID)SkillManager.Instance.enums[_id];
                    return false;
                }
                return true;
            }
        }

    }
}


