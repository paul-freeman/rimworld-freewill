using System;
using System.Collections.Generic;
using System.Reflection;
using Verse;
using System.Linq;
using RimWorld;
using HarmonyLib;

namespace Rimworld_FreeWillMod
{
    public class FreeWill_MapComponent : MapComponent
    {
        private static Func<string>[] mapComponentCheckActions;

        private Dictionary<Pawn, Dictionary<WorkTypeDef, Priority>> priorities;
        private Dictionary<Pawn, int> lastBored;
        private readonly FieldInfo activeAlertsField;
        private FreeWill_WorldComponent worldComp;

        public int NumPawns => this.map.mapPawns.FreeColonistsSpawnedCount;

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

        private int actionCounter = 0;

        public FreeWill_MapComponent(Map map) : base(map)
        {
            this.priorities = new Dictionary<Pawn, Dictionary<WorkTypeDef, Priority>> { };
            this.lastBored = new Dictionary<Pawn, int> { };
            this.activeAlertsField = AccessTools.Field(typeof(AlertsReadout), "AllAlerts");
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
                // var startTime = DateTime.Now;
                mapComponentCheckActions = mapComponentCheckActions ?? new Func<string>[]{
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

                var msg = getMapComponentTickAction()();
                this.actionCounter++;

                // var duration = DateTime.Now - startTime;
                // if (Prefs.DevMode && duration.TotalMilliseconds > 1.5f)
                // {
                //     Log.Message("FreeWill: MapComponentTick (" + msg + ") took " + duration.TotalMilliseconds + "ms");
                // }
            }
            catch (System.Exception e)
            {
                Log.ErrorOnce($"Free Will: could not perform tick action: mapTickCounter = {this.actionCounter}: {e}", 14147584);
            }
        }

