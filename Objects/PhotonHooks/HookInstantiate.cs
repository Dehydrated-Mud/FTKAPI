using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FTKAPI.Objects.SkillHooks;
using FTKAPI.Objects;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using ExitGames.Client.Photon;
using Logger = FTKAPI.Utils.Logger;
namespace FTKAPI.PhotonHooks
{
    internal class HookInstantiate : BaseModule
    {
        public override void Initialize()
        {
            Unload();
            //IL.PhotonNetwork.Instantiate_string_Vector3_Quaternion_byte_ObjectArray += InstantiateHook;
            //On.NetworkingPeer.DoInstantiate += DoInstantiateHook;
        }
        private void InstantiateHook(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdarg(0),
                x => true,
                x => true,
                x => true,
                x => true,
                x => x.MatchStloc(0)
                );
            c.Index += 6;
            c.Emit(OpCodes.Ldloc_0);
            c.EmitDelegate<Action<GameObject>>((_value) =>
            {
                _value.AddComponent<CustomCharacterStats>();
            });
        }

        private GameObject DoInstantiateHook(On.NetworkingPeer.orig_DoInstantiate orig, PhotonPeer self, Hashtable evData, PhotonPlayer photonPlayer, GameObject resourceGameObject)
        {
            GameObject gameObject = orig(self,evData,photonPlayer,resourceGameObject);
            if (gameObject.GetComponent<CharacterOverworld>())
            {
                Logger.LogWarning("Instantiating character prefab. Does the gameObject already have a customcharacterstats component? " + (bool)gameObject.GetComponent<CustomCharacterStats>());
                if (gameObject.GetComponent<CustomCharacterStats>() == null)
                {
                    gameObject.AddComponent<CustomCharacterStats>();
                }
            }
            return gameObject;
        }

        public override void Unload()
        {
            IL.PhotonNetwork.Instantiate_string_Vector3_Quaternion_byte_ObjectArray -= InstantiateHook;
            On.NetworkingPeer.DoInstantiate -= DoInstantiateHook;
        }
    }
}
