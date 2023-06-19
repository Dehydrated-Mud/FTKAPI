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
using HutongGames.PlayMaker.Actions;

namespace FTKAPI.Objects
{

    public class CustomTier : FTK_progressionTier
    {
        internal string PLUGIN_ORIGIN = "null";

        public CustomTier(FTK_progressionTier.ID baseTier = FTK_progressionTier.ID.None)
        {
            if (baseTier != FTK_progressionTier.ID.None)
            {
                var source = TierManager.GetTier(baseTier);
                foreach (FieldInfo field in typeof(FTK_progressionTier).GetFields())
                {
                    field.SetValue(this, field.GetValue(source));
                }
            }
        }
        public new string ID
        {
            get => this.m_ID;
            set => this.m_ID = value;
        }

        public int ItemLevel
        {
            get => this.m_ItemLevel;
            set => this.m_ItemLevel = value;
        }

    }
}
