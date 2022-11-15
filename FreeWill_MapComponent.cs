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
        private Dictionary<Pawn, int> lastBored;
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
        public float PercentPawnsMechHaulers { get { return percentPawnsMechHaulers; } }
        private float percentPawnsMechHaulers;
        public float SuppressionNeed { get { return suppressionNeed; } }
        private float suppressionNeed;
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
        public bool AlertMechDamaged { get { return alertMechDamaged; } }
        private bool alertMechDamaged;

        private int actionCounter = 0;

        public FreeWill_MapComponent(Map map) : base(map)
        {
            this.priorities = new Dictionary<Pawn, Dictionary<WorkTypeDef, Priority>> { };
            this.lastBored = new Dictionary<Pawn, int> { };
            this.activeAlertsField = AccessTools.Field(typeof(AlertsReadout), "AllAlerts");
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();

            mapCompnentsCheckActions = mapCompnentsCheckActions ?? new Action[]{
                checkPrisonerHealth,
                checkPetsHealth,
                checkColonyHealth,
                checkPercentPawnsMechHaulers,
                checkThingsDeteriorating,
                checkBlight,
                checkMapFire,
                checkRefuelNeeded,
                checkActiveAlerts,
                checkSuppressionNeed,
            };
            worldComp = Find.World.GetComponent<FreeWill_WorldComponent>();

            try
            {
                getMapComponentTickAction()();
                this.actionCounter++;
            }
            catch (System.Exception e)
            {
                Log.ErrorOnce($"Free Will: could not perform tick action: mapTickCounter = {this.actionCounter}: {e}", 14147584);
            }
        }

        private Action getMapComponentTickAction()
        {
            try
            {
                if (this.actionCounter < mapCompnentsCheckActions.Length)
                {
                    return mapCompnentsCheckActions[this.actionCounter];
                }
                int i = this.actionCounter - mapCompnentsCheckActions.Length;
                List<WorkTypeDef> workTypeDefs = DefDatabase<WorkTypeDef>.AllDefsListForReading;
                List<Pawn> pawns = this.map.mapPawns.FreeColonistsSpawned;
                if (i >= workTypeDefs.Count * pawns.Count)
                {
                    this.actionCounter = 0;
                    return getMapComponentTickAction();
                }
                int pawnIndex = i / workTypeDefs.Count;
                int worktypeIndex = i % workTypeDefs.Count;
                return () => setPriorityAction(pawns[pawnIndex], workTypeDefs[worktypeIndex]);
            }
            // show stack trace
            catch (System.Exception e)
            {
                Log.ErrorOnce($"Free Will: could not get map component tick action: mapTickCounter = {this.actionCounter}: {e}", 14847584);
                return () => { };
            }
        }

        public void UpdateLastBored(Pawn pawn)
        {
            this.lastBored[pawn] = Find.TickManager.TicksGame;
        }

        public int GetLastBored(Pawn pawn)
        {
            if (this.lastBored.ContainsKey(pawn))
            {
                return this.lastBored[pawn];
            }
            return 0;
        }

        public Dictionary<WorkTypeDef, Priority> GetPriorities(Pawn pawn)
        {
            if (!this.priorities.ContainsKey(pawn))
            {
                this.priorities[pawn] = new Dictionary<WorkTypeDef, Priority>();
            }
            return this.priorities[pawn];
        }

        private void setPriorityAction(Pawn pawn, WorkTypeDef workTypeDef)
        {
            if (pawn == null)
            {
                Log.ErrorOnce($"Free Will: pawn is null: mapTickCounter = {this.actionCounter}", 584624);
                return;
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
                    return;
                }
            }
            worldComp.EnsureFreeWillStatusIsCorrect(pawn);
            try
            {
                if (priorities == null)
                {
                    Log.ErrorOnce("Free Will: priorities is null", 2274889);
                    priorities = new Dictionary<Pawn, Dictionary<WorkTypeDef, Priority>>();
                }
                if (!priorities.ContainsKey(pawn))
                {
                    priorities[pawn] = new Dictionary<WorkTypeDef, Priority>();
                }
                priorities[pawn][workTypeDef] = new Priority(pawn, workTypeDef);
                if (worldComp.HasFreeWill(pawn))
                {
                    priorities[pawn][workTypeDef].ApplyPriorityToGame();
                }
                return;
            }
            catch (System.Exception)
            {
                Log.ErrorOnce("Free Will: could not set priorities for pawn: " + pawn.Name + ": marking " + pawn.Name + " as not having free will", 752116445);
                var ok = worldComp.TryRemoveFreeWill(pawn);
                if (!ok)
                {
                    Log.ErrorOnce("Free Will: could not remove free will", 752116446);
                }
                return;
            }
        }

        private void checkSuppressionNeed()
        {
            suppressionNeed = 0.0f;
            foreach (Pawn slave in map?.mapPawns?.SlavesOfColonySpawned)
            {
                Need_Suppression need_Suppression = slave?.needs.TryGetNeed<Need_Suppression>();
                if (need_Suppression == null)
                {
                    continue;
                }
                if (!need_Suppression.CanBeSuppressedNow)
                {
                    continue;
                }
                suppressionNeed = 0.2f;
                if (need_Suppression.IsHigh)
                {
                    suppressionNeed = 0.4f;
                    return;
                }
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

        private void checkPercentPawnsMechHaulers()
        {
            List<Pawn> pawnsInFaction = this.map.mapPawns.PawnsInFaction(Faction.OfPlayer);
            if (pawnsInFaction == null)
            {
                return;
            }
            float numMechHaulers = 0;
            float total = 0;
            foreach (Pawn pawn in pawnsInFaction)
            {
                if (!pawn.IsColonistPlayerControlled && !pawn.IsColonyMechPlayerControlled)
                {
                    continue;
                }
                if (!pawn.Awake() || pawn.Downed || pawn.Dead || pawn.IsCharging())
                {
                    continue;
                }
                if (pawn.IsColonyMechPlayerControlled && pawn.RaceProps.mechEnabledWorkTypes.Contains(WorkTypeDefOf.Hauling))
                {
                    numMechHaulers++;
                }
                total++;
            }
            percentPawnsMechHaulers = (total == 0.0f) ? 0.0f : numMechHaulers / total;
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
                this.alertMechDamaged = false;
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
                        case Alert_MechDamaged a:
                            this.alertMechDamaged = true;
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
