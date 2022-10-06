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
using System.Threading;

namespace ELTaiwuUtility
{
    partial class ELTaiwuModServer : TaiwuRemakeHarmonyPlugin
    {
        //太吾为长生种
        [HarmonyPatch]
        public class ImmortalTaiwu
        {
            public const short immortalMaxHP = 32000; //长生种HP上限

            //锁定身体年龄
            [HarmonyPostfix]
            [HarmonyPatch(typeof(Character), "CalcPhysiologicalAge")]
            public static void CalcPhysiologicalAge_Postfix(Character __instance, ref short __result)
            {
                int taiwuCharId = DomainManager.Taiwu.GetTaiwuCharId();
                if (__instance.GetId() == taiwuCharId)
                    __result = (short)Math.Min(immortalAge, __result);
            }

            //锁定技能成长模式
            [HarmonyPostfix]
            [HarmonyPatch(typeof(Character), "OfflineCreateProtagonist")]
            public static void OfflineCreateProtagonist_Postfix(Character __instance, ref sbyte ____combatSkillQualificationGrowthType, ref sbyte ____lifeSkillQualificationGrowthType)
            {
                ____combatSkillQualificationGrowthType = SkillQualificationGrowthType.Average;
                ____lifeSkillQualificationGrowthType = SkillQualificationGrowthType.Average;
            }

            //BEGIN 锁定长生种太吾的HP上限
            [HarmonyPostfix]
            [HarmonyPatch(typeof(Character), "AdjustLifespan")]
            public static void AdjustLifespan_Postfix(DataContext context, Character __instance, ref short ____baseMaxHealth, ref short ____maxHealth, ref short ____health)
            {
                if (__instance.GetId() == DomainManager.Taiwu.GetTaiwuCharId())
                {
                    ____baseMaxHealth = immortalMaxHP;
                    __instance.SetBaseMaxHealth(immortalMaxHP, context);
                    ____maxHealth = immortalMaxHP;
                    ____health = immortalMaxHP;
                    __instance.SetHealth(immortalMaxHP, context);
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Character), "GetLeftMaxHealth")]
            public static void GetLeftMaxHealth_Postfix(Character __instance, ref short __result)
            {
                if (__instance.GetId() == DomainManager.Taiwu.GetTaiwuCharId())
                    __result = immortalMaxHP;
            }


            [HarmonyPostfix]
            [HarmonyPatch(typeof(Character), "CalcMaxHealth")]
            public static void CalcMaxHealth_Postfix(Character __instance, ref short __result, ref short ____maxHealth)
            {
                if (__instance.GetId() == DomainManager.Taiwu.GetTaiwuCharId())
                {
                    Thread.MemoryBarrier();
                    var spinLock = new SpinLock(false);
                    bool lockTaken = false;
                    try
                    {
                        spinLock.Enter(ref lockTaken);
                        ____maxHealth = immortalMaxHP;
                        __result = immortalMaxHP;
                    }
                    finally
                    {
                        if (lockTaken)
                            spinLock.Exit(false);
                    }
                    Thread.MemoryBarrier();
                }
            }
            //END

            //BEGIN 改写长生种属性恢复量
            public static unsafe MainAttributes CalcImmortalAttrRecoveries(Character __instance)
            {
                MainAttributes maxMainAttributes = __instance.GetMaxMainAttributes();
                short physicAge = __instance.GetPhysiologicalAge();
                short mentalAge = __instance.GetActualAge();
                int clampedPhyAge = (int)((physicAge <= 100) ? physicAge : 100);
                int clampedMenAge = (int)((mentalAge <= 100) ? mentalAge : 100);
                MainAttributes phyAgeMod = AgeEffect.Instance[clampedPhyAge].MainAttributesRecoveries;
                MainAttributes menAgeMod = AgeEffect.Instance[clampedMenAge].MainAttributesRecoveries;
                MainAttributes recoveries = default(MainAttributes);
                for (int i = 0; i < 6; i++)
                {
                    short maxValue = maxMainAttributes.Items[i];
                    int recovery = 0;
                    switch (i)
                    {

                        case 臂力:
                        case 灵敏:
                        case 体质:
                            recovery = (int)(maxValue / 5 * phyAgeMod.Items[i] / 100);
                            break;
                        case 定力:
                        case 根骨:
                        case 悟性:
                            recovery = (int)(maxValue / 5 * menAgeMod.Items[i] / 100);
                            break;
                    }
                    recoveries.Items[i] = (short)recovery;
                }
                return recoveries;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(Character), "GetMainAttributesRecoveries")]
            public static unsafe void GetMainAttributesRecoveries_Postfix(Character __instance, ref MainAttributes __result)
            {
                if (__instance.GetId() == DomainManager.Taiwu.GetTaiwuCharId())
                {
                    __result = CalcImmortalAttrRecoveries(__instance);
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(Character), "OfflineAutoRecoverMainAttributes")]
            private static unsafe bool OfflineAutoRecoverMainAttributes_Prefix(Character __instance, ref MainAttributes ____currMainAttributes)
            {
                if (__instance.GetId() == DomainManager.Taiwu.GetTaiwuCharId())
                {
                    MainAttributes maxMainAttributes = __instance.GetMaxMainAttributes();
                    MainAttributes ageInfluence = CalcImmortalAttrRecoveries(__instance);
                    for (int i = 0; i < 6; i++)
                    {
                        short maxValue = maxMainAttributes.Items[i];
                        int recovery = maxValue / 5 * ageInfluence.Items[i] / 100;
                        int currValue = ____currMainAttributes.Items[i] + recovery;
                        ____currMainAttributes.Items[i] = (short)Math.Clamp(currValue, 0, (int)maxValue);
                    }
                    return false;
                }
                else
                    return true;
            }
            //END
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

