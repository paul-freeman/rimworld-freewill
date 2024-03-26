using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using System.Linq;
using RimWorld;
using HarmonyLib;
using Verse.AI;

namespace FreeWill
{
    public class FreeWill_MapComponent : MapComponent
    {
        private static readonly string[] mapComponentCheckActions = new string[]{
            "checkPrisonerHealth",
            "checkPetsHealth",
            "checkColonyHealth",
            "checkPercentPawnsMechHaulers",
            "checkThingsDeteriorating",
            "checkBlight",
            "checkMapFire",
            "checkRefuelNeeded",
            "checkActiveAlerts",
            "checkSuppressionNeed",
        };

        private static readonly int couldNotSetPrioritiesHash = ("FreeWill" + "could not set priorities for pawn").GetHashCode();

        private Dictionary<Pawn, Dictionary<WorkTypeDef, Priority>> priorities;
        private readonly Dictionary<Pawn, int> lastBored;
        private readonly FieldInfo activeAlertsField;
        private FreeWill_WorldComponent worldComp;

        public List<Pawn> PawnsInFaction { get; private set; }
        public int NumPawns => map.mapPawns.FreeColonistsSpawnedCount;


        public float PercentPawnsNeedingTreatment { get; private set; }
        public int NumPetsNeedingTreatment { get; private set; }
        public int NumPrisonersNeedingTreatment { get; private set; }
        public float PercentPawnsDowned { get; private set; }
        public float PercentPawnsMechHaulers { get; private set; }
        public float SuppressionNeed { get; private set; }
        public Thing ThingsDeteriorating { get; private set; }
        public int MapFires { get; private set; }
        public bool HomeFire { get; private set; }
        public bool RefuelNeededNow { get; private set; }
        public bool RefuelNeeded { get; private set; }
        public bool PlantsBlighted { get; private set; }
        public bool NeedWarmClothes { get; private set; }
        public bool AlertColonistLeftUnburied { get; private set; }
        public bool AlertAnimalRoaming { get; private set; }
        public bool AlertLowFood { get; private set; }
        public bool AlertMechDamaged { get; private set; }

        private string pawnIndexCache = "unknown";
        public bool AreaHasHaulables { get; private set; }
        public bool AreaHasFilth { get; private set; }
        public List<Thought> AllThoughts = new List<Thought>();
        public List<WorkTypeDef> DisabledWorkTypes = new List<WorkTypeDef>();

        private int actionCounter = 0;

        public FreeWill_MapComponent(Map map) : base(map)
        {
            priorities = new Dictionary<Pawn, Dictionary<WorkTypeDef, Priority>> { };
            lastBored = new Dictionary<Pawn, int> { };
            activeAlertsField = AccessTools.Field(typeof(AlertsReadout), "AllAlerts");
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            FreeWillUtility.UpdateMapComponent(this);
        }

        public override void MapComponentTick()
        {
            base.MapComponentTick();
            FreeWillUtility.UpdateMapComponent(this);

            try
            {
                worldComp = Find.World.GetComponent<FreeWill_WorldComponent>();

                GetMapComponentTickAction();
                actionCounter++;
            }
            catch (Exception e)
            {
                Log.ErrorOnce($"Free Will: could not perform tick action: mapTickCounter = {actionCounter}: {e.Message}", 14147584);
            }
        }

