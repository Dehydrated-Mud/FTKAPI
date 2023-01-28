using FTKAPI.Managers;
using GridEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Logger = FTKAPI.Utils.Logger;
using BepInEx;
using FTKAPI.Objects;
using FTKAPI.Utils;
using HarmonyLib;

namespace FTKAPI.Managers;

public class AvatarManager : BaseManager<AvatarManager>
{
    internal string PLUGIN_ORIGIN = "null";
    GameObject prefab = null;
    public AvatarManager()
    {
    }

    public static CharacterEventListener MakeAvatar(FTK_skinset.ID baseSkinset = FTK_skinset.ID.blacksmith_Male)
    {
        CharacterEventListener baseAvatar = SkinsetManager.GetSkinset(baseSkinset).m_Avatar;
        CharacterEventListener avatar = UnityEngine.Object.Instantiate(baseAvatar);

        foreach (Transform item in avatar.transform)
        {
            SkinnedMeshRenderer component = item.GetComponent<SkinnedMeshRenderer>();
            if ((bool)component)
            {
                if (item.name != "hairBottom" && item.name != "hairTop" && (bool)prefab)
                {
                    Logger.LogInfo("Mesh made it!");
                    component.sharedMesh = Prefab.GetComponent<Mesh>();
                }
            }
        }
        return avatar;
    }
}
