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
    public class CustomSanctumStats : FTK_sanctumStats
    {
        internal FTK_sanctumStats.ID myBase = FTK_sanctumStats.ID.Sanctum05;
        internal string PLUGIN_ORIGIN = "null";

        public CustomSanctumStats(ID baseSanctum = FTK_sanctumStats.ID.Sanctum05)
        {
            myBase = baseSanctum;
            var source = SanctumStatsManager.GetSanctum(baseSanctum);
            foreach (FieldInfo field in typeof(FTK_sanctumStats).GetFields())
            {
                //FTKAPI.Utils.Logger.LogMessage("Setting field: " + field.Name + " to value: " + field.GetValue(source));
                field.SetValue(this, field.GetValue(source));
            }
        }

        public FTK_sanctumStats.ID BaseSanctum
        {
            get => myBase;
            set => myBase = value;   
        }
        public new string ID
        {
            get => this.m_ID;
            set => this.m_ID = value;
        }
        public SpawnOption Spawn
        {
            get => this.m_Spawn;
            set => this.m_Spawn = value;
        }
        public GameObject ArtAsset
        {
            get => this.m_ArtAsset;
            set => this.m_ArtAsset = value;
        }
        public GameObject ArtAssetGrand
        {
            get => this.m_ArtAssetGrand;
            set => this.m_ArtAssetGrand = value;
        }
        public Sprite SanctumIcon
        {
            get => this.m_SanctumIcon; 
            set => this.m_SanctumIcon = value;
        }
        public Color SanctumColor
        {
            get => this.m_SanctumColor; set => this.m_SanctumColor = value;
        }
        public Color SanctumInactiveColor
        {
            get => this.SanctumInactiveColor; set => this.SanctumInactiveColor = value;
        }
        public Color SanctumLightColor
        {
            get => this.m_SanctumLightColor; set => this.m_SanctumLightColor = value;
        }
        public bool Ignore
        {
            get => this.m_Ignore; set => this.m_Ignore = value;
        }
        public AkEventID SoundID
        {
            get => this.m_SoundID;
            set => this.m_SoundID = value;
        }
        public FTK_statistic.ID Stat = FTK_statistic.ID.None;
    }
}
