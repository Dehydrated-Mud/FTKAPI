using FTKAPI.Managers;
using GridEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using FTKAPI.Utils;
using Logger = FTKAPI.Utils.Logger;
using myStitcher = FTKAPI.Utils.Stitcher;
using HutongGames.PlayMaker.Actions;

namespace FTKAPI.Objects
{
    public class CustomSkinset : FTK_skinset
    {
        internal FTK_skinset.ID myBase = FTK_skinset.ID.woodcutter_Female;
        internal string PLUGIN_ORIGIN = "null";

        public CustomSkinset(ID baseSkinset = FTK_skinset.ID.woodcutter_Female)
        {
            myBase = baseSkinset;
            var source = SkinsetManager.GetSkinset(baseSkinset);
            foreach (FieldInfo field in typeof(FTK_skinset).GetFields())
            {
                //FTKAPI.Utils.Logger.LogMessage("Setting field: " + field.Name + " to value: " + field.GetValue(source));
                field.SetValue(this, field.GetValue(source));
            }
        }
        public CharacterEventListener MakeAvatar(GameObject prefab)
        {
            Logger.LogWarning(prefab is null);
            myStitcher stitcher = new myStitcher();
            GameObject sourceRenderer = null;
            GameObject Avatar = UnityEngine.Object.Instantiate(SkinsetManager.GetSkinset(myBase).m_Avatar.gameObject); //Clone base avatar and set it as my new avatar
            Logger.LogWarning(Avatar.name);
            GameObject source = UnityEngine.Object.Instantiate(prefab);
            Transform sourceHairBottom = null;
            Transform sourceHairTop = null;
            Logger.LogWarning("We have initialized, now attempting to find the hairs...");
            try
            {
                sourceRenderer = source.transform.Children().Where(x => x.name.Contains("player")).ToList()[0].gameObject;
                sourceHairBottom = source.transform.Children().Where(x => x.name.Contains("hairBottom")).ToList()[0];
                sourceHairTop = source.transform.Children().Where(x => x.name.Contains("hairTop")).ToList()[0];
            }
            catch(ArgumentOutOfRangeException) 
            {
                Logger.LogWarning(String.Format("No hairBottom and or hairTop objects found for prefab {{}}, defaulting to base avatar's hair.", prefab.name));
            }

            Logger.LogWarning("Attempting to find destination objects...");
            GameObject destRenderer = Avatar.transform.Children().Where(x => x.name.Contains("player")).ToList()[0].gameObject;
            GameObject destRendererHairBottom = Avatar.transform.Children().Where(x => x.name.Contains("hairBottom")).ToList()[0].gameObject;
            GameObject destRendererHairTop = Avatar.transform.Children().Where(x => x.name.Contains("hairTop")).ToList()[0].gameObject;

            Logger.LogWarning("Attempting to destroy destination objects...");
            if (destRenderer == null)
            {
                Logger.LogError(String.Format("Found no Player (model) in target avatar."));
            }
            else
            {
                UnityEngine.Object.Destroy(destRenderer);
            }

            if (destRendererHairBottom == null)
            {
                Logger.LogError(String.Format("Found no HairTop in target avatar."));
            }
            else
            {
                UnityEngine.Object.Destroy(destRendererHairBottom);
            }
            if (destRendererHairTop == null)
            {
                Logger.LogError(String.Format("Found no HairBottom in target avatar."));
            }
            else
            {
                UnityEngine.Object.Destroy(destRendererHairTop);
            }
            
            if (sourceRenderer == null)
            {
                Logger.LogError(String.Format("Found no SkinnedMeshRenderers in 'player' transform of prefab {{}} ", prefab.name));
            }
            else
            {
                Logger.LogWarning("Attempting to stitch player...");
                stitcher.Stitch(sourceRenderer, Avatar);
            }

            if (sourceHairTop.gameObject != null)
            {
                Logger.LogWarning(sourceHairTop.transform.name);
                SkinnedMeshRenderer topRenderer = sourceHairTop.GetComponent<SkinnedMeshRenderer>();
                if (topRenderer == null)
                {
                    Logger.LogWarning(String.Format("You have a hairBottom transform in your prefab {{}}, but it has no skinnedmeshrenderer component!", prefab.name));
                }
                Logger.LogWarning("Attempting to stitch top hair...");
                stitcher.Stitch(sourceHairTop.gameObject, Avatar);
            }

            Logger.LogWarning("Attempting to stitch bottom hair...");
            if (sourceHairBottom != null)
            {
                Logger.LogWarning(sourceHairBottom.transform.name);
                SkinnedMeshRenderer bottomRenderer = sourceHairBottom.GetComponent<SkinnedMeshRenderer>();
                if (bottomRenderer == null)
                {
                    Logger.LogWarning(String.Format("You have a hairBottom transform in your prefab {{}}, but it has no skinnedmeshrenderer component!", prefab.name));
                }
                Logger.LogWarning("Attempting to add component to bottom hair...");
                sourceHairBottom.gameObject.AddComponent<WillRenderNotifier>();
                sourceHairBottom.gameObject.GetComponent<WillRenderNotifier>().m_Avatar = Avatar.GetComponent<CharacterEventListener>();
                stitcher.Stitch(sourceHairBottom.gameObject, Avatar);
            }

            Logger.LogWarning("Destroying source!");
            UnityEngine.Object.Destroy(source);
            return Avatar.GetComponent<CharacterEventListener>();
        }
        public CharacterEventListener MakeAvatar(ID skinsetID)
        {
            return SkinsetManager.GetSkinset(skinsetID).m_Avatar;
        }

