using HarmonyLib;
using RimWorld;
using Verse;

namespace VFE_Tribals_Auto_Gathering
{
    [HarmonyPatch(typeof(Command_Ritual), nameof(Command_Ritual.DescPostfix), MethodType.Getter)]
    public static class Command_Ritual_DescPostfix_Patch
    {
        public static void Postfix(ref string __result, Precept_Ritual ___ritual, TargetInfo ___targetInfo)
        {
            if (!AutoTribalGatheringGameComponent.IsTribalGathering(___ritual))
            {
                return;
            }

            AutoTribalGatheringGameComponent component = Current.Game.GetComponent<AutoTribalGatheringGameComponent>();
            if (!component.TryGetScheduledHour(___ritual, ___targetInfo, out int hourOfDay))
            {
                return;
            }

            string hourText = hourOfDay.ToString() + "LetterHour".Translate();
            __result += "\n\n" + "VFETribalsAutoGathering.TooltipHeader".Translate().Colorize(ColoredText.TipSectionTitleColor)
                + "\n" + "VFETribalsAutoGathering.TooltipEnabledAt".Translate(hourText);
        }
    }
}
