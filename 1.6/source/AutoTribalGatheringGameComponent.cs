using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace VFE_Tribals_Auto_Gathering
{
    public class AutoTribalGatheringGameComponent : GameComponent
    {
        private Precept_Ritual scheduledRitual;
        private TargetInfo scheduledTarget = TargetInfo.Invalid;
        private int scheduledLocalDayTick = -1;
        private long nextExecutionAbsTick = -1L;

        public AutoTribalGatheringGameComponent(Game game)
        {
        }

        public static bool IsTribalGathering(Precept_Ritual ritual)
        {
            return ritual != null && ritual.def != null && ritual.def.defName == "VFET_TribalGathering";
        }

        public void Toggle(Precept_Ritual ritual, TargetInfo target)
        {
            if (IsScheduledFor(ritual, target))
            {
                ClearSchedule();
                Messages.Message("VFETribalsAutoGathering.ScheduleDisabled".Translate(), MessageTypeDefOf.NeutralEvent, historical: false);
                SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
                return;
            }

            scheduledRitual = ritual;
            scheduledTarget = target;
            scheduledLocalDayTick = GenLocalDate.DayTick(target.Map);
            ScheduleNextExecutionAfter(GenTicks.TicksAbs);
            Messages.Message("VFETribalsAutoGathering.ScheduleEnabled".Translate(), target, MessageTypeDefOf.NeutralEvent, historical: false);
            SoundDefOf.Tick_Tiny.PlayOneShotOnCamera();
        }

        public bool IsScheduledFor(Precept_Ritual ritual, TargetInfo target)
        {
            return scheduledRitual == ritual && scheduledTarget == target;
        }

        public override void GameComponentTick()
        {
            if (scheduledRitual == null || nextExecutionAbsTick < 0L || GenTicks.TicksAbs < nextExecutionAbsTick)
            {
                return;
            }

            if (!ScheduledTargetIsValid())
            {
                ClearSchedule();
                Messages.Message("VFETribalsAutoGathering.ScheduleCancelled".Translate(), MessageTypeDefOf.NeutralEvent, historical: false);
                return;
            }

            ScheduleNextExecutionAfter(GenTicks.TicksAbs);
            TryExecuteScheduledGathering();
        }

        public override void LoadedGame()
        {
            if (scheduledRitual != null && scheduledTarget.IsValid && scheduledTarget.Map != null && scheduledLocalDayTick >= 0)
            {
                ScheduleNextExecutionAfter(GenTicks.TicksAbs);
            }
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref scheduledRitual, "scheduledRitual");
            Scribe_TargetInfo.Look(ref scheduledTarget, "scheduledTarget");
            Scribe_Values.Look(ref scheduledLocalDayTick, "scheduledLocalDayTick", -1);
            Scribe_Values.Look(ref nextExecutionAbsTick, "nextExecutionAbsTick", -1L);

            if (Scribe.mode == LoadSaveMode.PostLoadInit && (scheduledRitual == null || !scheduledTarget.IsValid || scheduledLocalDayTick < 0))
            {
                ClearSchedule();
            }
        }

        private bool ScheduledTargetIsValid()
        {
            if (!IsTribalGathering(scheduledRitual) || !scheduledTarget.IsValid || scheduledTarget.Map == null || !scheduledTarget.HasThing)
            {
                return false;
            }

            Thing targetThing = scheduledTarget.Thing;
            return targetThing != null && targetThing.Spawned && !targetThing.Destroyed && scheduledRitual.CanUseTarget(scheduledTarget, null).canUse;
        }

        private void TryExecuteScheduledGathering()
        {
            if (scheduledRitual.behavior == null || scheduledRitual.behavior.def == null || scheduledRitual.behavior.def.roles == null || !scheduledRitual.behavior.CanStartRitualNow(scheduledTarget, scheduledRitual).NullOrEmpty())
            {
                return;
            }

            Dialog_BeginRitual.PawnFilter filter = delegate(Pawn pawn, bool voluntary, bool allowOtherIdeos)
            {
                if (pawn.GetLord() != null || pawn.IsSubhuman)
                {
                    return false;
                }

                if (pawn.RaceProps.Animal && !scheduledRitual.behavior.def.roles.Any(role => role.AppliesToPawn(pawn, out _, scheduledTarget, null, null, null, skipReason: true)))
                {
                    return false;
                }

                return !scheduledRitual.ritualOnlyForIdeoMembers || scheduledRitual.def.allowSpectatorsFromOtherIdeos || pawn.Ideo == scheduledRitual.ideo || !voluntary || allowOtherIdeos || pawn.IsPrisonerOfColony || pawn.RaceProps.Animal;
            };

            RitualRoleAssignments assignments = Dialog_BeginRitual.CreateRitualRoleAssignments(scheduledRitual, scheduledTarget, scheduledTarget.Map, filter, null, null, null);
            assignments.FillPawns(filter, scheduledTarget);
            if (!AssignmentsCanStart(assignments))
            {
                return;
            }

            scheduledRitual.behavior.TryExecuteOn(scheduledTarget, null, scheduledRitual, null, assignments, playerForced: true);
        }

        private bool AssignmentsCanStart(RitualRoleAssignments assignments)
        {
            if (!assignments.Participants.Any())
            {
                return false;
            }

            if (scheduledRitual.behavior.SpectatorsRequired() && assignments.SpectatorsForReading.Count == 0)
            {
                return false;
            }

            foreach (IGrouping<string, RitualRole> roleGroup in assignments.RoleGroups())
            {
                RitualRole firstRole = roleGroup.First();
                List<Pawn> assignedPawns = roleGroup.SelectMany(role => assignments.AssignedPawns(role)).ToList();
                int requiredPawnCount = roleGroup.Count(role => role.required);
                if (requiredPawnCount > 0 && assignedPawns.Count < requiredPawnCount)
                {
                    return false;
                }

                foreach (Pawn pawn in assignedPawns)
                {
                    bool stillAddToPawnList;
                    if (assignments.PawnNotAssignableReason(pawn, firstRole, out stillAddToPawnList) != null)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void ScheduleNextExecutionAfter(long currentAbsTick)
        {
            int currentLocalDayTick = GenDate.DayTick(currentAbsTick, Find.WorldGrid.LongLatOf(scheduledTarget.Map.Tile).x);
            int ticksUntilNextExecution = scheduledLocalDayTick - currentLocalDayTick;
            if (ticksUntilNextExecution <= 0)
            {
                ticksUntilNextExecution += GenDate.TicksPerDay;
            }

            nextExecutionAbsTick = currentAbsTick + ticksUntilNextExecution;
        }

        private void ClearSchedule()
        {
            scheduledRitual = null;
            scheduledTarget = TargetInfo.Invalid;
            scheduledLocalDayTick = -1;
            nextExecutionAbsTick = -1L;
        }
    }
}