        public Armor MakeArmor(GameObject prefab)
        {
            GameObject armor = UnityEngine.Object.Instantiate(prefab);
            armor.AddComponent<Armor>();
            return armor.GetComponent<Armor>();
        }
        public Armor MakeArmor(ID skinsetID)
        {
            return SkinsetManager.GetSkinset(skinsetID).m_Armor;
        }

        public Helmet MakeHelmet(GameObject prefab)
        {
            GameObject helmet = UnityEngine.Object.Instantiate(prefab);
            helmet.AddComponent<Helmet>();
            helmet.GetComponent<Helmet>().m_IsHairBottomOn = true;
            return helmet.GetComponent<Helmet>();
        }

        public Helmet MakeHelmet(ID skinsetID)
        {
            return SkinsetManager.GetSkinset(skinsetID).m_Helmet;
        }

        public GameObject MakeBoots(GameObject prefab)
        {
            return prefab;
        }

        public GameObject MakeBoots(ID skinsetID)
        {
            return SkinsetManager.GetSkinset(skinsetID).m_Boot;
        }

        public GameObject MakeBackpack(GameObject prefab)
        {
            GameObject backpack = UnityEngine.Object.Instantiate(prefab);
            backpack.AddComponent<BoxCollider>();

            backpack.AddComponent<Rigidbody>();
            backpack.GetComponent<Rigidbody>().isKinematic = true;
            backpack.GetComponent<Rigidbody>().drag = 0.1f;
            backpack.GetComponent<Rigidbody>().angularDrag = 0.15f;

            backpack.AddComponent<Detachable>();
            backpack.GetComponent<Detachable>().m_DetachVelScale = 0.2f;
            return backpack;
        }

        public GameObject MakeBackpack(ID skinsetID)
        {
            return SkinsetManager.GetSkinset(skinsetID).m_Backpack;
        }

        public new string ID
        {
            get => this.m_ID;
            set => this.m_ID = value;
        }

        public CharacterEventListener Avatar
        {
            get => this.m_Avatar;
            set => this.m_Avatar = value;
        }

        public Armor Armor
        {
            get => this.m_Armor;
            set=> this.m_Armor= value;
        }

        public bool NoHelmet
        {
            get => this.m_NoHelmet;
            set => this.m_NoHelmet = value;
        }

        public Helmet Helmet
        {
            get => this.m_Helmet;
            set => this.m_Helmet = value;
        }

        public GameObject Backpack
        {
            get => this.m_Backpack;
            set => this.m_Backpack = value;
        }

        public GameObject Boot
        {
            get => this.m_Boot;
            set => this.m_Boot = value;
        }
    }
}
