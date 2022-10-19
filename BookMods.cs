using Config;
using GameData.Common;
using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.Character.Creation;
using GameData.Domains.Character.Relation;
using GameData.Domains.Item;
using GameData.Domains.Taiwu;
using GameData.Utilities;
using HarmonyLib;
using Redzen.Random;
using System;
using System.Collections.Generic;
using System.Reflection;
using static ELTaiwuUtility.ELTaiwuModServer;
using Character = GameData.Domains.Character.Character;
using SkillBook = GameData.Domains.Item.SkillBook;

namespace ELTaiwuUtility
{
    public partial class ELTaiwuModServer : TaiwuModdingLib.Core.Plugin.TaiwuRemakeHarmonyPlugin
    {
        //移动后推进读书进度
        [HarmonyPatch(typeof(GameData.Domains.Map.MapDomain), "Move", typeof(DataContext), typeof(short), typeof(bool))]
        public class AdvanceReadingProgressAfterMapMove
        {
            private static int moveCount = 0;
            public static void Postfix(DataContext context, short destBlockId, bool notCostTime)
            {
                moveCount++;
                if (moveCount >= advRPMapMoveCount)
                {
                    DomainManager.Taiwu.UpdateReadingProgressOnMonthChange(context);
                    moveCount = 0;
                }
            }
        }

        //天数增长后推进读书进度
        [HarmonyPatch(typeof(GameData.Domains.World.WorldDomain), "SetDaysInCurrMonth")]
        public class AdvanceReadingProgressAfterDateChange
        {
            [HarmonyPrefix]
            public static bool Prefix(GameData.Domains.World.WorldDomain __instance, ref sbyte value, ref GameData.Common.DataContext context)
            {
                sbyte thisday = __instance.GetDaysInCurrMonth();
                if (value - thisday > 1)
                {
                    for (int i = 0; i < (value - thisday); i++)
                    {
                        if (((thisday + i) % advRPDateSpan == 0))
                        {
                            DomainManager.Taiwu.UpdateReadingProgressOnMonthChange(context);
                            break;
                        }
                    }
                }
                else
                {
                    if (value % advRPDateSpan == 0)
                    {
                        DomainManager.Taiwu.UpdateReadingProgressOnMonthChange(context);
                    }
                }

                return true;
            }
        }

        //读武功秘籍时直接领悟全部书页
        [HarmonyPatch(typeof(GameData.Domains.Taiwu.TaiwuDomain), "UpdateCombatSkillBookReadingProgress")]
        public class LearnAllPagesInCombatSkillBook
        {
            public static unsafe void Postfix(TaiwuDomain __instance, DataContext context, SkillBook book, ReadingBookStrategies strategies)
            {
                short skillTemplateId = book.GetCombatSkillTemplateId();
                TaiwuCombatSkill taiwuCombatSkill = (TaiwuCombatSkill)typeof(TaiwuDomain).GetMethod("GetTaiwuCombatSkill", (BindingFlags)(-1)).Invoke(__instance, new object[] { skillTemplateId });
                sbyte[] totalProgress = taiwuCombatSkill.GetAllBookPageReadingProgress();
                var outlineProgressSeg = new ArraySegment<sbyte>(totalProgress, 0, 5);
                var maxOutlineProgress = outlineProgressSeg.Max();
                for (int i = 0; i < 5; i++)
                {
                    outlineProgressSeg[i] = maxOutlineProgress;
                }
                var sideAProgressSeg = new ArraySegment<sbyte>(totalProgress, 5, 5);
                var sideBProgressSeg = new ArraySegment<sbyte>(totalProgress, 10, 5);
                for (int i = 0; i < 5; i++)
                {
                    var maxSideProgress = Math.Max(sideAProgressSeg[i], sideBProgressSeg[i]);
                    sideAProgressSeg[i] = maxSideProgress;
                    sideBProgressSeg[i] = maxSideProgress;
                }
                for (byte i = 0; i < 15; i++)
                {
                    taiwuCombatSkill.SetBookPageReadingProgress(i, totalProgress[i]);
                    if (totalProgress[i] == 100)
                    {
                        typeof(GameData.Domains.Taiwu.TaiwuDomain).GetMethod("SetCombatSkillPageComplete", (BindingFlags)(-1)).Invoke(__instance, new object[] { context, book, i });
                    }
                }
            }
        }

        //无亡佚书页
        [HarmonyPatch]
        public class NoLostPages
        {
            [HarmonyPatch(typeof(SkillBook), "GeneratePageIncompleteState")]
            public unsafe static void Prefix(IRandomSource random, sbyte skillGroup, sbyte grade, ref sbyte completePagesCount, ref sbyte lostPagesCount, bool outlineAlwaysComplete)
            {
                lostPagesCount = 0;
                const int normalPagesCount = 5;
                bool flag = completePagesCount < 0;
                if (flag)
                {
                    float mean = 4.0f - grade / 4f;
                    completePagesCount = (sbyte)Math.Round((double)RedzenHelper.NormalDistribute(random, mean, 0.5f, Math.Max(0f, mean - 1f), Math.Min(normalPagesCount, mean + 1f)));
                }
            }

            [HarmonyPatch(typeof(ItemDomain), "CreateSkillBook", typeof(DataContext), typeof(short), typeof(sbyte), typeof(sbyte), typeof(sbyte), typeof(sbyte), typeof(bool))]
            public static void Prefix(DataContext context, short templateId, sbyte completePagesCount, ref sbyte lostPagesCount, sbyte outlinePageType, sbyte normalPagesDirectProb, bool outlineAlwaysComplete)
            {
                lostPagesCount = 0;
            }

            [HarmonyPatch(typeof(ItemDomain), "CreateSkillBook", typeof(DataContext), typeof(short), typeof(byte), typeof(sbyte), typeof(sbyte), typeof(bool))]
            public static void Prefix(DataContext context, short templateId, byte pageTypes, sbyte completePagesCount, ref sbyte lostPagesCount, bool outlineAlwaysComplete)
            {
                lostPagesCount = 0;
            }
        }
    }
}