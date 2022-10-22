using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using System.Linq;
using RimWorld;
using HarmonyLib;

namespace FreeWill
{
    public class FreeWill_MapComponent : MapComponent
    {
        private static Action[] mapCompnentsCheckActions;

        private Dictionary<Pawn, Dictionary<WorkTypeDef, Priority>> priorities;
        private readonly FieldInfo activeAlertsField;
        private FreeWill_WorldComponent worldComp;

        public int NumPawns
        {
            get
            {
                if (numPawns == 0)
                {
                    numPawns = this.map.mapPawns.FreeColonistsSpawnedCount;
                    return numPawns;
                }
                return numPawns;
            }
        }
        private int numPawns;

        public float PercentPawnsNeedingTreatment { get { return percentPawnsNeedingTreatment; } }
        private float percentPawnsNeedingTreatment;
        public int NumPetsNeedingTreatment { get { return numPetsNeedingTreatment; } }
        private int numPetsNeedingTreatment;
        public int NumPrisonersNeedingTreatment { get { return numPrisonersNeedingTreatment; } }
        private int numPrisonersNeedingTreatment;
        public float PercentPawnsDowned { get { return percentPawnsDowned; } }
        private float percentPawnsDowned;
        public bool ThingsDeteriorating { get { return thingsDeteriorating; } }
        private bool thingsDeteriorating;
        public int MapFires { get { return mapFires; } }
        private int mapFires;
        public bool HomeFire { get { return homeFire; } }
        private bool homeFire;
        public bool RefuelNeededNow { get { return refuelNeededNow; } }
        private bool refuelNeededNow;
        public bool RefuelNeeded { get { return refuelNeeded; } }
        private bool refuelNeeded;
        public bool PlantsBlighted { get { return plantsBlighted; } }
        private bool plantsBlighted;
        public bool NeedWarmClothes { get { return needWarmClothes; } }
        private bool needWarmClothes;
        public bool AlertColonistLeftUnburied { get { return alertColonistLeftUnburied; } }
        private bool alertColonistLeftUnburied;
        public bool AlertAnimalRoaming { get { return alertAnimalRoaming; } }
        private bool alertAnimalRoaming;
        public bool AlertLowFood { get { return alertLowFood; } }
        private bool alertLowFood;

        private int mapTickCounter = 0;

        public FreeWill_MapComponent(Map map) : base(map)
        {
            priorities = new Dictionary<Pawn, Dictionary<WorkTypeDef, Priority>> { };
            activeAlertsField = AccessTools.Field(typeof(AlertsReadout), "AllAlerts");
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            mapCompnentsCheckActions = mapCompnentsCheckActions ?? new Action[]{
                checkPrisonerHealth,
                checkPetsHealth,
                checkColonyHealth,
                checkThingsDeteriorating,
                checkBlight,
                checkMapFire,
                checkRefuelNeeded,
                checkActiveAlerts,
            };
            worldComp = worldComp ?? Find.World.GetComponent<FreeWill_WorldComponent>();

            try
            {
                getMapComponentTickAction(this.mapTickCounter)();
                this.mapTickCounter++;
            }
            catch (System.Exception e)
            {
                Log.ErrorOnce($"Free Will: could not perform tick action: mapTickCounter = {this.mapTickCounter}: {e}", 14147584);
            }
        }

        private Action getMapComponentTickAction(int i)
        {
            try
            {
                if (i < mapCompnentsCheckActions.Length)
                {
                    return mapCompnentsCheckActions[i];
                }
                i -= mapCompnentsCheckActions.Length;
                int worktypeCount = DefDatabase<WorkTypeDef>.AllDefsListForReading.Count();
                int pawnCount = this.map.mapPawns.FreeColonistsSpawnedCount;
                if (i >= worktypeCount * pawnCount)
                {
                    this.mapTickCounter = 0;
                    return getMapComponentTickAction(0);
                }
                int pawnIndex = i / worktypeCount;
                int worktypeIndex = i % worktypeCount;
                return setPriorityAction(pawnIndex, worktypeIndex);
            }
            // show stack trace
            catch (System.Exception e)
            {
                Log.ErrorOnce($"Free Will: could not get map component tick action: mapTickCounter = {this.mapTickCounter}: {e}", 14847584);
                return () => { };
            }
        }

