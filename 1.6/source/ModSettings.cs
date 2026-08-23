using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VFE_Tribals_Auto_Gathering
{
    public class ModSettings : Verse.ModSettings
    {
        public static bool ExcludePawnsWithPlayerForcedJobs = false;
        public static bool ExcludePawnsNeedingMedicalRest = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ExcludePawnsWithPlayerForcedJobs, "excludePawnsWithPlayerForcedJobs", false);
            Scribe_Values.Look(ref ExcludePawnsNeedingMedicalRest, "excludePawnsNeedingMedicalRest", true);
        }
    }
}
