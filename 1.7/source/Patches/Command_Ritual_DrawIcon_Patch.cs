using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFE_Tribals_Auto_Gathering
{
    [HarmonyPatch(typeof(Command_Ritual), nameof(Command_Ritual.DrawIcon))]
    [StaticConstructorOnStartup]
    public static class Command_Ritual_DrawIcon_Patch
    {
        private static readonly IntVec2 BadgeSize = new IntVec2(20, 20);

        private static readonly Texture2D AutoGatheringBadgeTex = ContentFinder<Texture2D>.Get("AutoGathering/UI/Icons/AutoGathering");

        public static void Postfix(Rect rect, Precept_Ritual ___ritual, TargetInfo ___targetInfo)
        {
            if (!AutoTribalGatheringGameComponent.IsTribalGathering(___ritual))
            {
                return;
            }

            AutoTribalGatheringGameComponent component = Current.Game.GetComponent<AutoTribalGatheringGameComponent>();
            if (!component.IsScheduledFor(___ritual, ___targetInfo))
            {
                return;
            }

            Rect badgeRect = new Rect(rect.xMax - (float)BadgeSize.x * 0.75f, rect.yMin - (float)BadgeSize.z * 0.25f, BadgeSize.x, BadgeSize.z);
            GUI.DrawTexture(badgeRect, AutoGatheringBadgeTex);
        }
    }
}
