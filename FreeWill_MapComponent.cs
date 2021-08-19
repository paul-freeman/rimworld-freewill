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
        public float TotalFood { get { return totalFood; } }
        private float totalFood;
        public bool NeedWarmClothes { get { return needWarmClothes; } }
        private bool needWarmClothes;
        public bool AlertColonistLeftUnburied { get { return alertColonistLeftUnburied; } }
        private bool alertColonistLeftUnburied;

        const int NUM_PREP_CASES = 7;
        private int counter = -NUM_PREP_CASES;

        public FreeWill_MapComponent(Map map) : base(map)
        {
            priorities = new Dictionary<Pawn, Dictionary<WorkTypeDef, Priority>> { };
            activeAlertsField = AccessTools.Field(typeof(AlertsReadout), "AllAlerts");
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            try
            {
                if (worldComp == null)
                {
                    worldComp = Find.World.GetComponent<FreeWill_WorldComponent>();
                }

                int i = counter;
                counter++;
                switch (i)
                {
                    case -7:
                        checkColonyHealth();
                        return;
                    case -6:
                        checkThingsDeteriorating();
                        return;
                    case -5:
                        checkBlight();
                        return;
                    case -4:
                        checkColonyFoodLevel();
                        return;
                    case -3:
                        checkMapFire();
                        return;
                    case -2:
                        checkRefuelNeeded();
                        return;
                    case -1:
                        checkActiveAlerts();
                        return;
                    default:
                        int worktypeCount = DefDatabase<WorkTypeDef>.AllDefsListForReading.Count();
                        int pawnCount = this.map.mapPawns.FreeColonistsSpawnedCount;
                        if (i >= worktypeCount * pawnCount)
                        {
                            counter = -NUM_PREP_CASES;
                            return;
                        }
                        int pawnIndex = i / worktypeCount;
                        int worktypeIndex = i % worktypeCount;
                        SetPriorities(pawnIndex, worktypeIndex);
                        return;
                }
            }
            catch (System.Exception err)
            {
                Log.Error("could not set priority: " + err.Message);
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

        private void SetPriorities(int pawnIndex, int worktypeIndex)
        {
            Pawn pawn = map.mapPawns.FreeColonistsSpawned[pawnIndex];
            if (pawn == null)
            {
                return;
            }
            if (pawn.IsSlaveOfColony)
            {
                if (worldComp.HasFreeWill(pawn))
                {
                    var ok = worldComp.TryRemoveFreeWill(pawn);
                    if (!ok)
                    {
                        Log.ErrorOnce("could not remove free will from slave", 164752145);
                    }
                    return;
                }
            }
            worldComp.CheckFreeWillStatus(pawn);
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
                    priorities[pawn][workTypeDef].ApplyPriorityToGame();
                }
            }
            catch (System.Exception err)
            {
                Log.Error("could not set priorities for pawn: " + pawn.Name + ": marking " + pawn.Name + " as not having free will: " + err.ToString());
                worldComp.TryRemoveFreeWill(pawn);
            }
        }

        private void checkColonyHealth()
        {
            numPetsNeedingTreatment =
                (from p in map.mapPawns.PawnsInFaction(Faction.OfPlayer)
                 where p.RaceProps.Animal && p.health.HasHediffsNeedingTend()
                 select p).Count();
            numPawns = map.mapPawns.FreeColonistsSpawnedCount;
            percentPawnsDowned = 0.0f;
            percentPawnsNeedingTreatment = 0.0f;
            float colonistWeight = 1.0f / numPawns;
            foreach (Pawn pawn in map.mapPawns.FreeColonistsSpawned)
            {
                if (pawn.Downed)
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
            thingsDeteriorating = false;
            foreach (Thing thing in map.listerHaulables.ThingsPotentiallyNeedingHauling())
            {
                if (SteadyEnvironmentEffects.FinalDeteriorationRate(thing) != 0)
                {
                    thingsDeteriorating = true;
                    return;
                }
            }
        }

        private void checkBlight()
        {
            try
            {
                if (worldComp.settings.ConsiderPlantsBlighted == 0.0f)
                {
                    return;
                }

                Thing thing = null;
                (from x in map.listerThings.ThingsInGroup(ThingRequestGroup.Plant)
                 where ((Plant)x).Blighted
                 select x).TryRandomElement(out thing);
                if (thing == null)
                {
                    this.plantsBlighted = false;
                }
                else
                {
                    this.plantsBlighted = true;
                }
            }
            catch (System.Exception err)
            {
                Log.Message("could not check blight levels on map");
                Log.Message(err.ToString());
                Log.Message("this consideration will be disabled in the mod settings to avoid future errors");
                worldComp.settings.ConsiderPlantsBlighted = 0.0f;
                this.plantsBlighted = false;
                return;
            }
        }

        private void checkColonyFoodLevel()
        {
            totalFood = map.resourceCounter.TotalHumanEdibleNutrition;
        }

        private void checkMapFire()
        {
            List<Thing> list = this.map.listerThings.ThingsOfDef(ThingDefOf.Fire);
            mapFires = list.Count;
            homeFire = false;
            for (int j = 0; j < list.Count; j++)
            {
                mapFires++;
                Thing thing = list[j];
                if (this.map.areaManager.Home[thing.Position] && !thing.Position.Fogged(thing.Map))
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
            List<Thing> list = this.map.listerThings.ThingsInGroup(ThingRequestGroup.Refuelable);
            foreach (Thing thing in list)
            {
                CompRefuelable refuel = thing.TryGetComp<CompRefuelable>();
                if (refuel == null)
                {
                    continue;
                }
                if (!refuel.HasFuel)
                {
                    refuelNeeded = true;
                    refuelNeededNow = true;
                    return;
                }
                if (!refuel.IsFull)
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
                        case Alert_NeedWarmClothes a:
                            this.needWarmClothes = true;
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
                Log.Error("could not check active alerts");
            }
        }
    }
}
