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

namespace FTKAPI.Objects
{
    public class CustomSkinset : FTK_skinset
    {
        internal FTK_skinset.ID myBase = FTK_skinset.ID.woodcutter_Male;
        internal string PLUGIN_ORIGIN = "null";

        public CustomSkinset(ID baseSkinset = FTK_skinset.ID.woodcutter_Male)
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
            SkinnedMeshRenderer sourceRenderer = null;
            CharacterEventListener Avatar = UnityEngine.Object.Instantiate(SkinsetManager.GetSkinset(myBase).m_Avatar); //Clone base avatar and set it as my new avatar
            Transform parentObjectDest = Avatar.transform;
            GameObject source = UnityEngine.Object.Instantiate(prefab);
            Transform sourceHairBottom = null;
            Transform sourceHairTop = null;
            /*Vector3 localPosition = source.transform.localPosition;
            Quaternion localRotation = source.transform.localRotation;

            source.transform.parent = parentObjectDest.transform;

            source.transform.localPosition = localPosition;
            source.transform.localRotation = localRotation;*/
            try
            {
                sourceHairBottom = source.transform.Children().Where(x => x.name.Contains("hairBottom")).ToList()[0];
                sourceHairTop = source.transform.Children().Where(x => x.name.Contains("hairTop")).ToList()[0];
            }
            catch(ArgumentOutOfRangeException) 
            {
                Logger.LogWarning(String.Format("No hairBottom and or hairTop objects found for prefab {{}}, defaulting to base avatar's hair.", prefab.name));
            }
            
            List<Transform> possibleTransforms = source.transform.Children().Where(x => x.name.Contains("player")).ToList();

            if (possibleTransforms.Any())
            {
                if (possibleTransforms.Count > 1)
                {
                    Logger.LogError(String.Format("Found multiple transforms in prefab {{}} that begin with 'player'", prefab.name));
                }

                else
                {
                    sourceRenderer = possibleTransforms[0].GetComponent<SkinnedMeshRenderer>();
                }
            }
            else
            {
                Logger.LogError(String.Format("Found no transforms in prefab {{}} that begin with 'player'", prefab.name));
            }

            SkinnedMeshRenderer destRenderer = parentObjectDest.transform.Children().Where(x => x.name.Contains("player")).ToList()[0].GetComponent<SkinnedMeshRenderer>();
            SkinnedMeshRenderer destRendererHairBottom = parentObjectDest.transform.Children().Where(x => x.name.Contains("hairBottom")).ToList()[0].GetComponent<SkinnedMeshRenderer>();
            SkinnedMeshRenderer destRendererHairTop = parentObjectDest.transform.Children().Where(x => x.name.Contains("hairTop")).ToList()[0].GetComponent<SkinnedMeshRenderer>();

            if (sourceRenderer == null)
            {
                Logger.LogError(String.Format("Found no SkinnedMeshRenderers in 'player' transform of prefab {{}} ", prefab.name));
            }
            else
            {
                Logger.LogInfo("setting mesh to: " + sourceRenderer.sharedMesh.name);
                //destRenderer.bones = sourceRenderer.bones;
                //destRenderer.rootBone = parentObjectDest.transform.Find("Root_M");
                destRenderer.sharedMesh = sourceRenderer.sharedMesh;
                Dictionary<int, Material> univOrder = new Dictionary<int, Material>();
                List<Material> newSharedMaterials = new List<Material>(); 

                for (int j = 0; j < destRenderer.materials.Length; j++)
                {
                    string text = destRenderer.materials[j].name;
                    if (text.Contains("_uncolored"))
                    {
                        univOrder.Add(0,destRenderer.materials[j]);
                    }
                    else if (text.Contains("_skin"))
                    {
                        univOrder.Add(1, destRenderer.materials[j]);
                    }
                    else if (text.Contains("_hair"))
                    {
                        univOrder.Add(2, destRenderer.materials[j]);
                    }
                }                
                
                for (int j = 0; j < sourceRenderer.materials.Length; j++)
                {
                    string text = sourceRenderer.materials[j].name;
                    if (text.Contains("_uncolored"))
                    {
                        newSharedMaterials.Add(UnityEngine.Object.Instantiate(univOrder[0]));
                    }
                    else if (text.Contains("_skin"))
                    {
                        newSharedMaterials.Add(UnityEngine.Object.Instantiate(univOrder[1]));
                    }
                    else if (text.Contains("_hair"))
                    {
                        newSharedMaterials.Add(UnityEngine.Object.Instantiate(univOrder[2]));
                    }
                }
                destRenderer.sharedMaterials = newSharedMaterials.ToArray();
                foreach (Material mat in destRenderer.sharedMaterials)
                {
                    mat.mainTexture = sourceRenderer.sharedMaterial.mainTexture;
                }
                Avatar.RefreshRenderers();
            }

            if (sourceHairBottom != null)
            {
                SkinnedMeshRenderer bottomRenderer = sourceHairBottom.GetComponent<SkinnedMeshRenderer>();
                if (bottomRenderer == null)
                {
                    Logger.LogError(String.Format("You have a hairBottom transform in your prefab {{}}, but it has no skinnedmeshrenderer component!", prefab.name));
                }
                destRendererHairBottom.sharedMesh = bottomRenderer.sharedMesh;
                destRendererHairBottom.sharedMaterial.mainTexture = bottomRenderer.sharedMaterial.mainTexture;
            }

            if (sourceHairTop != null)
            {
                SkinnedMeshRenderer topRenderer = sourceHairTop.GetComponent<SkinnedMeshRenderer>();
                if (topRenderer == null)
                {
                    Logger.LogError(String.Format("You have a hairBottom transform in your prefab {{}}, but it has no skinnedmeshrenderer component!", prefab.name));
                }
                destRendererHairTop.sharedMesh = topRenderer.sharedMesh;
                destRendererHairTop.sharedMaterial.mainTexture = topRenderer.sharedMaterial.mainTexture;
            }

            UnityEngine.Object.Destroy(source);
            return Avatar;
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
