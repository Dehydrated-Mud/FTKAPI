using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GridEditor;
using Logger = FTKAPI.Utils.Logger;
using UnityEngine;
using FTKAPI.APIs.BattleAPI;

namespace FTKAPI.Objects
{
    public class FTKAPI_CharacterSkill
    {
        public string m_ToolTip;
        public string m_DisplayName;
        public bool proc = false;
        internal FTK_characterSkill.ID m_skillInfo = FTK_characterSkill.ID.None;
        private ProfInfoContainer m_SpecialProfs = new();
        [Flags]
        public enum TriggerType : uint
        {
            None = 0,
            EndTurn = 1 << 0,
            SlotHardOverride = 1 << 1,
            SlotSoftOverride = 1 << 2,
            SlotNoOverride = 1 << 3,
            KillShot = 1 << 4,
            RespondToHit = 1 << 5,
            AnyLandedAttack = 1 << 6,
            TheyDodged = 1 << 7,
            IDodged = 1 << 8,
            TakeAnyDamage = 1 << 9,
            TakeLightDamage = 1 << 10,
            TakeHeavyDamage = 1 << 11,
            InflictAnyDamage = 1 << 12,
            InflictLightDamage = 1 << 13,
            InflictHeavyDamage = 1 << 14,
            SpecialAttackAnim = 1 << 15,
            ConvertFocusToAction = 1 << 16,
            RollSlots = 1 << 17,
            ProfButton = 1 << 18,
            AnimOverride = 1 << 19,
            StartCombatTurn = 1 << 20,
            Critical = 1 << 21,
            DamageCalcStart = 1 << 22,
            PerfectCombatRoll = 1 << 23,
            BlockedAttack = 1 << 24,
            BlockedMagicAttack = 1 << 25,
            CalledShot = 1 << 26,
            DamageCalcEnd = 1 << 27,
            SlotResults = 1 << 28,
            PerfectEncounterRoll = 1 << 29,

        }


        internal TriggerType m_TriggerType;

        public virtual void Skill(CharacterOverworld _cow, Query query) { }
        public virtual void Skill(CharacterOverworld cow, TriggerType trig, AttackAttempt atk) { }
        public virtual void Skill(CharacterOverworld _cow, TriggerType _trig) 
        {
            Logger.LogWarning("If you are seeing this you probably did not overwrite the Skill function, or overwrote the incorrect overload.");
        }
        public virtual ProfInfoContainer GetProfs(CharacterOverworld _player, TriggerType _trig, Weapon _weapon = null)
        {
            return SpecialProf;
        }

        public TriggerType Trigger
        {
            get => m_TriggerType;
            set => m_TriggerType = value;
        }

        private CustomLocalizedString name;
        public CustomLocalizedString Name
        {
            get => this.name;
            set
            {
                this.name = value;
                this.m_DisplayName = this.name.GetLocalizedString();
            }
        }
        private CustomLocalizedString description;
        public CustomLocalizedString Description
        {
            get => this.description;
            set
            {
                this.description = value;
                this.m_ToolTip = this.description.GetLocalizedString();
            }
        }
        public FTK_characterSkill.ID SkillInfo
        {
            get => this.m_skillInfo;
            set => this.m_skillInfo = value;
        }
        public ProfInfoContainer SpecialProf
        {
            get => m_SpecialProfs;
            set => m_SpecialProfs= value;
        }
    }
    /// <summary>
    /// A container with all the things you might want to override for a custom active combat skill
    /// </summary>
    public class ProfInfoContainer
    {
        /// <summary>
        /// The grip that the player's weapon must have in order to use this prof
        /// </summary>
        public enum WeaponGrip
        {
            Any,
            OneHanded,
            TwoHanded
        }
        /// <summary>
        /// If set, this will override the auto generated proficiency category description.
        /// </summary>
        public string CategoryDescription { get; set; } = "";
        
        /// <summary>
        /// Is the ability on cooldown?
        /// </summary>
        public bool IsCoolingDown { get; set; } = false;

        /// <summary>
        /// Should the special prof bypass slot roll?
        /// </summary>
        public bool ByPassSlots { get; set; } = false;

        /// <summary>
        /// Does the player need to have a shield equipped to use this skill?
        /// </summary>
        public bool NeedsShield { get; set; } = false;

        /// <summary>
        /// Should the api override the attack Anim of the character?
        /// </summary>
        public bool OverrideAttackAnim { get; set; } = false;

        /// <summary>
        /// Should the api overrid the attack animation override of the item? If false, the attack animation override will be inherited from the item.
        /// </summary>
        public bool OverrideAttackAnimOverride { get; set; } = false;