        public Dictionary<WorkTypeDef, Priority> GetPriorities(Pawn pawn)
        {
            if (!this.priorities.ContainsKey(pawn))
            {
                this.priorities[pawn] = new Dictionary<WorkTypeDef, Priority>();
            }
            return this.priorities[pawn];
        }

        private Action setPriorityAction(int pawnIndex, int worktypeIndex)
        {
            Pawn pawn = map.mapPawns.FreeColonistsSpawned[pawnIndex];
            if (pawn == null)
            {
                return () => { };
            }
            if (pawn.IsSlaveOfColony)
            {
                if (worldComp.HasFreeWill(pawn))
                {
                    var ok = worldComp.TryRemoveFreeWill(pawn);
                    if (!ok)
                    {
                        Log.ErrorOnce("Free Will: could not remove free will from slave", 164752145);
                    }
                    return () => { };
                }
            }
            worldComp.EnsureFreeWillStatusIsCorrect(pawn);
            try
            {
                var workTypeDef = DefDatabase<WorkTypeDef>.AllDefsListForReading[worktypeIndex];
                if (priorities == null)
                {
                    priorities = new Dictionary<Pawn, Dictionary<WorkTypeDef, Priority>>();
                }
                if (!priorities.ContainsKey(pawn))
                {
                    priorities[pawn] = new Dictionary<WorkTypeDef, Priority>();
                }
                priorities[pawn][workTypeDef] = new Priority(pawn, workTypeDef);
                if (worldComp.HasFreeWill(pawn))
                {
                    return priorities[pawn][workTypeDef].ApplyPriorityToGame;
                }
                return () => { };
            }
            catch (System.Exception)
            {
                Log.ErrorOnce("Free Will: could not set priorities for pawn: " + pawn.Name + ": marking " + pawn.Name + " as not having free will", 752116445);
                var ok = worldComp.TryRemoveFreeWill(pawn);
                if (!ok)
                {
                    Log.ErrorOnce("Free Will: could not remove free will", 752116446);
                }
                return () => { };
            }
        }

        private void checkPrisonerHealth()
        {
            var prisonersInColony = map?.mapPawns?.PrisonersOfColony;
            numPrisonersNeedingTreatment =
                (prisonersInColony == null)
                    ? 0
                    : (from prisoners in map?.mapPawns?.PrisonersOfColony
                       where prisoners.health.HasHediffsNeedingTend()
                       select prisoners).Count();
        }

        private void checkPetsHealth()
        {
            var pawnsInFaction = map?.mapPawns?.PawnsInFaction(Faction.OfPlayer);
            numPetsNeedingTreatment =
                (pawnsInFaction == null)
                    ? 0
                    : (from p in pawnsInFaction
                       where p.RaceProps.Animal && p.health.HasHediffsNeedingTend()
                       select p).Count();
        }

        private void checkColonyHealth()
        {
            numPawns = map?.mapPawns?.FreeColonistsSpawnedCount ?? 0;
            percentPawnsDowned = 0.0f;
            percentPawnsNeedingTreatment = 0.0f;
            float colonistWeight = 1.0f / numPawns;
            var freeColonistsSpawned = map?.mapPawns?.FreeColonistsSpawned;
            if (freeColonistsSpawned == null)
            {
                return;
            }
            foreach (Pawn pawn in freeColonistsSpawned)
            {
                bool inBed = pawn.CurrentBed() != null;
                if (pawn.Downed && !inBed)
                {
                    percentPawnsDowned += colonistWeight;
                }
                if (pawn.health.HasHediffsNeedingTend())
                {
                    percentPawnsNeedingTreatment += colonistWeight;
                }
            }
        }

