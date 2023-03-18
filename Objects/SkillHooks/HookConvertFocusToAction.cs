using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Logger = FTKAPI.Utils.Logger;

namespace FTKAPI.Objects.SkillHooks
{
    internal class HookConvertFocusToAction : BaseModule
    {
        public override void Initialize()
        {
            On.Movement.ConvertFocusToAction += ConvertFocusToActionHook;
        }
        public void ConvertFocusToActionHook(On.Movement.orig_ConvertFocusToAction _orig, Movement _self) 
        {
            Logger.LogWarning("Hooking convert focus");
            ApplySkills(_self.m_CharacterOverworld, FTKAPI_CharacterSkill.TriggerType.ConvertFocusToAction);
            _orig(_self);
        }
        public override void Unload()
        {
            On.Movement.ConvertFocusToAction += ConvertFocusToActionHook;
        }
    }
}