        private Func<string> getMapComponentTickAction()
        {
            try
            {
                if (this.actionCounter < mapComponentCheckActions.Length)
                {
                    return mapComponentCheckActions[this.actionCounter];
                }
                int i = this.actionCounter - mapComponentCheckActions.Length;
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
            catch (System.Exception e)
            {
                throw new Exception("could not get map component tick action", e);
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

        public Priority GetPriority(Pawn pawn, WorkTypeDef workTypeDef)
        {
            if (!this.priorities.ContainsKey(pawn))
            {
                this.priorities[pawn] = new Dictionary<WorkTypeDef, Priority>();
            }
            if (!this.priorities[pawn].ContainsKey(workTypeDef))
            {
                this.priorities[pawn][workTypeDef] = new Priority(pawn, workTypeDef);
            }
            return this.priorities[pawn][workTypeDef];
        }

        private string setPriorityAction(Pawn pawn, WorkTypeDef workTypeDef)
        {
            var msg = pawn.Name.ToStringShort + " " + workTypeDef.defName;
            if (pawn == null)
            {
                Log.ErrorOnce($"Free Will: pawn is null: mapTickCounter = {this.actionCounter}", 584624);
                return msg;
            }
            if (!pawn.Awake() || pawn.Downed || pawn.Dead)
            {
                return msg;
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
                    return msg;
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
                return msg;
            }
            catch (System.Exception)
            {
                Log.ErrorOnce("Free Will: could not set priorities for pawn: " + pawn.Name + ": marking " + pawn.Name + " as not having free will", 752116445);
                var ok = worldComp.TryRemoveFreeWill(pawn);
                if (!ok)
                {
                    Log.ErrorOnce("Free Will: could not remove free will", 752116446);
                }
                return msg;
            }
        }

        private string checkSuppressionNeed()
        {
            try
            {
                this.SuppressionNeed = 0.0f;
                List<Pawn> slavesOfColonySpawned = this.map?.mapPawns?.SlavesOfColonySpawned;
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
                    this.SuppressionNeed = 0.2f;
                    if (needsSuppression.IsHigh)
                    {
                        this.SuppressionNeed = 0.4f;
                        return "checkSuppressionNeed";
                    }
                }
                return "checkSuppressionNeed";
            }
            catch (System.Exception e)
            {
                throw new Exception("could not check suppression need", e);
            }
        }

        private string checkPrisonerHealth()
        {
            var prisonersInColony = map?.mapPawns?.PrisonersOfColony;
            this.NumPrisonersNeedingTreatment =
                (prisonersInColony == null)
                    ? 0
                    : (from prisoners in map?.mapPawns?.PrisonersOfColony
                       where prisoners.health.HasHediffsNeedingTend()
                       select prisoners).Count();
            return "checkPrisonerHealth";
        }

        private string checkPetsHealth()
        {
            var pawnsInFaction = map?.mapPawns?.PawnsInFaction(Faction.OfPlayer);
            this.NumPetsNeedingTreatment =
                (pawnsInFaction == null)
                    ? 0
                    : (from p in pawnsInFaction
                       where p.RaceProps.Animal && p.health.HasHediffsNeedingTend()
                       select p).Count();
            return "checkPetsHealth";
        }

        private string checkColonyHealth()
        {
            this.PercentPawnsDowned = 0;
            this.PercentPawnsNeedingTreatment = 0;
            float colonistWeight = 1f / (map?.mapPawns?.FreeColonistsSpawnedCount ?? 1);
            var freeColonistsSpawned = map?.mapPawns?.FreeColonistsSpawned ?? new List<Pawn>();
            foreach (Pawn pawn in freeColonistsSpawned)
            {
                bool inBed = pawn.CurrentBed() != null;
                if (pawn.Downed && !inBed)
                {
                    this.PercentPawnsDowned += colonistWeight;
                }
                if (pawn.health.HasHediffsNeedingTend())
                {
                    this.PercentPawnsNeedingTreatment += colonistWeight;
                }
            }
            return "checkColonyHealth";
        }

        private string checkPercentPawnsMechHaulers()
        {
            try
            {
                List<Pawn> pawnsInFaction = this.map?.mapPawns?.PawnsInFaction(Faction.OfPlayer);
                if (pawnsInFaction == null)
                {
                    return "checkPercentPawnsMechHaulers";
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
                this.PercentPawnsMechHaulers = (total == 0.0f) ? 0.0f : numMechHaulers / total;
                return "checkPercentPawnsMechHaulers";
            }
            catch (System.Exception e)
            {
                throw new Exception("could not check percent of mech haulers", e);
            }
        }

        private string checkThingsDeteriorating()
        {
            this.ThingsDeteriorating = null;
            var thingsPotentiallyNeedingHauling = this.map?.listerHaulables?.ThingsPotentiallyNeedingHauling();
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
                this.ThingsDeteriorating = thing;
                return "checkThingsDeteriorating";
            }
            return "checkThingsDeteriorating";
        }

        private string checkBlight()
        {
            try
            {
                this.PlantsBlighted = GridsUtility.GetFirstBlight(this.map.Center, this.map) != null;
                return "checkBlight";
            }
            catch (System.Exception err)
            {
                Log.Message("could not check blight levels on map");
                Log.Message(err.ToString());
                Log.Message("this consideration will be disabled in the mod settings to avoid future errors");
                this.worldComp.settings.ConsiderPlantsBlighted = 0.0f;
                this.PlantsBlighted = false;
                return "checkBlight";
            }
        }

        private string checkMapFire()
        {
            List<Thing> fires = this.map?.listerThings?.ThingsOfDef(ThingDefOf.Fire);
            this.MapFires = fires?.Count ?? 0;
            this.HomeFire = false;
            if (fires == null)
            {
                return "checkMapFire";
            }
            foreach (Thing fire in fires)
            {
                this.MapFires++;
                if (this.map.areaManager.Home[fire.Position] && !fire.Position.Fogged(fire.Map))
                {
                    this.HomeFire = true;
                    return "checkMapFire";
                }
            }
            return "checkMapFire";
        }

        private string checkRefuelNeeded()
        {
            this.RefuelNeeded = false;
            this.RefuelNeededNow = false;
            List<Thing> refuelableThings = this.map?.listerThings?.ThingsInGroup(ThingRequestGroup.Refuelable);
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
                    this.RefuelNeeded = true;
                    this.RefuelNeededNow = true;
                    return "checkRefuelNeeded";
                }
                if (!refuelable.IsFull)
                {
                    this.RefuelNeeded = true;
                    continue;
                }
            }
            return "checkRefuelNeeded";
        }

        public string checkActiveAlerts()
        {
            try
            {
                UIRoot_Play ui = Find.UIRoot as UIRoot_Play;
                if (ui == null)
                {
                    return "checkActiveAlerts";
                }
                // unset all the alerts
                this.AlertLowFood = false;
                this.AlertAnimalRoaming = false;
                this.NeedWarmClothes = false;
                this.AlertColonistLeftUnburied = false;
                this.AlertMechDamaged = false;
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
                            this.AlertLowFood = true;
                            break;
                        case Alert_NeedWarmClothes a:
                            this.NeedWarmClothes = true;
                            break;
                        case Alert_AnimalRoaming a:
                            this.AlertAnimalRoaming = true;
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
                                        this.AlertColonistLeftUnburied = true;
                                        break;
                                    }
                                }
                                if (this.AlertColonistLeftUnburied)
                                {
                                    break;
                                }
                            }
                            break;
                        case Alert_MechDamaged a:
                            this.AlertMechDamaged = true;
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
    }
}
