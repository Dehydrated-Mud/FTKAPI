using GridEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Logger = FTKAPI.Utils.Logger;
namespace FTKAPI.Objects.SkillHooks
{
    internal class HookRollSlots : BaseModule
    {
        public override void Initialize()
        {
            On.uiEncounterSlots.RollSlots += RollSlotsHook;

        }
        public void RollSlotsHook(On.uiEncounterSlots.orig_RollSlots _orig, uiEncounterSlots _self, FTK_slotOutput.ID _output, FTK_weaponStats2.SkillType _skill, CharacterOverworld _cow)
        {
            Logger.LogWarning("Hooking RollSlots");
            ApplySkills(_cow, FTKAPI_CharacterSkill.TriggerType.RollSlots);
            _orig(_self, _output, _skill, _cow);
        }
        public override void Unload()
        {
            On.uiEncounterSlots.RollSlots -= RollSlotsHook;
        }
    }
}

