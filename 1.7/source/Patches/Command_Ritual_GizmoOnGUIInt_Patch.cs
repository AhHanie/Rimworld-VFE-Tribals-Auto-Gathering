using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFE_Tribals_Auto_Gathering
{
    [HarmonyPatch(typeof(Command_Ritual), "GizmoOnGUIInt")]
    public static class Command_Ritual_GizmoOnGUIInt_Patch
    {
        // Infix on the base.GizmoOnGUIInt call: vanilla's ValidateDisabledState still runs on every
        // frame; only the base gizmo handling (and its own right-click handling) is skipped when the
        // right click toggles the schedule.
        [HarmonyPrefix]
        [HarmonyInfix(typeof(Command), "GizmoOnGUIInt", typeof(Rect), typeof(GizmoRenderParms))]
        public static bool ToggleScheduleOnRightClick(Rect butRect, ref GizmoResult __result, [HarmonyOuter] Precept_Ritual ___ritual, [HarmonyOuter] TargetInfo ___targetInfo)
        {
            Event ev = Event.current;
            if (ev == null || ev.type != EventType.MouseDown || ev.button != 1 || !Mouse.IsOver(butRect))
            {
                return true;
            }

            if (!AutoTribalGatheringGameComponent.IsTribalGathering(___ritual))
            {
                return true;
            }

            Current.Game.GetComponent<AutoTribalGatheringGameComponent>().Toggle(___ritual, ___targetInfo);
            ev.Use();
            __result = new GizmoResult(GizmoState.Clear, null);
            return false;
        }
    }
}
