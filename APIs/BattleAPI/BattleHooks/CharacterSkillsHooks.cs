using FTKAPI.Objects.SkillHooks;
using GridEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using static FullInspector.tk<T, TContext>;

namespace FTKAPI.APIs.BattleAPI.BattleHooks
{
    internal class CharacterSkillsHooks : BaseModule
    {
        public override void Initialize()
        {
            On.CharacterSkills.CalledShot += CalledShotHook;
        }

        private bool CalledShotHook(On.CharacterSkills.orig_CalledShot _orig, CharacterOverworld _player, FTK_proficiencyTable.ID _attemptedProf)
        {
            return CalledShot(_player, _attemptedProf);
        }

        private bool CalledShot(CharacterOverworld _player, FTK_proficiencyTable.ID _attemptedProf)
        {
            if (!_player.m_CharacterStats.m_CharacterSkills.m_CalledShot)
            {
                return false;
            }
            if (_player.m_CharacterStats.SpentFocus > 0)
            {
                return false;
            }
            if (_player.m_CurrentDummy.m_SpecialAttack != 0)
            {
                return false;
            }
            Weapon.WeaponType bestWeaponType = _player.m_CurrentDummy.m_EventListener.GetBestWeaponType(_attemptedProf);
            if (bestWeaponType != Weapon.WeaponType.ranged && bestWeaponType != Weapon.WeaponType.firearm)
            {
                return false;
            }
            if (_attemptedProf != FTK_proficiencyTable.ID.None)
            {
                FTK_proficiencyTable fTK_proficiencyTable = FTK_proficiencyTableDB.Get(_attemptedProf);
                if (fTK_proficiencyTable.m_TargetFriendly || fTK_proficiencyTable.m_Target != 0)
                {
                    return false;
                }
            }
            if (!_player.m_CurrentDummy.CanUseAbility())
            {
                return false;
            }
            if (FTK_weaponStats2DB.GetDB().GetEntry(_player.m_WeaponID).m_NoFocus)
            {
                return false;
            }
            float value = 1f;
            CombatFloat combatFloat = new CombatFloat
            {
                Value = value,
                SetFloat = SetFloats.CalledShot
            };
            value = BattleAPI.Instance.GetFloat(_player.GetCurrentDummy(), combatFloat);
            if (UnityEngine.Random.value < value || FTKUI.Instance.m_PlayerSlots.m_CheatAttack == SlotControl.AttackCheatType.TriggerAbility)
            {
                _player.m_CurrentDummy.m_CriticalStrike = true;
                _player.m_CurrentDummy.m_SpecialAttack = CharacterDummy.SpecialAttack.CalledShot;
                ApplySkills(_player, Objects.FTKAPI_CharacterSkill.TriggerType.CalledShot);
                return true;
            }
            return false;
        }

        
    }
}
