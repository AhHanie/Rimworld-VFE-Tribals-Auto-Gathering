using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VFE_Tribals_Auto_Gathering
{
    public static class ModSettingsWindow
    {
        public static void Draw(Rect parent)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(parent);

            listing.CheckboxLabeled(
                "VFETribalsAutoGathering.ExcludePlayerForcedPawnsLabel".Translate(),
                ref ModSettings.ExcludePawnsWithPlayerForcedJobs,
                "VFETribalsAutoGathering.ExcludePlayerForcedPawnsTooltip".Translate());

            listing.End();
        }
    }
}
