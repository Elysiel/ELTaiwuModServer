using Config;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Character;
using HarmonyLib;
using Redzen.Random;
using System.Collections.Generic;
using System;
using static ELTaiwuUtility.ELTaiwuModServer;
using Character = GameData.Domains.Character.Character;
using TaiwuModdingLib.Core.Plugin;
using GameData.Domains.Character.Creation;
using System.Reflection;
using GameData.Domains.Item;

namespace ELTaiwuUtility
{
    partial class ELTaiwuModServer : TaiwuRemakeHarmonyPlugin
    {
        //太吾为长生种
        [HarmonyPatch]
        public class ImmortalTaiwu
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(Character), "CalcPhysiologicalAge")]
            public static void CalcPhysiologicalAge_Postfix(Character __instance, ref short __result)
            {
                int taiwuCharId = DomainManager.Taiwu.GetTaiwuCharId();
                if (__instance.GetId() == taiwuCharId)
                    __result = (short)immortalAge;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Character), "OfflineCreateProtagonist")]
            public static void OfflineCreateProtagonist_Postfix(Character __instance, ref sbyte ____combatSkillQualificationGrowthType, ref sbyte ____lifeSkillQualificationGrowthType)
            {
                ____combatSkillQualificationGrowthType = SkillQualificationGrowthType.Average;
                ____lifeSkillQualificationGrowthType = SkillQualificationGrowthType.Average;
            }
        }
        //开局随机特性时，锁定无负面特性
        [HarmonyPatch(typeof(Character), "GenerateRandomBasicFeatures")]
        public class LockAllGoodFeatures
        {
            public static void Prefix(DataContext context, Dictionary<short, short> featureGroup2Id, bool isProtagonist, ref bool allGoodBasicFeatures)
            {
                if (isProtagonist) allGoodBasicFeatures = true;
            }
        }

        //加速功法修行
        [HarmonyPatch(typeof(GameData.Domains.Taiwu.TaiwuDomain), "CalcPracticeResult")]
        public class AccelPractice
        {
            public static int Postfix(int result)
            {
                return (accelPracticeBase + RollSingleDice(GetDice(accelPracticeDiceOption)));
            }
        }
        //技能保底
        [HarmonyPatch(typeof(Character), "OfflineCreateProtagonist")]
        public class SkillQualificationFloor
        {
            public static unsafe void Postfix(short templateId, short orgMemberId, ProtagonistCreationInfo info, DataContext context, ref LifeSkillShorts ____baseLifeSkillQualifications, ref CombatSkillShorts ____baseCombatSkillQualifications, ref MainAttributes ____baseMainAttributes, ref LifeSkillShorts ____lifeSkillQualifications, ref CombatSkillShorts ____combatSkillQualifications, ref sbyte ____lifeSkillQualificationGrowthType, ref sbyte ____combatSkillQualificationGrowthType)
            {
                fixed (short* ptr = ____baseLifeSkillQualifications.Items) for (int i = 0; i < 16; i++)
                    {
                        *(ptr + i) = Math.Max(*(ptr + i), (short)(lifeSkillFloor + RollSingleDice(20) - 1));
                    }
                fixed (short* ptr = ____baseCombatSkillQualifications.Items) for (int i = 0; i < 14; i++)
                    {
                        *(ptr + i) = Math.Max(*(ptr + i), (short)(combatSkillFloor + RollSingleDice(20) - 1));
                    }
                fixed (short* ptr = ____baseMainAttributes.Items) for (int i = 0; i < 6; i++)
                    {
                        *(ptr + i) = Math.Max(*(ptr + i), (short)(mainAttrFloor + RollSingleDice(20) - 1));
                    }
                ____lifeSkillQualifications = ____baseLifeSkillQualifications;
                ____combatSkillQualifications = ____baseCombatSkillQualifications;
            }
        }

        //内力获取加速
        [HarmonyPatch(typeof(GameData.Domains.CombatSkill.CombatSkillDomain), "CalcNeigongLoopingEffect")]
        public class AccelNeigongLooping
        {
            public static void Postfix(ref ValueTuple<short, short> __result, IRandomSource random, Character character, CombatSkillItem skillCfg)
            {
                var taiwuCharID = DomainManager.Taiwu.GetTaiwuCharId();
                if (character.GetId() == taiwuCharID)
                {
                    __result.Item1 *= (short)neigongLoopingMult;
                }
            }
        }

        [HarmonyPatch(typeof(CombatHelper), "GetMaxTotalNeiliAllocation")]
        //提高真气上限
        public class IncreaseNeiliCap
        {
            public static void Postfix(ref short __result)
            {
                __result *= (short)neiliCapMult;
            }
        }

    }


}

