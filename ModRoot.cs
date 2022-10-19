using GameData.Domains;
using GameData.Domains.Character;
using GameData.Domains.CombatSkill;
using HarmonyLib;
using System;
using Character = GameData.Domains.Character.Character;

namespace ELTaiwuUtility
{
    [TaiwuModdingLib.Core.Plugin.PluginConfig("ELTaiwuUtility", "Elysiel", "0.0.7")]
    public partial class ELTaiwuModServer : TaiwuModdingLib.Core.Plugin.TaiwuRemakeHarmonyPlugin
    {
        //------Mod设置用变量------
        public static int advRPDateSpan; //多少天后读书

        public static int advRPMapMoveCount; //多少次地图移动后读书 advRP = AdvanceReadingProgress

        public static int accelPracticeBase; //加速修行基本值
        public static int accelPracticeDiceOption; //加速修行骰子选项

        public static int immortalAge; //太吾长生年龄

        public static int combatSkillFloor; //战斗技能保底
        public static int lifeSkillFloor; //生活技能保底
        public static int mainAttrFloor; //主属性保底

        public static int neigongLoopingMult; //内力获取倍率
        public static int neiliCapMult; //真气上限倍率
        //------

        //------Mod初始化------
        public bool IsModEnabled(string modKey)
        {
            bool enabled = false;
            if (modKey == "ELMod00_LockAllGoodFeatures")
            {
                DomainManager.Mod.GetSetting(ModIdStr, modKey, ref enabled);
                return enabled;
            }
            if (modKey == "ELMod01_AdvaceReadingProgressAfterDateChange")
            {
                DomainManager.Mod.GetSetting(ModIdStr, modKey, ref advRPDateSpan);
                return advRPDateSpan > 0;
            }
            if (modKey == "ELMod02_AdvaceReadingProgressAfterMapMove")
            {
                DomainManager.Mod.GetSetting(ModIdStr, modKey, ref advRPMapMoveCount);
                return advRPMapMoveCount > 0;
            }
            if (modKey == "ELMod04_AccelPracticeBase")
            {
                DomainManager.Mod.GetSetting(ModIdStr, modKey, ref accelPracticeBase);
                return accelPracticeBase > 0;
            }
            if (modKey == "ELMod03_AccelPracticeDiceOption")
            {
                DomainManager.Mod.GetSetting(ModIdStr, modKey, ref accelPracticeDiceOption);
                return accelPracticeDiceOption > 0;
            }
            if (modKey == "ELMod05_ImmortalTaiwu")
            {
                DomainManager.Mod.GetSetting(ModIdStr, modKey, ref enabled);
                return enabled;
            }
            if (modKey == "ELMod07_LearnAllPagesInInCombatSkillBook")
            {
                DomainManager.Mod.GetSetting(ModIdStr, modKey, ref enabled);
                return enabled;
            }
            if (modKey == "ELMod08_LesbianWorld")
            {
                DomainManager.Mod.GetSetting(ModIdStr, modKey, ref enabled);
                return enabled;
            }
            if (modKey == "ELMod09_SkillQualificationFloor")
            {
                DomainManager.Mod.GetSetting(ModIdStr, modKey, ref enabled);
                return enabled;
            }
            if (modKey == "ELMod12_NoLostPages")
            {
                DomainManager.Mod.GetSetting(ModIdStr, modKey, ref enabled);
                return enabled;
            }
            if (modKey == "ELMod13_AccelNeigongLooping")
            {
                DomainManager.Mod.GetSetting(ModIdStr, modKey, ref neigongLoopingMult);
                return neigongLoopingMult > 1;
            }
            if (modKey == "ELMod14_IncreaseNeiliCap")
            {
                DomainManager.Mod.GetSetting(ModIdStr, modKey, ref neiliCapMult);
                return neiliCapMult > 1;
            }
            if (modKey == "ELMod15_NoSamsaraInjuryEffect")
            {
                DomainManager.Mod.GetSetting(ModIdStr, modKey, ref enabled);
                return enabled;
            }

            //default
            return enabled;
        }

