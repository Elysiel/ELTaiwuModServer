using GameData.Domains;
using GameData.Domains.Character.Creation;
using GameData.Domains.Character.Relation;
using GameData.Utilities;
using HarmonyLib;
using System;
using static ELTaiwuUtility.ELTaiwuModServer;
using Character = GameData.Domains.Character.Character;

namespace ELTaiwuUtility
{
    public partial class ELTaiwuModServer : TaiwuModdingLib.Core.Plugin.TaiwuRemakeHarmonyPlugin
    {
        //百合世界
        [HarmonyPatch]
        public class LesbianWorld
        {
            [HarmonyPatch(typeof(GameData.Domains.Character.Character), "OfflineCreateIntelligentCharacter")]
            public static bool Prefix(ref IntelligentCharacterCreationInfo info, ref sbyte ____gender)
            {
                bool flag = cfCharTemplateID != -1 && info.CharTemplateId.Equals(cfCharTemplateID) && DomainManager.Taiwu.GetTaiwu().GetGender() == female;
                if (flag)
                    ____gender = female;
                return true;
            }

            [HarmonyPatch(typeof(GameData.Domains.Character.Ai.AiHelper.Relation), "GetStartRelationSuccessRate_SexRelationBaseRate")]
            public static void Postfix(ref int __result, Character selfChar, Character targetChar, RelatedCharacter selfToTarget, RelatedCharacter targetToSelf)
            {
                int successRate = 100;
                sbyte selfGender = selfChar.GetGender();
                sbyte selfDisplayingGender = selfChar.GetDisplayingGender();
                bool bisexualSelf = selfChar.GetBisexual();
                sbyte targetGender = targetChar.GetGender();
                sbyte targetDisplayingGender = targetChar.GetDisplayingGender();
                bool bisexualTarget = targetChar.GetBisexual();
                int taiwuCharId = DomainManager.Taiwu.GetTaiwuCharId();
                do
                {
                    if (targetChar.GetId() == taiwuCharId && targetGender == female && selfGender == male)
                    {
                        successRate = int.MinValue;
                        __result = successRate;
                        break;
                    }

                    if (targetGender == female && selfGender == female)
                    {
                        bool flag5 = RelationType.HasRelation(targetToSelf.RelationType, 1024);
                        if (flag5)
                        {
                            successRate += 40;
                        }
                        bool flag6 = RelationType.ContainDirectBloodRelations(targetToSelf.RelationType);
                        if (flag6)
                        {
                            successRate -= 60;
                        }
                        bool flag7 = RelationType.ContainNonBloodFamilyRelations(targetToSelf.RelationType) || RelationType.HasRelation(selfToTarget.RelationType, 2048) || RelationType.HasRelation(selfToTarget.RelationType, 4096);
                        if (flag7)
                        {
                            successRate -= 30;
                        }
                        short selfCharAge = selfChar.GetCurrAge();
                        short targetCharAge = targetChar.GetCurrAge();
                        successRate -= MathUtils.Clamp(2 * Math.Abs((int)(selfCharAge - targetCharAge)), 0, 40);
                        successRate += Math.Min(90, (int)((FavorabilityType.GetFavorabilityType(targetToSelf.Favorability) - 3) * 40));
                        successRate += (int)((selfChar.GetAttraction() - targetChar.GetAttraction()) * 10);
                        successRate += (int)((selfChar.GetInteractionGrade() - targetChar.GetInteractionGrade()) * 10);
                        if (selfDisplayingGender == male && !bisexualTarget)
                        {
                            successRate -= 20;
                        }
                        if (targetDisplayingGender == male && !bisexualSelf)
                        {
                            successRate -= 20;
                        }
                        bool flag11 = selfChar.GetId() != taiwuCharId && targetChar.GetId() != taiwuCharId;
                        if (flag11)
                        {
                            successRate += 20;
                        }
                        int childCount = 0;
                        bool flag12 = RelationType.ContainChildRelations(selfToTarget.RelationType);
                        if (flag12)
                        {
                            RelatedCharacters selfRelatedChars = DomainManager.Character.GetRelatedCharacters(selfChar.GetId());
                            childCount += selfRelatedChars.AdoptiveChildren.GetCount() + selfRelatedChars.BloodChildren.GetCount() + selfRelatedChars.StepChildren.GetCount();
                        }
                        bool flag13 = RelationType.ContainChildRelations(targetToSelf.RelationType);
                        if (flag13)
                        {
                            RelatedCharacters targetRelatedChars = DomainManager.Character.GetRelatedCharacters(targetChar.GetId());
                            childCount += targetRelatedChars.AdoptiveChildren.GetCount() + targetRelatedChars.BloodChildren.GetCount() + targetRelatedChars.StepChildren.GetCount();
                        }
                        successRate -= 5 * childCount;
                        bool flag14 = selfChar.GetMonkType() > 0;
                        if (flag14)
                        {
                            successRate -= 60;
                        }
                        bool flag15 = targetChar.GetMonkType() > 0;
                        if (flag15)
                        {
                            successRate -= 60;
                        }
                        bool flag16 = selfChar.GetFertility() <= 0;
                        if (flag16)
                        {
                            successRate -= 30;
                        }
                        bool flag17 = targetChar.GetFertility() <= 0;
                        if (flag17)
                        {
                            successRate -= 30;
                        }
                        __result = successRate;
                    }
                } while (false);
            }
        }
    }
}