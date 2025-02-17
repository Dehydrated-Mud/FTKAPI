﻿using IL.QuestMaker.Attributes;
using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Xml.Schema;
using Logger = FTKAPI.Utils.Logger;
using OpCodes = Mono.Cecil.Cil.OpCodes;

namespace FTKAPI.Objects.SkillHooks
{
    internal class HookRespondToAttack : BaseModule
    {
        public override void Initialize()
        {
            Unload();
            IL.CharacterDummy.RespondToHit += HookRespondToHit;
        }

        private static void HookRespondToHit(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            //Killshot hook
            c.GotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchLdfld<CharacterDummy>("m_DamageInfo"),
                x => x.MatchLdfld<DummyDamageInfo>("m_SpecialAttack"),
                x => x.MatchStloc(20),
                x => x.MatchLdloc(20),
                x => x.MatchLdcI4(9)
                ) ;
            
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate<Action<CharacterDummy, bool>>((_this, _mainVictim) =>
            {
                CharacterDummy _assailant = EncounterSession.Instance.GetDummyByFID(_this.m_DamageInfo.m_AttackerID);
                if (_mainVictim & (bool)_assailant.m_CharacterOverworld)
                {
                    CustomCharacterSkills _tmpSkills;
                    if (_assailant.m_CharacterOverworld.m_CharacterStats.m_CharacterSkills is CustomCharacterSkills)
                    {
                        _tmpSkills = (CustomCharacterSkills)_assailant.m_CharacterOverworld.m_CharacterStats.m_CharacterSkills;
                        if (_tmpSkills.Skills != null)
                        {
                            foreach (FTKAPI_CharacterSkill _skill in _tmpSkills.Skills)
                            {
                                if ((_skill.m_TriggerType & FTKAPI_CharacterSkill.TriggerType.RespondToHit) == FTKAPI_CharacterSkill.TriggerType.RespondToHit)
                                {
                                    _skill.Skill(_assailant.m_CharacterOverworld, FTKAPI_CharacterSkill.TriggerType.RespondToHit);
                                }
                            }
                        }
                    }
                }
            });
            if (c.TryGotoNext(
                x => x.MatchLdarg(0),
                x => x.MatchCastclass<EnemyDummy>(),
                x => x.MatchStloc(28)
                ))
            {
                c.RemoveRange(6);
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldloc_S, (byte)27);
                c.EmitDelegate(CheckEnemy);
            }
        }
        private static void CheckEnemy(CharacterDummy _this, CharacterDummy _attacker)
        {
            if (_this is EnemyDummy)
            {
                EnemyDummy enemyDummy2 = (EnemyDummy)_this;
                enemyDummy2.m_LastAttackedBy = _attacker;
            }
            else
            {
                Logger.LogWarning($"The game was going to cast the dummy {_this.name} to an enemy but we stopped it");
            }
        }
        public override void Unload()
        {
            IL.CharacterStats.TallyCharacterMods -= HookRespondToHit;
        }
    }
}
