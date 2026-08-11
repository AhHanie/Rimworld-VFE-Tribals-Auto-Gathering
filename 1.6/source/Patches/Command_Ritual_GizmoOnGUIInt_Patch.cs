using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFE_Tribals_Auto_Gathering
{
    [HarmonyPatch(typeof(Command_Ritual), "GizmoOnGUIInt")]
    public static class Command_Ritual_GizmoOnGUIInt_Patch
    {
        public static void Prefix(Rect butRect, Precept_Ritual ___ritual, TargetInfo ___targetInfo)
        {
            Event ev = Event.current;
            if (ev == null || ev.type != EventType.MouseDown || ev.button != 1 || !Mouse.IsOver(butRect))
            {
                return;
            }

            if (!AutoTribalGatheringGameComponent.IsTribalGathering(___ritual))
            {
                return;
            }

            Current.Game.GetComponent<AutoTribalGatheringGameComponent>().Toggle(___ritual, ___targetInfo);
            ev.Use();
        }
    }
}