        public override void OnModSettingUpdate()
        {
            HarmonyInstance.UnpatchSelf();
            HarmonyInstance.PatchAll(typeof(GetCloseFriend));

            if (IsModEnabled("ELMod00_LockAllGoodFeatures"))
                HarmonyInstance.PatchAll(typeof(LockAllGoodFeatures));

            if (IsModEnabled("ELMod01_AdvaceReadingProgressAfterDateChange"))
                HarmonyInstance.PatchAll(typeof(AdvanceReadingProgressAfterDateChange));

            if (IsModEnabled("ELMod02_AdvaceReadingProgressAfterMapMove"))
                HarmonyInstance.PatchAll(typeof(AdvanceReadingProgressAfterMapMove));

            bool modEnable = IsModEnabled("ELMod04_AccelPracticeBase");
            modEnable = IsModEnabled("ELMod03_AccelPracticeDiceOption") || modEnable;
            if (modEnable)
                HarmonyInstance.PatchAll(typeof(AccelPractice));

            if (IsModEnabled("ELMod05_ImmortalTaiwu"))
            {
                DomainManager.Mod.GetSetting(ModIdStr, "ELMod05A_ImmortalAge", ref immortalAge);
                HarmonyInstance.PatchAll(typeof(ImmortalTaiwu));
            }

            if (IsModEnabled("ELMod07_LearnAllPagesInInCombatSkillBook"))
                HarmonyInstance.PatchAll(typeof(LearnAllPagesInCombatSkillBook));

            if (IsModEnabled("ELMod08_LesbianWorld"))
            {
                HarmonyInstance.PatchAll(typeof(LesbianWorld));
            }

            if (IsModEnabled("ELMod09_SkillQualificationFloor"))
            {
                DomainManager.Mod.GetSetting(ModIdStr, "ELMod09A_MainAttrFloor", ref mainAttrFloor);
                DomainManager.Mod.GetSetting(ModIdStr, "ELMod09B_CombatSkillFloor", ref combatSkillFloor);
                DomainManager.Mod.GetSetting(ModIdStr, "ELMod09C_LifeSkillFloor", ref lifeSkillFloor);
                HarmonyInstance.PatchAll(typeof(SkillQualificationFloor));
            }

            if (IsModEnabled("ELMod12_NoLostPages"))
                HarmonyInstance.PatchAll(typeof(NoLostPages));

            if (IsModEnabled("ELMod13_AccelNeigongLooping"))
                HarmonyInstance.PatchAll(typeof(AccelNeigongLooping));

            if (IsModEnabled("ELMod14_IncreaseNeiliCap"))
                HarmonyInstance.PatchAll(typeof(IncreaseNeiliCap));

            if (IsModEnabled("ELMod15_NoSamsaraInjuryEffect"))
                HarmonyInstance.PatchAll(typeof(NoSamsaraInjuryEffect));
        }

        public override void Initialize()
        {
            OnModSettingUpdate();
        }

        public override void Dispose()
        {
            HarmonyInstance.UnpatchSelf();
        }
        //------

        //------工具方法与变量------
        public const sbyte female = 0;
        public const sbyte male = 1;
        public static Random rand = new Random();

        //主属性索引号
        public const int 臂力 = 0;
        public const int 灵敏 = 1;
        public const int 定力 = 2;
        public const int 体质 = 3;
        public const int 根骨 = 4;
        public const int 悟性 = 5;

        public enum SkillGrowthType
        {
            Average = 0,
            Precocious = 1,
            LateBlooming = 2
        }

        public static short cfCharTemplateID = -1; //密友人物模板ID cf = close friend
        public static int cfCharID = -1; //密友人物ID

        public static int RollSingleDice(int dice) //投一个骰子
        {
            int result = rand.Next(1, dice);
            return result;
        }

        public static int GetDice(int diceOption) //TRPG通用骰子列表
        {
            switch (diceOption)
            {
                case 1:
                    return 2;
                case 2:
                    return 4;
                case 3:
                    return 6;
                case 4:
                    return 8;
                case 5:
                    return 10;
                case 6:
                    return 20;
                case 7:
                    return 100;
                default:
                    throw new Exception("Dice Option Invalid!");
            };
        }

        public static short GetClampedAgeOfAgeEffect(short age)
        {
            bool flag = age < 0;
            short result;
            if (flag)
            {
                result = 20;
            }
            else
            {
                bool flag2 = age > 100;
                if (flag2)
                {
                    result = 100;
                }
                else
                {
                    result = age;
                }
            }
            return result;
        }

        [HarmonyPatch]
        public class GetCloseFriend //获取密友
        {
            [HarmonyPatch(typeof(CharacterDomain), "CreateCloseFriend")]
            public static void Prefix(short charTemplateId)
            {
                cfCharTemplateID = charTemplateId;
            }

            [HarmonyPatch(typeof(CharacterDomain), "CreateCloseFriend")]
            public static void Postfix(Character __instance)
            {
                cfCharID = __instance.GetId();
            }
        }
        //------


        //------Mod功能实现：已转移至其余文件中

    }
}