        private void checkThingsDeteriorating()
        {
            this.thingsDeteriorating = false;
            var thingsPotentiallyNeedingHauling = this.map?.listerHaulables?.ThingsPotentiallyNeedingHauling();
            if (thingsPotentiallyNeedingHauling == null)
            {
                return;
            }
            foreach (Thing thing in thingsPotentiallyNeedingHauling)
            {
                if (thing.IsInValidStorage() || SteadyEnvironmentEffects.FinalDeteriorationRate(thing) == 0)
                {
                    continue;
                }
                this.thingsDeteriorating = true;
                return;
            }
        }

        private void checkBlight()
        {
            try
            {
                this.plantsBlighted = false;
                if ((this.worldComp?.settings?.ConsiderPlantsBlighted ?? 0.0f) == 0.0f)
                {
                    return;
                }
                Thing thing = null;
                var plants = this.map?.listerThings?.ThingsInGroup(ThingRequestGroup.Plant);
                if (plants == null)
                {
                    return;
                }
                (from x in plants
                 where ((Plant)x).Blighted
                 select x).TryRandomElement(out thing);
                this.plantsBlighted = (thing != null);
            }
            catch (System.Exception err)
            {
                Log.Message("could not check blight levels on map");
                Log.Message(err.ToString());
                Log.Message("this consideration will be disabled in the mod settings to avoid future errors");
                this.worldComp.settings.ConsiderPlantsBlighted = 0.0f;
                this.plantsBlighted = false;
                return;
            }
        }

        private void checkMapFire()
        {
            List<Thing> fires = this.map?.listerThings?.ThingsOfDef(ThingDefOf.Fire);
            mapFires = fires?.Count ?? 0;
            homeFire = false;
            if (fires == null)
            {
                return;
            }
            foreach (Thing fire in fires)
            {
                mapFires++;
                if (this.map.areaManager.Home[fire.Position] && !fire.Position.Fogged(fire.Map))
                {
                    homeFire = true;
                    return;
                }
            }
        }

        private void checkRefuelNeeded()
        {
            refuelNeeded = false;
            refuelNeededNow = false;
            List<Thing> refuelableThings = this.map?.listerThings?.ThingsInGroup(ThingRequestGroup.Refuelable);
            if (refuelableThings == null)
            {
                return;
            }
            foreach (Thing thing in refuelableThings)
            {
                CompRefuelable refuelable = thing.TryGetComp<CompRefuelable>();
                if (refuelable == null)
                {
                    continue;
                }
                if (!refuelable.allowAutoRefuel)
                {
                    continue;
                }
                if (!refuelable.HasFuel)
                {
                    refuelNeeded = true;
                    refuelNeededNow = true;
                    return;
                }
                if (!refuelable.IsFull)
                {
                    refuelNeeded = true;
                    continue;
                }
            }
        }

        public void checkActiveAlerts()
        {
            try
            {
                UIRoot_Play ui = Find.UIRoot as UIRoot_Play;
                if (ui == null)
                {
                    return;
                }
                // unset all the alerts
                this.alertLowFood = false;
                this.alertAnimalRoaming = false;
                this.needWarmClothes = false;
                this.alertColonistLeftUnburied = false;
                // check current alerts
                foreach (Alert alert in (List<Alert>)activeAlertsField.GetValue(ui.alerts))
                {
                    if (!alert.Active)
                    {
                        continue;
                    }
                    switch (alert)
                    {
                        case Alert_LowFood a:
                            this.alertLowFood = true;
                            break;
                        case Alert_NeedWarmClothes a:
                            this.needWarmClothes = true;
                            break;
                        case Alert_AnimalRoaming a:
                            this.alertAnimalRoaming = true;
                            break;
                        case Alert_ColonistLeftUnburied a:
                            if (this.map.mapPawns.AnyFreeColonistSpawned)
                            {
                                List<Thing> list = this.map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.Corpse));
                                for (int i = 0; i < list.Count; i++)
                                {
                                    Corpse corpse = (Corpse)list[i];
                                    if (Alert_ColonistLeftUnburied.IsCorpseOfColonist(corpse))
                                    {
                                        this.alertColonistLeftUnburied = true;
                                        break;
                                    }
                                }
                                if (this.alertColonistLeftUnburied)
                                {
                                    break;
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch
            {
                Log.ErrorOnce("Free Will: could not check active alerts", 58548754);
            }
        }
    }
}
