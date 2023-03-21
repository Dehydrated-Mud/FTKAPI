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
    public class CustomModifier : FTK_characterModifier
    {
        internal FTK_characterModifier.ID myBase = FTK_characterModifier.ID.amuletScare;
        internal string PLUGIN_ORIGIN = "null";

        public CustomModifier(ID baseModifier = FTK_characterModifier.ID.amuletScare)
        {
            myBase = baseModifier;
            var source = ModifierManager.GetModifier(baseModifier);
            foreach (FieldInfo field in typeof(FTK_characterModifier).GetFields())
            {
                field.SetValue(this, field.GetValue(source));
            }
        }
        public virtual void AddStatModifierToTally(CharacterStats _player)
        {
            _player.m_ModAttackPhysical += 0;
            _player.m_ModAttackMagic += 0;
            _player.m_ModAttackAll += 0;
            _player.m_ModCritChance += 0f;
            _player.m_ReflectDamage += 0;
            _player.m_ModToughness += 0f;
            _player.m_ModAwareness += 0f;
            _player.m_ModFortitude += 0f;
            _player.m_ModQuickness += 0f;
            _player.m_ModTalent += 0f;
            _player.m_ModVitality += 0f;
            _player.m_ModLuck += 0.1f;
            _player.m_ModFindRadius += 0;
            _player.m_ModShopPrice += 0f;
            _player.m_ModExtraActions += 0;
            _player.m_ModMaxFocus += 0;
            _player.m_ModMaxHealth += 0;
            _player.m_ModHealthRegen += 0;
            _player.m_ModXp += 0f;
            _player.m_ModGold += 0f;
        }
        public FTK_characterModifier.ID BaseModifier
        {
            get => myBase;
            set => myBase = value;
        }
        public new string ID
        {
            get => this.m_ID;
            set => this.m_ID = value;
        }
        public int DefensePhysical
        {
            get => this.m_ModDefensePhysical;
            set => this.m_ModDefensePhysical = value;
        }
        public int DefenseMagic
        {
            get => this.m_ModDefenseMagic;
            set => this.m_ModDefenseMagic = value;
        }
        public float EvadeRating
        {
            get => this.m_ModEvadeRating;
            set => this.m_ModEvadeRating = value;
        }
        public int PartyArmor
        {
            get => this.m_PartyCombatArmor;
            set => this.m_PartyCombatArmor = value;
        }
        public int PartyResist
        {
            get => this.m_PartyCombatResist;
            set => this.m_PartyCombatResist = value;
        }
        public float PartyEvade
        {
            get => this.m_PartyCombatEvade;
            set => this.m_PartyCombatEvade = value;
        }
        public int ReflectDamage
        {
            get => this.m_ReflectDamage;
            set => this.m_ReflectDamage = value;
        }
        public int PhysicalDamage
        {
            get => this.m_ModAttackPhysical;
            set => this.m_ModAttackPhysical = value;
        }
        public int MagicDamage
        {
            get => this.m_ModAttackMagic;
            set => this.m_ModAttackMagic = value;
        }
        public int AllDamage
        {
            get => this.m_ModAttackAll;
            set => this.m_ModAttackAll = value;
        }
        public float CritChance
        {
            get => this.m_ModCritChance;
            set => this.m_ModCritChance = value;
        }
        public FTK_enemyCombat.EnemyRace BonusAgainstRace1
        {
            get => this.m_BonusAgainstRace1;
            set => this.m_BonusAgainstRace1 = value;
        }
        public FTK_enemyCombat.EnemyRace BonusAgainstRace2
        {
            get => this.m_BonusAgainstRace2;
            set => this.m_BonusAgainstRace2 = value;
        }
        public float BonusAgainstRace1Value
        {
            get => this.m_BonusAgainstRace1Value;
            set => this.m_BonusAgainstRace1Value = value;
        }
        public float BonusAgainstRace2Value
        {
            get => this.m_BonusAgainstRace2Value;
            set => this.m_BonusAgainstRace2Value = value;
        }
        public int FindRadius
        {
            get => this.m_ModFindRadius;
            set => this.m_ModFindRadius = value;
        }

        public float ModShopPrice
        {
            get => this.m_ModShopPrice;
            set => this.m_ModShopPrice = value;
        }

        public int ExtraAction
        {
            get => this.m_ExtraAction;
            set => this.m_ExtraAction = value;
        }

        public int ExtraFocus
        {
            get => this.m_ExtraFocus; 
            set => this.m_ExtraFocus = value;
        }
        public int ExtraHealth
        {
            get => this.m_ExtraHealth; 
            set => this.m_ExtraHealth = value;
        }
        public int HealthRegen
        {
            get => this.m_HealthRegen;
            set => this.m_HealthRegen = value;
        }
        public float XPMultiplier
        {
            get => this.m_ModXP; set => this.m_ModXP = value;
        }
        public float GoldMultiplier
        {
            get => this.m_ModGold;
            set => this.m_ModGold = value;
        }
        public float Vitality
        {
            get=> this.m_ModVitality;
            set => this.m_ModVitality = value;
        }
        public float Strength
        {
            get => this.m_ModToughness;
            set => this.m_ModToughness = value;
        }
        public float Intelligence
        {
            get => this.m_ModFortitude; 
            set => this.m_ModFortitude = value;
        }
        public float Awareness
        {
            get => this.m_ModAwareness;
            set => this.m_ModAwareness = value;
        }
        public float Talent
        {
            get => this.m_ModTalent;
            set => this.m_ModTalent = value;
        }
        public float Speed
        {
            get => this.m_ModQuickness; set => 
            this.m_ModQuickness = value;
        }
        public float Luck
        {
            get => this.m_ModLuck; 
            set => this.m_ModLuck = value;
        }
        public bool ImmunePoison
        {
            get => this.m_ImmunePoison; 
            set => this.m_ImmunePoison = value;
        }
        public bool ImmuneCurse
        {
            get => this.m_ImmuneCurse;
            set => this.m_ImmuneCurse = value;
        }
        public bool ImmuneDisease
        {
            get => this.m_ImmuneDisease;
            set => this.m_ImmuneDisease = value;
        }
        public bool ImmuneStun
        {
            get => this.m_ImmuneStun;
            set => this.m_ImmuneStun = value;
        }
        public bool ImmuneFire
        {
            get => this.m_ImmuneFire;
            set => this.m_ImmuneFire = value;
        }
        public bool ImmuneLightning
        {
            get => this.m_ImmuneLightning;
            set => this.m_ImmuneLightning = value;
        }
        public bool ImmuneIce
        {
            get => this.m_ImmuneIce;
            set => this.m_ImmuneIce = value;
        }
        public bool ImmuneSteal
        {
            get => this.m_ImmuneSteal;
            set => this.m_ImmuneSteal = value;
        }
        public bool ImmuneAmbush
        {
            get => this.m_ImmuneAmbush;
            set => this.m_ImmuneAmbush = value;
        }
        public bool Immune
        {
            get => this.m_ImmuneConfuse;
            set => this.m_ImmuneConfuse = value;
        }
        public bool ImmuneScare
        {
            get => this.m_ImmuneScare;
            set => this.m_ImmuneScare = value;
        }
        public bool ImmuneDeathMark
        {
            get => this.m_ImmuneDeathMark;
            set => this.m_ImmuneDeathMark = value;
        }
        public bool ImmuneAcid
        {
            get => this.m_ImmuneAcid;
            set => this.m_ImmuneAcid = value;
        }
        public bool ImmuneWater
        {
            get => this.m_ImmuneWater;
            set => this.m_ImmuneWater = value;
        }
        public bool ImmunePetrify
        {
            get => this.m_ImmunePetrify;
            set => this.m_ImmunePetrify = value;
        }
        public bool PartyImmuneDisease
        {
            get => this.m_PartyImmuneDisease;
            set => this.m_PartyImmuneDisease = value;
        }
        public bool PartyImmuneStun
        {
            get => this.m_PartyImmuneStun;
            set => this.m_PartyImmuneStun = value;
        }
        public bool PartyImmuneFire
        {
            get => this.m_PartyImmuneFire;
            set => this.m_PartyImmuneFire = value;
        }
        public bool PartyImmuneLightning
        {
            get => this.m_PartyImmuneLightning;
            set => this.m_PartyImmuneLightning = value;
        }
        public bool PartyImmuneIce
        {
            get => this.m_PartyImmuneIce;
            set => this.m_PartyImmuneIce = value;
        }
        public bool PartyImmuneSteal
        {
            get => this.m_PartyImmuneSteal;
            set => this.m_PartyImmuneSteal = value;
        }

        public bool PartyImmune
        {
            get => this.m_PartyImmuneConfuse;
            set => this.m_PartyImmuneConfuse = value;
        }
        public bool PartyImmuneScare
        {
            get => this.m_PartyImmuneScare;
            set => this.m_PartyImmuneScare = value;
        }
        public bool PartyImmuneAcid
        {
            get => this.m_PartyImmuneAcid;
            set => this.m_PartyImmuneAcid = value;
        }
        public bool PartyImmuneWater
        {
            get => this.m_PartyImmuneWater;
            set => this.m_PartyImmuneWater = value;
        }
        public bool PartyImmunePetrify
        {
            get => this.m_PartyImmunePetrify;
            set => this.m_PartyImmunePetrify = value;
        }
        public CharacterSkills ModCharacterSkills
        {
            get => this.m_CharacterSkills;
            set => this.m_CharacterSkills = value;
        }
        public int MaxActions
        {
            get => this.m_MaxActions;
            set => this.m_MaxActions = value;
        }
        public int MaxHealth
        {
            get => this.m_MaxHealth; 
            set => this.m_MaxHealth = value;
        }
        public bool EnemyTarget
        {
            get => this.m_EnemyTarget;
            set => this.m_EnemyTarget = value;
        }
    }
}
