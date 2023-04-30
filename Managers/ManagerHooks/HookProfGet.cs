using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FTKAPI.Objects.SkillHooks;
using Logger = FTKAPI.Utils.Logger;
using GridEditor;
using FTKAPI.Objects;

internal class HookProfGet : BaseModule
{
    public override void Initialize()
    {
        Unload();
        On.ProficiencyManager.Get += HookProfManagerGet;
        
    }

    public ProficiencyBase HookProfManagerGet(On.ProficiencyManager.orig_Get _orig, ProficiencyManager _self, GridEditor.FTK_proficiencyTable.ID _pid)
    {
        if (FTKAPI.Managers.ProficiencyManager.Instance.customDictionary.TryGetValue((int)_pid, out CustomProficiency customProf))
        {
            return customProf;
        }         
        else
        {
            return _orig(_self, _pid);
        }
    }

    public override void Unload()
    {
        On.ProficiencyManager.Get -= HookProfManagerGet;
    }
}

