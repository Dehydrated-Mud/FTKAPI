using FTKAPI.Objects.SkillHooks;
using Logger = FTKAPI.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GridEditor;
using FTKAPI.Managers;
using UnityEngine;

namespace FTKAPI.APIs.BattleAPI.BattleHooks
{
    public class CharacterDummyHooks : BaseModule
    {
        public override void Initialize()
        {
            On.CharacterDummy._beginActualEngageAttack += OnStartCombatTurn;
            On.CharacterDummy.ActionCompleted += FinishedAttackHook;
            //On.CharacterDummy.GetHitEffect += HitEffectHook;
            On.CharacterEventListener.HookupWeaponForBattle += AnimatorHook;
        }

        private void AnimatorHook(On.CharacterEventListener.orig_HookupWeaponForBattle _orig, CharacterEventListener _this)
        {
            _orig(_this);
            Logger.LogWarning("Active battle skill is: " + BattleAPI.Instance.ActiveBattleSkill);
            if (BattleAPI.Instance.ActiveBattleSkill != FTK_proficiencyTable.ID.None)
            {
                if (BattleAPI.Instance.ActiveBattleSkills.ContainsKey(BattleAPI.Instance.ActiveBattleSkill))
                {
                    string animator = BattleAPI.Instance.ActiveBattleSkills[BattleAPI.Instance.ActiveBattleSkill].AnimatorOverride;
                    Logger.LogWarning("Animator is: " + animator);
                    if (animator != "")
                    {
                        Animator component = _this.GetComponent<Animator>();
                        RuntimeAnimatorController runtimeAnimatorController = AssetManager.GetAnimationControllers<Weapon>().Find(i => i.name == animator);
                        if (runtimeAnimatorController != null)
                        {
                            component.runtimeAnimatorController = runtimeAnimatorController;
                        }
                        else
                        {
                            Logger.LogError($"Desired animator {animator} was not found!");
                        }

                    }
                }
            }
        }

        private HitEffect HitEffectHook(On.CharacterDummy.orig_GetHitEffect _orig, CharacterDummy _this, DummyDamageInfo _ddi)
        {
            HitEffect orig = _orig(_this, _ddi);
            if (BattleAPI.Instance.ActiveBattleSkills.ContainsKey(_ddi.m_Prof))
            {
                if (BattleAPI.Instance.ActiveBattleSkills[_ddi.m_Prof].HitEffectOverride != FTK_hitEffect.ID.None)
                {
                    return TableManager.Instance.Get<FTK_hitEffectDB>().GetEntry(BattleAPI.Instance.ActiveBattleSkills[_ddi.m_Prof].HitEffectOverride).m_Prefab;
                }
            }
            return orig;
        }

        private void OnStartCombatTurn(On.CharacterDummy.orig__beginActualEngageAttack _orig, CharacterDummy _this)
        {
            //Logger.LogWarning("This should show at the start of every combat turn!");
            BattleAPI.Instance.CombatStartTurn(_this);
            ApplySkills(_this.m_CharacterOverworld, Objects.FTKAPI_CharacterSkill.TriggerType.StartCombatTurn);
            _orig(_this);
        }

        private void FinishedAttackHook(On.CharacterDummy.orig_ActionCompleted _orig, CharacterDummy _this)
        {
            CombatFloat combatFloat = new CombatFloat //Even though this says damage reflection, its actually just post attack in general
            {
                Value = _this.m_PostAttackHealthMod,
                SetFloat = SetFloats.DamageReflection
            };
            CombatFloat lifestealFac = new CombatFloat
            {
                Value = 0,
                SetFloat = SetFloats.LifestealFac
            };
            int postAttack = FTKUtil.RoundToInt(BattleAPI.Instance.GetFloat(_this, combatFloat));
            Logger.LogWarning("Post Attack mod:" + postAttack);
            if (postAttack < 0)
            {
                postAttack = (int)((BattleAPI.Instance.GetFloat(_this, lifestealFac) + 1f)*(float)postAttack);
            }
            Logger.LogWarning("Post Attack mod:" + postAttack);
            _this.m_PostAttackHealthMod = postAttack;
            _orig(_this);
            BattleAPI.Instance.ApplyProfsToAttacker(_this);
        }
    }
}