        private void GetMapComponentTickAction()
        {
            try
            {
                if (actionCounter < mapComponentCheckActions.Length)
                {
                    switch (mapComponentCheckActions[actionCounter])
                    {
                        case "checkPrisonerHealth":
                            _ = CheckPrisonerHealth();
                            break;
                        case "checkPetsHealth":
                            _ = CheckPetsHealth();
                            break;
                        case "checkColonyHealth":
                            _ = CheckColonyHealth();
                            break;
                        case "checkPercentPawnsMechHaulers":
                            _ = CheckPercentPawnsMechHaulers();
                            break;
                        case "checkThingsDeteriorating":
                            _ = CheckThingsDeteriorating();
                            break;
                        case "checkBlight":
                            PlantsBlighted = CheckBlight();
                            break;
                        case "checkMapFire":
                            _ = CheckMapFire();
                            break;
                        case "checkRefuelNeeded":
                            _ = CheckRefuelNeeded();
                            break;
                        case "checkActiveAlerts":
                            _ = CheckActiveAlerts();
                            break;
                        case "checkSuppressionNeed":
                            _ = CheckSuppressionNeed();
                            break;
                        default:
                            Log.ErrorOnce($"Free Will: unknown map component tick action: {mapComponentCheckActions[actionCounter]}", 14147585);
                            break;
                    }
                    return;
                }
                int i = actionCounter - mapComponentCheckActions.Length;
                List<WorkTypeDef> workTypeDefs = DefDatabase<WorkTypeDef>.AllDefsListForReading;
                List<Pawn> pawns = map.mapPawns.FreeColonistsSpawned;
                if (i >= workTypeDefs.Count * pawns.Count)
                {
                    actionCounter = 0;
                    _ = CheckPrisonerHealth();
                    return;
                }
                int pawnIndex = i / workTypeDefs.Count;
                int worktypeIndex = i % workTypeDefs.Count;
                Pawn pawn = pawns[pawnIndex];
                if (pawn.GetUniqueLoadID() != pawnIndexCache)
                {
                    // new pawn, so check their area
                    BeautyUtility.FillBeautyRelevantCells(pawn.Position, pawn.Map);
                    AreaHasHaulables = CheckIfAreaHasHaulables(pawn, BeautyUtility.beautyRelevantCells);
                    AreaHasFilth = CheckIfAreaHasFilth(pawn, BeautyUtility.beautyRelevantCells);
                    pawnIndexCache = pawn.GetUniqueLoadID();
                    // update their thoughts
                    pawn.needs.mood.thoughts.GetAllMoodThoughts(AllThoughts);
                    // update their disabled work types
                    DisabledWorkTypes = pawn.GetDisabledWorkTypes(permanentOnly: true);

                }
                string pawnKey = pawn.GetUniqueLoadID();
                WorkTypeDef workTypeDef = workTypeDefs[worktypeIndex];
                _ = SetPriorityAction(pawn, pawnKey, workTypeDef);
            }
            catch (Exception e)
            {
                throw new Exception("could not get map component tick action: " + e.Message);
            }
        }

        public void UpdateLastBored(Pawn pawn)
        {
            lastBored[pawn] = Find.TickManager.TicksGame;
        }

        public int GetLastBored(Pawn pawn)
        {
            return lastBored.ContainsKey(pawn) ? lastBored[pawn] : 0;
        }

        public Priority GetPriority(Pawn pawn, WorkTypeDef workTypeDef)
        {
            if (!priorities.ContainsKey(pawn))
            {
                priorities[pawn] = new Dictionary<WorkTypeDef, Priority>();
            }
            if (!priorities[pawn].ContainsKey(workTypeDef))
            {
                priorities[pawn][workTypeDef] = new Priority(pawn, workTypeDef);
                priorities[pawn][workTypeDef].Compute();
            }
            return priorities[pawn][workTypeDef];
        }

        private string SetPriorityAction(Pawn pawn, string pawnKey, WorkTypeDef workTypeDef)
        {
            string msg = pawn.Name.ToStringShort + " " + workTypeDef.defName;
            if (pawn == null)
            {
                Log.ErrorOnce($"Free Will: pawn is null: mapTickCounter = {actionCounter}", 584624);
                return msg;
            }
            if (!pawn.Awake() || pawn.Downed || pawn.Dead)
            {
                return msg;
            }
            if (pawn.IsSlaveOfColony)
            {
                if (worldComp.HasFreeWill(pawn, pawnKey))
                {
                    bool ok = worldComp.TryRemoveFreeWill(pawn);
                    if (!ok)
                    {
                        Log.ErrorOnce("Free Will: could not remove free will from slave", 164752145);
                    }
                    return msg;
                }
            }
            worldComp.EnsureFreeWillStatusIsCorrect(pawn, pawnKey);
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
                if (!priorities[pawn].ContainsKey(workTypeDef))
                {
                    priorities[pawn][workTypeDef] = new Priority(pawn, workTypeDef);
                }
                if (worldComp.HasFreeWill(pawn, pawnKey))
                {
                    priorities[pawn][workTypeDef].Compute();
                    priorities[pawn][workTypeDef].ApplyPriorityToGame();
                }
                return msg;
            }
            catch (Exception e)
            {
                if (Prefs.DevMode)
                {
                    throw new Exception("could not set priorities for pawn: " + pawn.Name + ": " + e.Message);
                }
                Log.ErrorOnce("Free Will: could not set priorities for pawn: " + pawn.Name + ": marking " + pawn.Name + " as not having free will: " + e, couldNotSetPrioritiesHash);
                bool ok = worldComp.TryRemoveFreeWill(pawn);
                if (!ok)
                {
                    Log.ErrorOnce("Free Will: could not remove free will", 752116446);
                }
                return msg;
            }
        }