        /// <summary>
        /// Should the character's weapon have to match a specific weapon subtype?
        /// </summary>
        /// 
        public bool CheckWeaponSubType { get; set; } = false;

        /// <summary>
        /// HitEffect to override with. FTK_hitEffect.ID.None will result in no override.
        /// </summary>
        public FTK_hitEffect.ID HitEffectOverride { get; set; } = FTK_hitEffect.ID.None;

        /// <summary>
        /// The animator to override with
        /// </summary>
        public string AnimatorOverride { get; set; } = "";

        /// <summary>
        /// The proficiency itself
        /// </summary>
        /// 
        public FTK_proficiencyTable.ID AttackProficiency { get; set; } = FTK_proficiencyTable.ID.None;
        /// <summary>
        /// The damage type that the weapon must use in order to use this skill. None => any
        /// </summary>
        public FTK_weaponStats2.DamageType DamageType { get; set; } = FTK_weaponStats2.DamageType.none;

        /// <summary>
        /// The Skill check that the weapon must use in order to use this skill. None => any
        /// </summary>
        public FTK_weaponStats2.SkillType SkillType { get; set; } = FTK_weaponStats2.SkillType.none;

        /// <summary>
        /// Overrides the weapon's skill check with this skill check
        /// </summary>
        public FTK_weaponStats2.SkillType OverrideSkillCheck { get; set; } = FTK_weaponStats2.SkillType.none;

        /// <summary>
        /// The weapon slot that the character's weapon must use in order to use this skill.
        /// </summary>
        public WeaponGrip WeaponSlot { get; set; } = WeaponGrip.Any;

        /// <summary>
        /// AttackResponse to override with
        /// </summary>
        public CharacterDummy.AttackResponse AttackResponse { get; set; } = CharacterDummy.AttackResponse.None;

        /// <summary>
        /// Not implemented
        /// </summary>
        public CharacterDummy.SpecialAttack SpecialAttack { get; set; } = CharacterDummy.SpecialAttack.None;

        /// <summary>
        /// Not implemented
        /// </summary>
        public SlotControl.AttackCheatType CheatType { get; set; } = SlotControl.AttackCheatType.None;

        /// <summary>
        /// AttackAnim to override with
        /// </summary>
        public CharacterDummy.AttackAnim AttackAnim { get; set; } = CharacterDummy.AttackAnim.Attack;

        /// <summary>
        /// CombatAnimTrigger to override AttackAnimOverride with
        /// </summary>
        public CharacterEventListener.CombatAnimTrigger AttackAnimOverride { get; set; } = CharacterEventListener.CombatAnimTrigger.None;

        /// <summary>
        /// The WeaponType that must be used in order for this skill to be active. none => any
        /// </summary>
        public Weapon.WeaponType WeaponType { get; set; } = Weapon.WeaponType.none;

        /// <summary>
        /// WeaponSubType that must be used in order for this skill to be active, if CheckWeaponSubtype is true.
        /// </summary>
        public Weapon.WeaponSubType WeaponSubType { get; set; }

        /// <summary>
        /// API usage only
        /// </summary>
        public uiBattleButton BattleButton { get; set; }

        public ProfInfoContainer() { }
        public ProfInfoContainer(ProfInfoContainer _copy)
        {
            CategoryDescription = _copy.CategoryDescription;
            IsCoolingDown = _copy.IsCoolingDown;
            ByPassSlots = _copy.ByPassSlots;
            NeedsShield = _copy.NeedsShield;
            OverrideAttackAnim = _copy.OverrideAttackAnim;
            OverrideAttackAnimOverride = _copy.OverrideAttackAnimOverride;
            CheckWeaponSubType = _copy.CheckWeaponSubType;

            AttackProficiency = _copy.AttackProficiency;
            DamageType = _copy.DamageType;
            SkillType = _copy.SkillType;
            OverrideSkillCheck = _copy.OverrideSkillCheck;
            WeaponSlot = _copy.WeaponSlot;
            AttackResponse = _copy.AttackResponse;
            SpecialAttack = _copy.SpecialAttack;
            CheatType = _copy.CheatType;
            AttackAnim = _copy.AttackAnim;
            AttackAnimOverride = _copy.AttackAnimOverride;
            AnimatorOverride = _copy.AnimatorOverride;
            WeaponType = _copy.WeaponType;
            WeaponSubType = _copy.WeaponSubType;
            BattleButton = _copy.BattleButton;
        }
    }
}
