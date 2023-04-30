using GridEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTKAPI.Objects.SkillHooks
{
    internal class HookSetDevoteCount : BaseModule
    {
        public override void Initialize()
        {
            On.MiniHexSanctum.GetDevoteCount += GetDevoteCountHook;
            On.MiniHexSanctum.SetDevoteCount += SetDevoteCountHook;   
        }

        public void SetDevoteCountHook(On.MiniHexSanctum.orig_SetDevoteCount _orig, FTK_sanctumStats.ID _id, int _int) 
        {
            if (FTK_sanctumStatsDB.Get(_id) is CustomSanctumStats)
            {
                return;
            }
            _orig(_id, _int);
        }

        public int GetDevoteCountHook(On.MiniHexSanctum.orig_GetDevoteCount _orig, FTK_sanctumStats.ID _id)
        {
            if (FTK_sanctumStatsDB.Get(_id) is CustomSanctumStats)
            {
                return 0;
            }
            return _orig(_id);
        }
    }
}
