using GameData.Domains;
using GameData.Domains.Combat;
using GameData.Domains.SpecialEffect;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaiwuModdingLib.Core.Plugin;

namespace ELTaiwuUtility
{
    partial class ELTaiwuModServer : TaiwuRemakeHarmonyPlugin
    {
        //轮回次数不影响伤害
        [HarmonyPatch]
        public class NoSamsaraInjuryEffect
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(GameData.Domains.SpecialEffect.CombatSkill.XiangShu.Neigong.Boss.WanXiangHeShu), "GetModifyValue")]
            public static void GetModifyValue_Postfix(ref int __result)
            {
                __result = 0;
            }
        }
    }
}
