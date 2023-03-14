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
using FTKAPI.Managers;
using FTKAPI.Objects.SkillHooks;

using Logger = FTKAPI.Utils.Logger;
using UnityEngine;
using GridEditor;

namespace FTKAPI.Objects.SkinHooks
{
    internal class HookUpdateHelmet : BaseModule
    {
        public override void Initialize()
        {
            Unload();
            IL.CharacterEventListener.UpdateHelmet += UpdateHelmetHook;
        }
        private static void UpdateHelmetHook(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.GotoNext(
                x => x.MatchLdloc(0),
                x => x.MatchCallOrCallvirt<GameObject>("get_transform"),
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CharacterEventListener>("m_TargetHair")
                ); 

            c.Index += 6;
            c.RemoveRange(14);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<GameObject,CharacterEventListener>>((gameObject,_this) =>
            {
                if (_this.m_CharacterOverworld != null)
                {
                    FTK_playerGameStart _player = ClassManager.GetClass(_this.m_CharacterOverworld.m_CharacterStats.m_CharacterClass);
                    Logger.LogWarning(_player.GetType());
                    if (_player is CustomClass)
                    {
                        CustomClass _player1 = (CustomClass)_player;
                        if (!_player1.m_DefaultHeadSize)
                        {
                            UpdateNotNormal(gameObject);
                            return;
                        }
                    }
                }
                else if(_this.m_uiQuickPlayerCreate != null)
                {
                    FTK_playerGameStart _player =  _this.m_uiQuickPlayerCreate.GetClassDBEntry();
                    if (_player is CustomClass)
                    {
                        CustomClass _player1 = (CustomClass)_player;
                        if (!_player1.m_DefaultHeadSize) 
                        {
                            UpdateNotNormal(gameObject);
                            return;
                        }
                    }
                }
                UpdateNormal(gameObject);
            });
        }
        public override void Unload()
        {
            IL.CharacterEventListener.UpdateHelmet -= UpdateHelmetHook;
        }

        internal static void UpdateNormal(GameObject _gameObject)
        {
            _gameObject.transform.localPosition = Vector3.zero;
            _gameObject.transform.localRotation = Quaternion.Euler(-90f, 90f, 0f);
            _gameObject.transform.localScale = Vector3.one;
        }

        internal static void UpdateNotNormal(GameObject _gameObject)
        {
            _gameObject.transform.localPosition = new Vector3(0.125f, 0.0f, 0.0f);
            _gameObject.transform.localRotation = Quaternion.Euler(-90f, 90f, 0f);
            _gameObject.transform.localScale = new Vector3(0.83f, 0.83f, 0.83f);
        }
    }
}
