using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFE_Tribals_Auto_Gathering
{
    [HarmonyPatch(typeof(Command_Ritual), nameof(Command_Ritual.ProcessInput))]
    public static class Command_Ritual_ProcessInput_Patch
    {
        public static bool Prefix(Event ev, Precept_Ritual ___ritual, TargetInfo ___targetInfo)
        {
            if (ev == null || ev.button != 1 || !AutoTribalGatheringGameComponent.IsTribalGathering(___ritual))
            {
                return true;
            }

            AutoTribalGatheringGameComponent component = Current.Game.GetComponent<AutoTribalGatheringGameComponent>();
            component.Toggle(___ritual, ___targetInfo);

            return false;
        }
    }
}