        private string CheckSuppressionNeed()
        {
            try
            {
                SuppressionNeed = 0.0f;
                List<Pawn> slavesOfColonySpawned = map?.mapPawns?.SlavesOfColonySpawned;
                if (slavesOfColonySpawned == null)
                {
                    return "checkSuppressionNeed";
                }
                foreach (Pawn slave in slavesOfColonySpawned)
                {
                    Need_Suppression needsSuppression = slave?.needs?.TryGetNeed<Need_Suppression>();
                    if (needsSuppression == null)
                    {
                        continue;
                    }
                    if (!needsSuppression.CanBeSuppressedNow)
                    {
                        continue;
                    }
                    SuppressionNeed = 0.2f;
                    if (needsSuppression.IsHigh)
                    {
                        SuppressionNeed = 0.4f;
                        return "checkSuppressionNeed";
                    }
                }
                return "checkSuppressionNeed";
            }
            catch (Exception e)
            {
                throw new Exception("could not check suppression need", e);
            }
        }

        private string CheckPrisonerHealth()
        {
            List<Pawn> prisonersInColony = map?.mapPawns?.PrisonersOfColony;
            NumPrisonersNeedingTreatment =
                (prisonersInColony == null)
                    ? 0
                    : (from prisoners in map?.mapPawns?.PrisonersOfColony
                       where prisoners.health.HasHediffsNeedingTend()
                       select prisoners).Count();
            return "checkPrisonerHealth";
        }

        private string CheckPetsHealth()
        {
            PawnsInFaction = map?.mapPawns?.PawnsInFaction(Faction.OfPlayer) ?? new List<Pawn>();
            NumPetsNeedingTreatment = (from p in PawnsInFaction
                                       where p.RaceProps.Animal && p.health.HasHediffsNeedingTend()
                                       select p).Count();
            return "checkPetsHealth";
        }

        private string CheckColonyHealth()
        {
            PercentPawnsDowned = 0;
            PercentPawnsNeedingTreatment = 0;
            float colonistWeight = 1f / (map?.mapPawns?.FreeColonistsSpawnedCount ?? 1);
            List<Pawn> freeColonistsSpawned = map?.mapPawns?.FreeColonistsSpawned ?? new List<Pawn>();
            foreach (Pawn pawn in freeColonistsSpawned)
            {
                bool inBed = pawn.CurrentBed() != null;
                if (pawn.Downed && !inBed)
                {
                    PercentPawnsDowned += colonistWeight;
                }
                if (pawn.health.HasHediffsNeedingTend())
                {
                    PercentPawnsNeedingTreatment += colonistWeight;
                }
            }
            return "checkColonyHealth";
        }

        private string CheckPercentPawnsMechHaulers()
        {
            try
            {
                PawnsInFaction = map?.mapPawns?.PawnsInFaction(Faction.OfPlayer) ?? new List<Pawn>();
                float numMechHaulers = 0;
                float total = 0;
                foreach (Pawn pawn in PawnsInFaction)
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
                PercentPawnsMechHaulers = (total == 0.0f) ? 0.0f : numMechHaulers / total;
                return "checkPercentPawnsMechHaulers";
            }
            catch (Exception e)
            {
                throw new Exception("could not check percent of mech haulers", e);
            }
        }

        private string CheckThingsDeteriorating()
        {
            ThingsDeteriorating = null;
            List<Thing> thingsPotentiallyNeedingHauling = map?.listerHaulables?.ThingsPotentiallyNeedingHauling();
            if (thingsPotentiallyNeedingHauling == null)
            {
                return "checkThingsDeteriorating";
            }
            foreach (Thing thing in thingsPotentiallyNeedingHauling)
            {
                if (thing.IsInValidStorage() || SteadyEnvironmentEffects.FinalDeteriorationRate(thing) == 0)
                {
                    continue;
                }
                if (thing.IsDessicated())
                {
                    continue;
                }
                ThingsDeteriorating = thing;
                return "checkThingsDeteriorating";
            }
            return "checkThingsDeteriorating";
        }

        private bool CheckBlight()
        {
            foreach (Thing thing in map?.listerThings?.ThingsOfDef(ThingDefOf.Blight))
            {
                if (thing is Blight)
                {
                    return true;
                }
            }
            return false;
        }

