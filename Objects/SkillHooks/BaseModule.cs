using Logger = FTKAPI.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FTKAPI.Objects.SkillHooks
{
    public class BaseModule
    {
        internal int prof;
        public virtual void Initialize() { }
        public virtual void Unload() { }

        public int Prof {
            get => prof;
            set => prof = value;
        }
        public static void ApplySkills(CharacterOverworld _char, FTKAPI_CharacterSkill.TriggerType _trig)
        {
            if (_char.m_CharacterStats.m_CharacterSkills is CustomCharacterSkills)
            {
                Logger.LogWarning("Found custom character skills");
                CustomCharacterSkills _tmpSkills = (CustomCharacterSkills)_char.m_CharacterStats.m_CharacterSkills;
                if (_tmpSkills.Skills != null)
                {
                    Logger.LogWarning("skills not null");
                    foreach (FTKAPI_CharacterSkill _skill in _tmpSkills.Skills)
                    {
                        if ((_skill.Trigger & _trig) == _trig)
                        {
                            Logger.LogWarning("Found a skill");
                            _skill.Skill(_char, _trig);
                        }
                    }
                }
            }
        }
        public static void ApplySkills(CharacterDummy _char, FTKAPI_CharacterSkill.TriggerType _trig, AttackAttempt _atk)
        {
            if ((bool)_char.m_CharacterOverworld)
            {
                if (_char.m_CharacterOverworld.m_CharacterStats.m_CharacterSkills is CustomCharacterSkills)
                {
                    CustomCharacterSkills _tmpSkills = (CustomCharacterSkills)_char.m_CharacterOverworld.m_CharacterStats.m_CharacterSkills;
                    if (_tmpSkills.Skills != null)
                    {
                        foreach (FTKAPI_CharacterSkill _skill in _tmpSkills.Skills)
                        {
                            if ((_skill.Trigger & _trig) == _trig)
                            {
                                _skill.Skill(_char.m_CharacterOverworld, _trig, _atk);
                            }
                        }
                    }
                }
            }
        }
    }
}