        private string CheckMapFire()
        {
            List<Thing> fires = map?.listerThings?.ThingsOfDef(ThingDefOf.Fire);
            MapFires = fires?.Count ?? 0;
            HomeFire = false;
            if (fires == null)
            {
                return "checkMapFire";
            }
            foreach (Thing fire in fires)
            {
                MapFires++;
                if (map.areaManager.Home[fire.Position] && !fire.Position.Fogged(fire.Map))
                {
                    HomeFire = true;
                    return "checkMapFire";
                }
            }
            return "checkMapFire";
        }

        private string CheckRefuelNeeded()
        {
            RefuelNeeded = false;
            RefuelNeededNow = false;
            List<Thing> refuelableThings = map?.listerThings?.ThingsInGroup(ThingRequestGroup.Refuelable);
            if (refuelableThings == null)
            {
                return "checkRefuelNeeded";
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
                    RefuelNeeded = true;
                    RefuelNeededNow = true;
                    return "checkRefuelNeeded";
                }
                if (!refuelable.IsFull)
                {
                    RefuelNeeded = true;
                    continue;
                }
            }
            return "checkRefuelNeeded";
        }

        public string CheckActiveAlerts()
        {
            try
            {
                if (!(Find.UIRoot is UIRoot_Play ui))
                {
                    return "checkActiveAlerts";
                }
                // unset all the alerts
                AlertLowFood = false;
                AlertAnimalRoaming = false;
                NeedWarmClothes = false;
                AlertColonistLeftUnburied = false;
                AlertMechDamaged = false;
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
                            AlertLowFood = true;
                            break;
                        case Alert_NeedWarmClothes a:
                            NeedWarmClothes = true;
                            break;
                        case Alert_AnimalRoaming a:
                            AlertAnimalRoaming = true;
                            break;
                        case Alert_ColonistLeftUnburied a:
                            if (map.mapPawns.AnyFreeColonistSpawned)
                            {
                                List<Thing> list = map.listerThings.ThingsMatching(ThingRequest.ForGroup(ThingRequestGroup.Corpse));
                                for (int i = 0; i < list.Count; i++)
                                {
                                    Corpse corpse = (Corpse)list[i];
                                    if (Alert_ColonistLeftUnburied.IsCorpseOfColonist(corpse))
                                    {
                                        AlertColonistLeftUnburied = true;
                                        break;
                                    }
                                }
                                if (AlertColonistLeftUnburied)
                                {
                                    break;
                                }
                            }
                            break;
                        case Alert_MechDamaged a:
                            AlertMechDamaged = true;
                            break;
                        default:
                            break;
                    }
                }
                return "checkActiveAlerts";
            }
            catch
            {
                Log.ErrorOnce("Free Will: could not check active alerts", 58548754);
                return "checkActiveAlerts";
            }
        }

        private bool CheckIfAreaHasHaulables(Pawn pawn, List<IntVec3> area)
        {
            return area
                .SelectMany(cell => cell.GetThingList(pawn.Map))
                .Any((t) => IsHaulable(pawn, t));
        }

        private bool IsHaulable(Pawn pawn, Thing t)
        {
            if (!t.def.alwaysHaulable)
            {
                if (!t.def.EverHaulable)
                {
                    return false;
                }
                if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Haul) == null && !t.IsInAnyStorage())
                {
                    return false;
                }
            }
            return !t.IsInValidBestStorage()
                && !t.IsForbidden(pawn)
                && HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, t, forced: false)
                && pawn.carryTracker.MaxStackSpaceEver(t.def) > 0;
        }

        private bool CheckIfAreaHasFilth(Pawn pawn, List<IntVec3> area)
        {
            bool areaHasCleaningJobToDo = false;
            foreach (IntVec3 cell in area)
            {
                foreach (Thing thing in cell.GetThingList(pawn.Map))
                {
                    if (!(thing is Filth filth))
                    {
                        continue;
                    }
                    if (!filth.Map.areaManager.Home[filth.Position])
                    {
                        continue;
                    }
                    if (!pawn.CanReserve(thing, 1, -1, null, false))
                    {
                        continue;
                    }
                    if (filth.TicksSinceThickened < 600)
                    {
                        continue;
                    }
                    areaHasCleaningJobToDo = true;
                    break;
                }
                if (areaHasCleaningJobToDo)
                {
                    break;
                }
            }
            return areaHasCleaningJobToDo;

        }
    }
}
