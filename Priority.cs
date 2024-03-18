using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;


namespace FreeWill
{
    public class Priority : IComparable
    {
        const int DISABLED_CUTOFF = 100 / (Pawn_WorkSettings.LowestPriority + 1); // 20 if LowestPriority is 4
        const int DISABLED_CUTOFF_ACTIVE_WORK_AREA = 100 - DISABLED_CUTOFF; // 80 if LowestPriority is 4
        const float ONE_PRIORITY_WIDTH = (float)DISABLED_CUTOFF_ACTIVE_WORK_AREA / (float)Pawn_WorkSettings.LowestPriority; // ~20 if LowestPriority is 4

        private Pawn pawn;
        private FreeWill_WorldComponent worldComp;
        private FreeWill_MapComponent mapComp;
        public WorkTypeDef WorkTypeDef { get; }
        public float Value { get; private set; }
        public List<Func<string>> AdjustmentStrings { get; private set; }

        public bool Enabled { get; private set; }
        public bool Disabled { get; private set; }

        // work types
        const string FIREFIGHTER = "Firefighter";
        const string PATIENT = "Patient";
        const string DOCTOR = "Doctor";
        const string PATIENT_BED_REST = "PatientBedRest";
        const string CHILDCARE = "Childcare";
        const string BASIC_WORKER = "BasicWorker";
        const string WARDEN = "Warden";
        const string HANDLING = "Handling";
        const string COOKING = "Cooking";
        const string HUNTING = "Hunting";
        const string CONSTRUCTION = "Construction";
        const string GROWING = "Growing";
        const string MINING = "Mining";
        const string PLANT_CUTTING = "PlantCutting";
        const string SMITHING = "Smithing";
        const string TAILORING = "Tailoring";
        const string ART = "Art";
        const string CRAFTING = "Crafting";
        const string HAULING = "Hauling";
        const string CLEANING = "Cleaning";
        const string RESEARCHING = "Research";

        // supported modded work types
        const string HAULING_URGENT = "HaulingUrgent";

        private static int couldNotConvertToGamePriority = ("FreeWill" + "could not convert to game priority").GetHashCode();


        public Priority(Pawn pawn, WorkTypeDef workTypeDef)
        {
            this.pawn = pawn;
            WorkTypeDef = workTypeDef;
        }

        public void Compute()
        {
            try
            {
                AdjustmentStrings = new List<Func<string>> { };

                mapComp = pawn.Map.GetComponent<FreeWill_MapComponent>();
                worldComp = Find.World.GetComponent<FreeWill_WorldComponent>();

                // start priority at the global default and compute the priority
                // using the AI in this file
                set(0.2f, "FreeWillPriorityGlobalDefault".TranslateSimple).compute();
                return;
            }
            catch (System.Exception e)
            {
                if (Prefs.DevMode)
                {
                    throw new System.Exception("could not compute " + WorkTypeDef.defName + " priority for pawn: " + pawn.Name + ": " + e.Message);
                }
                Log.ErrorOnce("could not compute " + WorkTypeDef.defName + " priority for pawn: " + pawn.Name + ": " + e.Message, 15448413);
                
                    alwaysDo("FreeWillPriorityError".TranslateSimple)
                    .set(0.4f, "FreeWillPriorityError".TranslateSimple)
                    ;
            }
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj == null)
            {
                return 1;
            }
            Priority p = obj as Priority;
            if (p == null)
            {
                return 1;
            }
            return Value.CompareTo(p.Value);
        }

        private Priority compute()
        {
            Enabled = false;
            Disabled = false;
            if (mapComp.DisabledWorkTypes.Contains(WorkTypeDef))
            {
                return neverDo("FreeWillPriorityPermanentlyDisabled".TranslateSimple);
            }
            switch (WorkTypeDef.defName)
            {
                case FIREFIGHTER:
                    return 
                        set(0.0f, "FreeWillPriorityFirefightingDefault".TranslateSimple)
                        .alwaysDo("FreeWillPriorityFirefightingAlways".TranslateSimple)
                        .neverDoIf(pawn.Downed, "FreeWillPriorityPawnDowned".TranslateSimple)
                        .considerFire()
                        .considerBuildingImmunity()
                        .ConsiderCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .considerColonyPolicy()
                        ;

                case PATIENT:
                    return 
                        set(0.0f, "FreeWillPriorityPatientDefault".TranslateSimple)
                        .alwaysDo("FreeWillPriorityPatientAlways".TranslateSimple)
                        .considerHealth()
                        .considerBuildingImmunity()
                        .ConsiderCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .considerOperation()
                        .considerColonyPolicy()
                        ;

                case DOCTOR:
                    return 
                        considerRelevantSkills()
                        .ConsiderCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .ConsiderPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerInjuredPets()
                        .considerInjuredPrisoners()
                        .considerColonistLeftUnburied()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .ConsiderCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .considerColonyPolicy()
                        ;

                case PATIENT_BED_REST:
                    return 
                        set(0.0f, "FreeWillPriorityBedrestDefault".TranslateSimple)
                        .alwaysDo("FreeWillPriorityBedrestAlways".TranslateSimple)
                        .considerHealth()
                        .considerBuildingImmunity()
                        .considerLowFood(-0.2f)
                        .ConsiderCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .considerBored()
                        .ConsiderDownedColonists()
                        .considerOperation()
                        .considerColonyPolicy()
                        ;

                case CHILDCARE:
                    return 
                        set(0.5f, "FreeWillPriorityChildcareDefault".TranslateSimple)
                        .considerRelevantSkills(shouldAdd: true)
                        .ConsiderPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .ConsiderCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .considerColonyPolicy()
                        ;

                case BASIC_WORKER:
                    return 
                        set(0.5f, "FreeWillPriorityBasicWorkDefault".TranslateSimple)
                        .considerThoughts()
                        .considerHealth()
                        .considerLowFood(-0.3f)
                        .considerBored()
                        .neverDoIf(pawn.Downed, "FreeWillPriorityPawnDowned".TranslateSimple)
                        .considerBuildingImmunity()
                        .ConsiderCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .considerColonyPolicy()
                        ;

                case WARDEN:
                    return 
                        considerRelevantSkills()
                        .ConsiderCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .ConsiderPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerLowFood(-0.3f)
                        .considerSuppressionNeed()
                        .considerColonistLeftUnburied()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .ConsiderCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .considerColonyPolicy()
                        ;

                case HANDLING:
                    return 
                        considerRelevantSkills()
                        .ConsiderMovementSpeed()
                        .ConsiderCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .ConsiderPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerAnimalsRoaming()
                        .considerColonistLeftUnburied()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .ConsiderCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .considerColonyPolicy()
                        ;

                case COOKING:
                    return 
                        considerRelevantSkills()
                        .ConsiderCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .ConsiderPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerLowFood(0.2f)
                        .considerColonistLeftUnburied()
                        .considerFoodPoisoning()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .ConsiderCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .considerColonyPolicy()
                        ;

                case HUNTING:
                    return 
                        considerRelevantSkills()
                        .ConsiderMovementSpeed()
                        .ConsiderCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .ConsiderPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerLowFood(0.3f)
                        .considerWeaponRange()
                        .considerColonistLeftUnburied()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerBored()
                        .ConsiderHasHuntingWeapon()
                        .ConsiderBrawlersNotHunting()
                        .considerFire()
                        .considerBuildingImmunity()
                        .ConsiderCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .considerColonyPolicy()
                        ;

                case CONSTRUCTION:
                    return 
                        considerRelevantSkills()
                        .ConsiderCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .ConsiderPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerLowFood(-0.3f)
                        .considerColonistLeftUnburied()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .ConsiderCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .considerColonyPolicy()
                        ;

                case GROWING:
                    return 
                        considerRelevantSkills()
                        .ConsiderCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .ConsiderPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerLowFood(0.3f)
                        .considerColonistLeftUnburied()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .ConsiderCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .considerColonyPolicy()
                        ;

                case MINING:
                    return 
                        considerRelevantSkills()
                        .ConsiderCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .ConsiderPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerLowFood(-0.3f)
                        .considerColonistLeftUnburied()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .ConsiderCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .considerColonyPolicy()
                        ;

                case PLANT_CUTTING:
                    return 
                        considerRelevantSkills()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .ConsiderPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerGauranlenPruning()
                        .considerLowFood(0.3f)
                        .considerHealth()
                        .considerPlantsBlighted()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .ConsiderCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .considerColonyPolicy()
                        ;

                case SMITHING:
                    return 
                        considerRelevantSkills()
                        .ConsiderCarryingCapacity()
                        .ConsiderFinishedMechGestators()
                        .considerRepairingMech()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .considerBeautyExpectations()
                        .ConsiderPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerLowFood(-0.3f)
                        .considerColonistLeftUnburied()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .ConsiderCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .considerColonyPolicy()
                        ;

                case TAILORING:
                    return 
                        considerRelevantSkills()
                        .ConsiderCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .considerBeautyExpectations()
                        .ConsiderPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerLowFood(-0.3f)
                        .considerNeedingWarmClothes()
                        .considerColonistLeftUnburied()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .ConsiderCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .considerColonyPolicy()
                        ;

                case ART:
                    return 
                        considerRelevantSkills()
                        .ConsiderCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .considerBeautyExpectations()
                        .ConsiderPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerLowFood(-0.3f)
                        .considerColonistLeftUnburied()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .ConsiderCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .considerColonyPolicy()
                        ;

                case CRAFTING:
                    return 
                        considerRelevantSkills()
                        .ConsiderCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .considerBeautyExpectations()
                        .ConsiderPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerLowFood(-0.3f)
                        .considerColonistLeftUnburied()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .ConsiderCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .considerColonyPolicy()
                        ;

                case HAULING:
                    return 
                        set(0.3f, "FreeWillPriorityHaulingDefault".TranslateSimple)
                        .considerBeautyExpectations()
                        .ConsiderMovementSpeed()
                        .ConsiderCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .ConsiderPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerRefueling()
                        .considerLowFood(0.2f)
                        .considerColonistLeftUnburied()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerThingsDeteriorating()
                        .considerMechHaulers()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .ConsiderCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .considerColonyPolicy()
                        ;

                case CLEANING:
                    return 
                        set(0.5f, "FreeWillPriorityCleaningDefault".TranslateSimple)
                        .considerBeautyExpectations()
                        .considerIsAnyoneElseDoing()
                        .considerThoughts()
                        .considerOwnRoom()
                        .considerLowFood(-0.2f)
                        .considerFoodPoisoning()
                        .considerHealth()
                        .considerBored()
                        .neverDoIf(notInHomeArea(pawn), "FreeWillPriorityNotInHomeArea".TranslateSimple)
                        .considerBuildingImmunity()
                        .ConsiderCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .considerColonyPolicy()
                        ;

                case RESEARCHING:
                    return 
                        considerRelevantSkills()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .considerBeautyExpectations()
                        .ConsiderPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerLowFood(-0.4f)
                        .considerColonistLeftUnburied()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .considerColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .considerColonyPolicy()
                        ;

                case HAULING_URGENT:
                    return 
                        set(0.5f, "FreeWillPriorityUrgentHaulingDefault".TranslateSimple)
                        .considerBeautyExpectations()
                        .ConsiderMovementSpeed()
                        .ConsiderCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .ConsiderPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerRefueling()
                        .considerLowFood(0.3f)
                        .considerColonistLeftUnburied()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerThingsDeteriorating()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .ConsiderCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .considerColonyPolicy()
                        ;

                default:
                    return 
                        considerRelevantSkills()
                        .ConsiderMovementSpeed()
                        .ConsiderCarryingCapacity()
                        .considerBeautyExpectations()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .ConsiderPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerLowFood(-0.3f)
                        .considerColonistLeftUnburied()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .ConsiderCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .considerColonyPolicy()
                        ;
            }
        }

        public void ApplyPriorityToGame()
        {
            if (!Current.Game.playSettings.useWorkPriorities)
            {
                Current.Game.playSettings.useWorkPriorities = true;
            }

            int priority = ToGamePriority();
            if (pawn.workSettings.GetPriority(WorkTypeDef) != priority)
            {
                pawn.workSettings.SetPriority(WorkTypeDef, priority);
            }
        }

        public int ToGamePriority()
        {
            try
            {
                int valueInt = UnityEngine.Mathf.Clamp(UnityEngine.Mathf.RoundToInt(Value * 100), 0, 100);
                if (valueInt <= DISABLED_CUTOFF)
                {
                    if (Enabled)
                    {
                        return Pawn_WorkSettings.LowestPriority;
                    }
                    return 0;
                }
                if (Disabled)
                {
                    return 0;
                }
                int invertedValueRange = DISABLED_CUTOFF_ACTIVE_WORK_AREA - (valueInt - DISABLED_CUTOFF); // 0-80 if LowestPriority is 4
                int gamePriorityValue = UnityEngine.Mathf.FloorToInt((float)invertedValueRange / ONE_PRIORITY_WIDTH) + 1;
                if (gamePriorityValue > Pawn_WorkSettings.LowestPriority || gamePriorityValue < 1)
                {
                    Log.Error("calculated an invalid game priority value of " + gamePriorityValue.ToString());
                    gamePriorityValue = UnityEngine.Mathf.Clamp(gamePriorityValue, 1, Pawn_WorkSettings.LowestPriority);
                }

                return gamePriorityValue;
            }
            catch (Exception e)
            {
                Log.ErrorOnce("could not convert to game priority: " + e.ToString(), couldNotConvertToGamePriority);
                return 0;
            }
        }

        private Priority set(float x, Func<string> description)
        {
            Value = UnityEngine.Mathf.Clamp01(x);
            Func<string> adjustmentString = () =>
            {
                // Create string builder
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append(" - ");
                stringBuilder.Append(description().CapitalizeFirst());
                stringBuilder.Append(": ");
                stringBuilder.Append(Value.ToStringPercent());
                return stringBuilder.ToString();
            };
            if (Prefs.DevMode)
            {
                AdjustmentStrings.Add(() => "-- reset --");
                AdjustmentStrings.Add(adjustmentString);
            }
            else
            {
                AdjustmentStrings = new List<Func<string>> { adjustmentString };
            }
            return this;
        }

        private Priority add(float x, Func<string> description)
        {
            if (Disabled)
            {
                return this;
            }
            float newValue = UnityEngine.Mathf.Clamp01(Value + x);
            if (newValue > Value)
            {
                Func<string> adjustmentString = () =>
                {
                    // Create string builder
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(" - ");
                    stringBuilder.Append(description().CapitalizeFirst());
                    stringBuilder.Append(": +");
                    stringBuilder.Append((newValue - Value).ToStringPercent());
                    return stringBuilder.ToString();
                };
                AdjustmentStrings.Add(adjustmentString);
                Value = newValue;
            }
            else if (newValue < Value)
            {
                Func<string> adjustmentString = () =>
                {
                    // Create string builder
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(" - ");
                    stringBuilder.Append(description().CapitalizeFirst());
                    stringBuilder.Append(": -");
                    stringBuilder.Append((Value - newValue).ToStringPercent());
                    return stringBuilder.ToString();
                };
                AdjustmentStrings.Add(adjustmentString);
                Value = newValue;
            }
            else if (newValue == Value && Prefs.DevMode)
            {
                Func<string> adjustmentString = () =>
                {
                    // Create string builder
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(" - ");
                    stringBuilder.Append(description().CapitalizeFirst());
                    stringBuilder.Append(": +");
                    stringBuilder.Append((newValue - Value).ToStringPercent());
                    return stringBuilder.ToString();
                };
                AdjustmentStrings.Add(adjustmentString);
                Value = newValue;
            }
            return this;
        }

        private Priority multiply(float x, Func<string> description)
        {
            if (Disabled || Value == 0.0f)
            {
                return this;
            }
            float newClampedValue = UnityEngine.Mathf.Clamp01(Value * x);
            if (newClampedValue != Value)
            {
                Func<string> adjustmentString = () =>
                {
                    // Create string builder
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(" - ");
                    stringBuilder.Append(description().CapitalizeFirst());
                    stringBuilder.Append(": x");
                    stringBuilder.Append((newClampedValue / Value).ToStringPercent());
                    return stringBuilder.ToString();
                };
                AdjustmentStrings.Add(adjustmentString);
                Value = newClampedValue;
            }
            else if (newClampedValue == Value && Prefs.DevMode)
            {
                Func<string> adjustmentString = () =>
                {
                    // Create string builder
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(" - ");
                    stringBuilder.Append(description().CapitalizeFirst());
                    stringBuilder.Append(": x");
                    stringBuilder.Append((newClampedValue / Value).ToStringPercent());
                    return stringBuilder.ToString();
                };
                AdjustmentStrings.Add(adjustmentString);
                Value = newClampedValue;
            }
            return this;
        }

        private bool isDisabled()
        {
            return Disabled;
        }

        private Priority alwaysDoIf(bool cond, Func<string> description)
        {
            if (!cond || Enabled)
            {
                return this;
            }
            if (Prefs.DevMode || Disabled || ToGamePriority() == 0)
            {
                Func<string> adjustmentString = () =>
                {
                    // Create string builder
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(" - ");
                    stringBuilder.Append(description().CapitalizeFirst());
                    stringBuilder.Append(": ");
                    stringBuilder.Append("FreeWillPriorityEnabled".TranslateSimple().CapitalizeFirst());
                    return stringBuilder.ToString();
                };
                AdjustmentStrings.Add(adjustmentString);
            }
            Enabled = true;
            Disabled = false;
            return this;
        }

        private Priority alwaysDo(Func<string> description)
        {
            return alwaysDoIf(true, description);
        }

        private Priority neverDoIf(bool cond, Func<string> description)
        {
            if (!cond || Disabled)
            {
                return this;
            }
            if (Prefs.DevMode || Enabled || ToGamePriority() >= 0)
            {
                Func<string> adjustmentString = () =>
                {
                    // Create string builder
                    StringBuilder stringBuilder = new StringBuilder();
                    stringBuilder.Append(" - ");
                    stringBuilder.Append(description().CapitalizeFirst());
                    stringBuilder.Append(": ");
                    stringBuilder.Append("FreeWillPriorityDisabled".TranslateSimple().CapitalizeFirst());
                    return stringBuilder.ToString();
                };
                AdjustmentStrings.Add(adjustmentString);
            }
            Disabled = true;
            Enabled = false;
            return this;
        }

        private Priority neverDo(Func<string> description)
        {
            return neverDoIf(true, description);
        }

        private Priority considerInspiration()
        {
            if (!pawn.mindState.inspirationHandler.Inspired)
            {
                return this;
            }
            Inspiration inspiration = pawn.mindState.inspirationHandler.CurState;
            if (WorkTypeDef.defName == HUNTING && inspiration.def.defName == "Frenzy_Shoot")
            {
                return add(0.4f, "FreeWillPriorityInspired".TranslateSimple);
            }
            foreach (WorkTypeDef workTypeDefB in inspiration?.def?.requiredNonDisabledWorkTypes ?? new List<WorkTypeDef>())
            {
                if (WorkTypeDef.defName == workTypeDefB.defName)
                {
                    return add(0.4f, "FreeWillPriorityInspired".TranslateSimple);
                }
            }
            foreach (WorkTypeDef workTypeDefB in inspiration?.def?.requiredAnyNonDisabledWorkType ?? new List<WorkTypeDef>())
            {
                if (WorkTypeDef.defName == workTypeDefB.defName)
                {
                    return add(0.4f, "FreeWillPriorityInspired".TranslateSimple);
                }
            }
            return this;
        }

        private Priority considerThoughts()
        {
            foreach (Thought thought in mapComp.AllThoughts)
            {
                if (thought.def.defName == "NeedFood")
                {
                    if (WorkTypeDef.defName == COOKING)
                    {
                        return add(-0.01f * thought.CurStage.baseMoodEffect, "FreeWillPriorityHungerLevel".TranslateSimple);
                    }
                    if (WorkTypeDef.defName == HUNTING || WorkTypeDef.defName == PLANT_CUTTING)
                    {
                        return add(-0.005f * thought.CurStage.baseMoodEffect, "FreeWillPriorityHungerLevel".TranslateSimple);
                    }
                    return add(0.005f * thought.CurStage.baseMoodEffect, "FreeWillPriorityHungerLevel".TranslateSimple);
                }
            }
            return this;
        }

        private Priority considerNeedingWarmClothes()
        {
            if (mapComp.NeedWarmClothes)
            {
                return add(0.2f, "FreeWillPriorityNeedWarmClothes".TranslateSimple);
            }
            return this;
        }

        private Priority considerAnimalsRoaming()
        {
            if (mapComp.AlertAnimalRoaming)
            {
                return add(0.4f, "FreeWillPriorityAnimalsRoaming".TranslateSimple);
            }
            return this;
        }

        private Priority considerSuppressionNeed()
        {
            if (mapComp.SuppressionNeed != 0.0f)
            {
                return add(mapComp.SuppressionNeed, "FreeWillPrioritySuppressionNeed".TranslateSimple);
            }
            return this;
        }

        private Priority considerColonistLeftUnburied()
        {
            if (mapComp.AlertColonistLeftUnburied && (WorkTypeDef.defName == HAULING || WorkTypeDef.defName == HAULING_URGENT))
            {
                return add(0.4f, "FreeWillPriorityColonistLeftUnburied".TranslateSimple);
            }
            return this;
        }

        private Priority considerBored()
        {
            const int boredomMemory = 2500; // 1 hour in game
            if (pawn.mindState.IsIdle)
            {
                (mapComp as FreeWill_MapComponent)?.UpdateLastBored(pawn);
                return alwaysDoIf(pawn.mindState.IsIdle, "FreeWillPriorityBored".TranslateSimple);
            }
            var lastBored = (mapComp as FreeWill_MapComponent)?.GetLastBored(pawn);
            var wasBored = lastBored != 0 && Find.TickManager.TicksGame - lastBored < boredomMemory;
            return alwaysDoIf(wasBored, "FreeWillPriorityWasBored".TranslateSimple);
        }

        private Priority ConsiderHasHuntingWeapon()
        {
            try
            {
                if (!worldComp.settings.ConsiderHasHuntingWeapon)
                {
                    return this;
                }
                return neverDoIf(!WorkGiver_HunterHunt.HasHuntingWeapon(pawn), "FreeWillPriorityNoHuntingWeapon".TranslateSimple);
            }
            catch (Exception err)
            {
                Log.Error(pawn.Name + " could not consider has hunting weapon to adjust " + WorkTypeDef.defName);
                Log.Message(err.ToString());
                Log.Message("this consideration will be disabled in the mod settings to avoid future errors");
                worldComp.settings.ConsiderHasHuntingWeapon = false;
                return this;
            }
        }

        private Priority ConsiderBrawlersNotHunting()
        {
            if (!worldComp.settings.ConsiderBrawlersNotHunting)
            {
                return this;
            }
            try
            {
                if (WorkTypeDef.defName != HUNTING)
                {
                    return this;
                }
                return neverDoIf(pawn.story.traits.HasTrait(DefDatabase<TraitDef>.GetNamed("Brawler")), "FreeWillPriorityBrawler".TranslateSimple);
            }
            catch (System.Exception err)
            {
                Log.Error(pawn.Name + " could not consider brawlers can hunt to adjust " + WorkTypeDef.defName);
                Log.Message(err.ToString());
                Log.Message("this consideration will be disabled in the mod settings to avoid future errors");
                worldComp.settings.ConsiderBrawlersNotHunting = false;
                return this;
            }
        }

        private Priority ConsiderCompletingTask()
        {
            if (pawn.CurJob != null && pawn.CurJob.workGiverDef != null && pawn.CurJob.workGiverDef.workType == WorkTypeDef)
            {
                return 

                    // pawns should not stop doing the work they are currently
                    // doing
                    alwaysDo("FreeWillPriorityCurrentlyDoing".TranslateSimple)

                    // pawns prefer the work they are current doing
                    .multiply(1.8f, "FreeWillPriorityCurrentlyDoing".TranslateSimple)

                    ;
            }
            return this;
        }

        private Priority ConsiderMovementSpeed()
        {
            try
            {
                if (worldComp.settings.ConsiderMovementSpeed == 0.0f)
                {
                    return this;
                }
                return multiply(
                    (
                        worldComp.settings.ConsiderMovementSpeed
                            * 0.25f
                            * pawn.GetStatValue(StatDefOf.MoveSpeed, true)
                    ),
                    "FreeWillPriorityMovementSpeed".TranslateSimple
                );
            }
            catch (System.Exception err)
            {
                Log.Message(pawn.Name + " could not consider movement speed to adjust " + WorkTypeDef.defName);
                Log.Message(err.ToString());
                Log.Message("this consideration will be disabled in the mod settings to avoid future errors");
                worldComp.settings.ConsiderMovementSpeed = 0.0f;
                return this;
            }
        }

        private Priority ConsiderCarryingCapacity()
        {
            var _baseCarryingCapacity = 75.0f;
            if (WorkTypeDef.defName != HAULING && WorkTypeDef.defName != HAULING_URGENT)
            {
                return this;
            }
            float _carryingCapacity = pawn.GetStatValue(StatDefOf.CarryingCapacity, true);
            if (_carryingCapacity >= _baseCarryingCapacity)
            {
                return this;
            }
            return multiply(_carryingCapacity / _baseCarryingCapacity, "FreeWillPriorityCarryingCapacity".TranslateSimple);
        }

        private Priority ConsiderPassion()
        {
            try
            {
                if (worldComp.settings.ConsiderPassions == 0f)
                {
                    return this;
                }
                var relevantSkills = WorkTypeDef.relevantSkills;
                float x;
                for (int i = 0; i < relevantSkills.Count; i++)
                {
                    int index = i;
                    switch (pawn.skills.GetSkill(relevantSkills[i]).passion)
                    {
                        case Passion.None:
                            continue;
                        case Passion.Major:
                            x = worldComp.settings.ConsiderPassions * pawn.needs.mood.CurLevel * 0.5f / relevantSkills.Count;
                            
                                alwaysDo(() => "FreeWillPriorityMajorPassionFor".Translate(relevantSkills[index].skillLabel))
                                .add(x, () => "FreeWillPriorityMajorPassionFor".Translate(relevantSkills[index].skillLabel));
                            continue;
                        case Passion.Minor:
                            x = worldComp.settings.ConsiderPassions * pawn.needs.mood.CurLevel * 0.25f / relevantSkills.Count;
                            
                                alwaysDo(() => "FreeWillPriorityMinorPassionFor".Translate(relevantSkills[index].skillLabel))
                                .add(x, () => "FreeWillPriorityMinorPassionFor".Translate(relevantSkills[index].skillLabel));
                            continue;
                        default:
                            // considerInterest(pawn, relevantSkills[i], relevantSkills.Count, WorkTypeDef);
                            continue;
                    }
                }
            }
            catch (Exception err)
            {
                Log.ErrorOnce("could not consider passions: " + "this consideration will be disabled in the mod settings to avoid future errors: " + err.ToString(), 228486541);
                worldComp.settings.ConsiderPassions = 0.0f;
            }
            return this;
        }

        private Priority ConsiderFinishedMechGestators()
        {
            var mechGestators = pawn?.Map?.listerThings?.ThingsInGroup(ThingRequestGroup.MechGestator);
            if (mechGestators == null)
            {
                return this;
            }
            foreach (Thing thing in mechGestators)
            {
                if (!(thing is Building_MechGestator mechGestator))
                {
                    continue;
                }
                if (!(mechGestator.ActiveBill is Bill_ProductionMech productionMech))
                {
                    continue;
                }
                if (productionMech.State != FormingState.Formed)
                {
                    continue;
                }
                if (productionMech.BoundPawn == pawn)
                {
                    return add(0.4f, "FreeWillPriorityMechGestator".TranslateSimple);
                }
            }
            return this;
        }

        private Priority ConsiderDownedColonists()
        {
            if (pawn.Downed)
            {
                if (WorkTypeDef.defName == PATIENT || WorkTypeDef.defName == PATIENT_BED_REST)
                {
                    return alwaysDo("FreeWillPriorityPawnDowned".TranslateSimple).set(1.0f, "FreeWillPriorityPawnDowned".TranslateSimple);
                }
                return neverDo("FreeWillPriorityPawnDowned".TranslateSimple);
            }
            if (mapComp.PercentPawnsDowned <= 0.0f)
            {
                return this;
            }
            if (WorkTypeDef.defName == DOCTOR)
            {
                return add(mapComp.PercentPawnsDowned, "FreeWillPriorityOtherPawnsDowned".TranslateSimple);
            }
            if (WorkTypeDef.defName == SMITHING ||
                WorkTypeDef.defName == TAILORING ||
                WorkTypeDef.defName == ART ||
                WorkTypeDef.defName == CRAFTING ||
                WorkTypeDef.defName == RESEARCHING
                )
            {
                return neverDo("FreeWillPriorityOtherPawnsDowned".TranslateSimple);
            }
            return this;
        }

        private Priority considerColonyPolicy()
        {
            try
            {
                add(worldComp.settings.globalWorkAdjustments[WorkTypeDef.defName], "FreeWillPriorityColonyPolicy".TranslateSimple);
            }
            catch (System.Exception)
            {
                worldComp.settings.globalWorkAdjustments[WorkTypeDef.defName] = 0.0f;
            }
            return this;
        }

        private Priority considerRefueling()
        {
            if (mapComp.RefuelNeededNow)
            {
                return add(0.35f, "FreeWillPriorityRefueling".TranslateSimple);
            }
            if (mapComp.RefuelNeeded)
            {
                return add(0.20f, "FreeWillPriorityRefueling".TranslateSimple);
            }
            return this;
        }

        private Priority considerFire()
        {
            if (mapComp.HomeFire)
            {
                if (WorkTypeDef.defName != FIREFIGHTER)
                {
                    return add(-0.2f, "FreeWillPriorityFireInHomeArea".TranslateSimple);
                }
                return set(1.0f, "FreeWillPriorityFireInHomeArea".TranslateSimple);
            }
            if (mapComp.MapFires > 0 && WorkTypeDef.defName == FIREFIGHTER)
            {
                return add(Mathf.Clamp01(mapComp.MapFires * 0.01f), "FreeWillPriorityFireOnMap".TranslateSimple);
            }
            return this;
        }

        private Priority considerOperation()
        {
            if (HealthAIUtility.ShouldHaveSurgeryDoneNow(pawn))
            {
                return set(1.0f, "FreeWillPriorityOperation".TranslateSimple);
            }
            return this;
        }

        private Priority considerBuildingImmunity()
        {
            try
            {
                if (!pawn.health.hediffSet.HasImmunizableNotImmuneHediff())
                {
                    return this;
                }
                if (WorkTypeDef.defName == PATIENT_BED_REST)
                {
                    return add(0.4f, "FreeWillPriorityBuildingImmunity".TranslateSimple);
                }
                if (WorkTypeDef.defName == PATIENT)
                {
                    return this;
                }
                return add(-0.2f, "FreeWillPriorityBuildingImmunity".TranslateSimple);
            }
            catch
            {
                Log.Message("could not consider pawn building immunity");
                return this;
            }
        }

        private Priority considerColonistsNeedingTreatment()
        {
            if (mapComp.PercentPawnsNeedingTreatment <= 0.0f)
            {
                return this;
            }

            if (pawn.health.HasHediffsNeedingTend())
            {
                // this pawn needs treatment
                return considerThisPawnNeedsTreatment();
            }
            else
            {
                // another pawn needs treatment
                return considerAnotherPawnNeedsTreatment();
            }
        }

        private Priority considerThisPawnNeedsTreatment()
        {

            if (WorkTypeDef.defName == PATIENT || WorkTypeDef.defName == PATIENT_BED_REST)
            {
                // patient and bed rest are activated and set to 100%
                return 
                    alwaysDo("FreeWillPriorityNeedTreatment".TranslateSimple)
                    .set(1.0f, "FreeWillPriorityNeedTreatment".TranslateSimple)
                    ;
            }
            if (WorkTypeDef.defName == DOCTOR)
            {
                if (pawn.playerSettings.selfTend)
                {
                    // this pawn can self tend, so activate doctor skill and set
                    // to 100%
                    return 
                        alwaysDo("FreeWillPriorityNeedTreatmentSelfTend".TranslateSimple)
                        .set(1.0f, "FreeWillPriorityNeedTreatmentSelfTend".TranslateSimple)
                        ;
                }
                // doctoring stays the same
                return this;
            }
            // don't do other work types
            return neverDo("FreeWillPriorityNeedTreatment".TranslateSimple);
        }

        private Priority considerAnotherPawnNeedsTreatment()
        {
            if (WorkTypeDef.defName == FIREFIGHTER ||
                WorkTypeDef.defName == PATIENT_BED_REST
                )
            {
                // don't adjust these work types
                return this;
            }

            // increase doctor priority for all pawns
            if (WorkTypeDef.defName == DOCTOR)
            {
                // increase the doctor priority by the percentage of pawns
                // needing treatment
                //
                // so if 25% of the colony is injured, doctoring for all
                // non-injured pawns will increase by 25%
                return add(mapComp.PercentPawnsNeedingTreatment, "FreeWillPriorityOthersNeedTreatment".TranslateSimple);
            }

            if (WorkTypeDef.defName == RESEARCHING)
            {
                // don't research when someone is dying please... it's rude
                return neverDo("FreeWillPriorityOthersNeedTreatment".TranslateSimple);
            }

            if (WorkTypeDef.defName == SMITHING ||
                WorkTypeDef.defName == TAILORING ||
                WorkTypeDef.defName == ART ||
                WorkTypeDef.defName == CRAFTING
                )
            {
                // crafting work types are low priority when someone is injured
                if (Value > 0.3f)
                {
                    return add(-(Value - 0.3f), "FreeWillPriorityOthersNeedTreatment".TranslateSimple);
                }
                return this;
            }

            // any other work type is capped at 0.6
            if (Value > 0.6f)
            {
                return add(-(Value - 0.6f), "FreeWillPriorityOthersNeedTreatment".TranslateSimple);
            }
            return this;
        }

        private Priority considerHealth()
        {
            if (WorkTypeDef.defName == PATIENT || WorkTypeDef.defName == PATIENT_BED_REST)
            {
                return add(1 - Mathf.Pow(pawn.health.summaryHealth.SummaryHealthPercent, 7.0f), "FreeWillPriorityHealth".TranslateSimple);
            }
            return multiply(pawn.health.summaryHealth.SummaryHealthPercent, "FreeWillPriorityHealth".TranslateSimple);
        }

        private Priority considerFoodPoisoning()
        {
            if (worldComp.settings.ConsiderFoodPoisoning == 0.0f)
            {
                return this;
            }
            try
            {
                if (WorkTypeDef.defName != CLEANING && WorkTypeDef.defName != COOKING)
                {
                    return this;
                }

                var adjustment = 0.0f;
                var room = pawn.GetRoom();
                if (room.TouchesMapEdge)
                {
                    return this;
                }
                if (room.IsHuge)
                {
                    return this;
                }
                foreach (Building building in room.ContainedAndAdjacentThings.OfType<Building>())
                {
                    if (building == null)
                    {
                        continue;
                    }
                    if (building.Faction != Faction.OfPlayer)
                    {
                        continue;
                    }
                    if (building.def.building.isMealSource)
                    {
                        adjustment =
                            (
                                worldComp.settings.ConsiderFoodPoisoning
                                * 20.0f
                                * pawn.GetRoom().GetStat(RoomStatDefOf.FoodPoisonChance)
                            );
                        if (WorkTypeDef.defName == CLEANING)
                        {
                            return add(adjustment, "FreeWillPriorityFilthyCookingArea".TranslateSimple);
                        }
                        if (WorkTypeDef.defName == COOKING)
                        {
                            return add(-adjustment, "FreeWillPriorityFilthyCookingArea".TranslateSimple);
                        }
                    }
                }
                return this;
            }
            catch (System.Exception err)
            {
                Log.Error(pawn.Name + " could not consider food poisoning to adjust " + WorkTypeDef.defName);
                Log.Message(err.ToString());
                Log.Message("this consideration will be disabled in the mod settings to avoid future errors");
                worldComp.settings.ConsiderFoodPoisoning = 0.0f;
                return this;
            }
        }

        private Priority considerOwnRoom()
        {
            if (worldComp.settings.ConsiderOwnRoom == 0.0f)
            {
                return this;
            }
            try
            {
                if (WorkTypeDef.defName != CLEANING)
                {
                    return this;
                }
                var room = pawn.GetRoom();
                var isPawnsRoom = false;
                foreach (Pawn owner in room.Owners)
                {
                    if (pawn == owner)
                    {
                        isPawnsRoom = true;
                        break;
                    }
                }
                if (!isPawnsRoom)
                {
                    return this;
                }
                return multiply(worldComp.settings.ConsiderOwnRoom * 2.0f, "FreeWillPriorityOwnRoom".TranslateSimple);
            }
            catch (System.Exception err)
            {
                Log.Message(pawn.Name + " could not consider being in own room to adjust " + WorkTypeDef.defName);
                Log.Message(err.ToString());
                Log.Message("this consideration will be disabled in the mod settings to avoid future errors");
                worldComp.settings.ConsiderOwnRoom = 0.0f;
                return this;
            }
        }

        private Priority considerRepairingMech()
        {
            if (!mapComp.AlertMechDamaged)
            {
                return this;
            }
            if (!MechanitorUtility.IsMechanitor(pawn))
            {
                return this;
            }
            return add(0.6f, "FreeWillPriorityMechanoidDamaged".TranslateSimple);
        }

        private Priority considerIsAnyoneElseDoing()
        {
            try
            {
                foreach (Pawn other in mapComp.PawnsInFaction)
                {
                    if (other == null || other == pawn)
                    {
                        continue;
                    }
                    if (!other.IsColonistPlayerControlled && !other.IsColonyMechPlayerControlled)
                    {
                        continue;
                    }
                    if (!other.Awake() || other.Downed || other.Dead || other.IsCharging())
                    {
                        continue;
                    }
                    if ((other.workSettings?.GetPriority(WorkTypeDef) ?? 0) != 0)
                    {
                        return this; // someone else is doing
                    }
                    if (other.RaceProps?.mechEnabledWorkTypes?.Contains(WorkTypeDef) ?? false)
                    {
                        return this; // a mech is doing
                    }
                }
                return alwaysDo("FreeWillPriorityNoOneElseDoing".TranslateSimple);
            }
            catch (System.Exception err)
            {
                Log.ErrorOnce(pawn.Name + " could not consider if anyone else is doing " + WorkTypeDef.defName + ": " + err.ToString(), 59947211);
                return this;
            }

        }

        private Priority considerBestAtDoing()
        {
            try
            {
                if (worldComp.settings.ConsiderBestAtDoing == 0.0f)
                {
                    return this;
                }
                var allPawns = mapComp.PawnsInFaction;
                if (allPawns.Count() <= 1)
                {
                    return this;
                }
                bool isBest = true;
                float pawnSkill = pawn.skills.AverageOfRelevantSkillsFor(WorkTypeDef);
                float impactPerPawn = worldComp.settings.ConsiderBestAtDoing / (float)allPawns.Count();
                foreach (Pawn other in allPawns)
                {
                    if (other == null || other == pawn)
                    {
                        continue;
                    }
                    if (!other.IsColonistPlayerControlled && !other.IsColonyMechPlayerControlled)
                    {
                        continue;
                    }
                    if (!other.Awake() || other.Downed || other.Dead || other.IsCharging())
                    {
                        continue;
                    }
                    if (other.IsColonyMechPlayerControlled && !other.RaceProps.mechEnabledWorkTypes.Contains(WorkTypeDef))
                    {
                        continue;
                    }
                    float otherSkill =
                        (other.IsColonistPlayerControlled)
                            ? other.skills.AverageOfRelevantSkillsFor(WorkTypeDef)
                            : other.RaceProps.mechFixedSkillLevel;
                    float skillDiff = otherSkill - pawnSkill;
                    if (skillDiff > 0.0f)
                    {
                        // not the best
                        isBest = false;
                        var isDoing = other.CurJob != null && other.CurJob.workGiverDef != null && other.CurJob.workGiverDef.workType == WorkTypeDef;
                        var isMuchBetter = skillDiff >= 5.0f;
                        var isMuchMuchBetter = skillDiff >= 10.0f;
                        var isMuchMuchMuchBetter = skillDiff >= 15.0f;
                        if (isDoing)
                        {
                            if (isMuchMuchMuchBetter)
                            {
                                add(1.5f * -impactPerPawn, () => "FreeWillPrioritySomeoneMuchMuchMuchBetterIsDoing".Translate(other.LabelShortCap));
                            }
                            else if (isMuchMuchBetter)
                            {
                                add(1.5f * -impactPerPawn * 0.8f, () => "FreeWillPrioritySomeoneMuchMuchBetterIsDoing".Translate(other.LabelShortCap));
                            }
                            else if (isMuchBetter)
                            {
                                add(1.5f * -impactPerPawn * 0.6f, () => "FreeWillPrioritySomeoneMuchBetterIsDoing".Translate(other.LabelShortCap));
                            }
                            else
                            {
                                add(1.5f * -impactPerPawn * 0.4f, () => "FreeWillPrioritySomeoneBetterIsDoing".Translate(other.LabelShortCap));
                            }
                        }
                        else
                        {
                            if (isMuchMuchMuchBetter)
                            {
                                add(-impactPerPawn, () => "FreeWillPrioritySomeoneMuchMuchMuchBetterAtDoing".Translate(other.LabelShortCap));
                            }
                            else if (isMuchMuchBetter)
                            {
                                add(-impactPerPawn * 0.8f, () => "FreeWillPrioritySomeoneMuchMuchBetterAtDoing".Translate(other.LabelShortCap));
                            }
                            else if (isMuchBetter)
                            {
                                add(-impactPerPawn * 0.6f, () => "FreeWillPrioritySomeoneMuchBetterAtDoing".Translate(other.LabelShortCap));
                            }
                            else
                            {
                                add(-impactPerPawn * 0.4f, () => "FreeWillPrioritySomeoneBetterAtDoing".Translate(other.LabelShortCap));
                            }
                        }
                    }
                }
                if (isBest)
                {
                    return multiply(1.5f * worldComp.settings.ConsiderBestAtDoing, "FreeWillPriorityBestAtDoing".TranslateSimple);
                }
                return this;
            }
            catch (System.Exception err)
            {
                Log.ErrorOnce("could not consider being the best at something: " + "this consideration will be disabled in the mod settings to avoid future errors: " + err.ToString(), 76898214);
                worldComp.settings.ConsiderBestAtDoing = 0.0f;
            }
            return this;
        }

        private Priority considerInjuredPets()
        {
            if (WorkTypeDef.defName != DOCTOR)
            {
                return this;
            }

            int n = mapComp.NumPawns;
            if (n == 0)
            {
                return this;
            }
            float numPetsNeedingTreatment = mapComp.NumPetsNeedingTreatment;
            return add(UnityEngine.Mathf.Clamp01(numPetsNeedingTreatment / ((float)n)) * 0.5f, "FreeWillPriorityPetsInjured".TranslateSimple);
        }

        private Priority considerMechHaulers()
        {
            float percentPawnsMechHaulers = mapComp.PercentPawnsMechHaulers;
            return add(-percentPawnsMechHaulers, "FreeWillPriorityMechHaulers".TranslateSimple);
        }

        private Priority considerInjuredPrisoners()
        {
            if (WorkTypeDef.defName != DOCTOR)
            {
                return this;
            }

            int n = mapComp.NumPawns;
            if (n == 0)
            {
                return this;
            }
            float numPrisonersNeedingTreatment = mapComp.NumPrisonersNeedingTreatment;
            return add(UnityEngine.Mathf.Clamp01(numPrisonersNeedingTreatment / ((float)n)) * 0.5f, "FreeWillPriorityPrisonersInjured".TranslateSimple);
        }

        private Priority considerLowFood(float adjustment)
        {
            try
            {
                if (worldComp.settings.ConsiderLowFood == 0.0f || !mapComp.AlertLowFood)
                {
                    return this;
                }

                // don't adjust hauling if nothing deteriorating (i.e. food in the field)
                if ((WorkTypeDef.defName == HAULING || WorkTypeDef.defName == HAULING_URGENT)
                    && mapComp.ThingsDeteriorating == null)
                {
                    return this;
                }

                return add(adjustment * worldComp.settings.ConsiderLowFood, "FreeWillPriorityLowFood".TranslateSimple);
            }
            catch (System.Exception err)
            {
                Log.ErrorOnce("could not consider low food: " + "this consideration will be disabled in the mod settings to avoid future errors: " + err.ToString(), 10979710);
                worldComp.settings.ConsiderLowFood = 0.0f;
            }
            return this;
        }

        private Priority considerWeaponRange()
        {
            if ((worldComp?.settings?.ConsiderWeaponRange ?? 0.0f) == 0.0f)
            {
                return this;
            }
            if (!WorkGiver_HunterHunt.HasHuntingWeapon(pawn))
            {
                return this;
            }
            const float boltActionRifleRange = 37.0f;
            float range = pawn.equipment.PrimaryEq.PrimaryVerb.verbProps.range;
            float relativeRange = range / boltActionRifleRange;
            return multiply(relativeRange * worldComp.settings.ConsiderWeaponRange, "FreeWillPriorityWeaponRange".TranslateSimple);
        }

        private Priority considerAteRawFood()
        {
            if (WorkTypeDef.defName != COOKING)
            {
                return this;
            }

            foreach (Thought thought in mapComp.AllThoughts)
            {
                if (thought.def.defName == "AteRawFood" && 0.6f > Value)
                {
                    return set(0.6f, "FreeWillPriorityAteRawFood".TranslateSimple);
                }
            }
            return this;
        }

        private Priority considerThingsDeteriorating()
        {
            if (WorkTypeDef.defName == HAULING || WorkTypeDef.defName == HAULING_URGENT)
            {
                if (mapComp.ThingsDeteriorating != null)
                {
                    if (Prefs.DevMode)
                    {
                        Func<string> adjustmentString = () =>
                        {
                            // Create string builder
                            StringBuilder stringBuilder = new StringBuilder();
                            stringBuilder.Append("FreeWillPriorityThingsDeteriorating".TranslateSimple());
                            stringBuilder.Append(": ");
                            stringBuilder.Append(mapComp.ThingsDeteriorating.def.defName);
                            return stringBuilder.ToString();
                        };
                        return multiply(2.0f, adjustmentString);
                    }
                    return multiply(2.0f, "FreeWillPriorityThingsDeteriorating".TranslateSimple);
                }
            }
            return this;
        }

        private Priority considerPlantsBlighted()
        {
            try
            {
                if (worldComp.settings.ConsiderPlantsBlighted == 0.0f)
                {
                    // no point checking if it is disabled
                    return this;
                }
                if (mapComp.PlantsBlighted)
                {
                    return add(0.4f * worldComp.settings.ConsiderPlantsBlighted, "FreeWillPriorityBlight".TranslateSimple);
                }
            }
            catch (System.Exception err)
            {
                Log.Message("could not consider blight levels");
                Log.Message(err.ToString());
                Log.Message("this consideration will be disabled in the mod settings to avoid future errors");
                worldComp.settings.ConsiderPlantsBlighted = 0.0f;
            }
            return this;
        }

        private Priority considerGauranlenPruning()
        {
            try
            {
                if (WorkTypeDef.defName != PLANT_CUTTING)
                {
                    return this;
                }
                foreach (Thing connectedThing in pawn.connections.ConnectedThings)
                {
                    CompTreeConnection compTreeConnection = connectedThing.TryGetComp<CompTreeConnection>();
                    if (compTreeConnection != null && compTreeConnection.Mode != null)
                    {
                        if (!compTreeConnection.ShouldBePrunedNow(false))
                        {
                            return this;
                        }
                        {
                            return multiply(2.0f * worldComp.settings.ConsiderGauranlenPruning, "FreeWillPriorityPruneGauranlenTree".TranslateSimple);
                        }
                    }
                }
            }
            catch (System.Exception err)
            {
                Log.ErrorOnce("could not consider pruning gauranlen tree: " + "this consideration will be disabled in the mod settings to avoid future errors: " + err.ToString(), 45846314);
                worldComp.settings.ConsiderGauranlenPruning = 0.0f;
            }
            return this;
        }

        private Priority considerBeautyExpectations()
        {
            try
            {
                if (worldComp.settings.ConsiderBeauty == 0.0f)
                {
                    return this;
                }
                float expectations = worldComp.settings.ConsiderBeauty * expectationGrid[ExpectationsUtility.CurrentExpectationFor(pawn).defName][pawn.needs.beauty.CurCategory];
                switch (WorkTypeDef.defName)
                {
                    case HAULING:
                    case HAULING_URGENT:
                        // check for haulable
                        if (!mapComp.AreaHasHaulables)
                        {
                            // no hauling job
                            return this;
                        }
                        if (expectations < 0.2f)
                        {
                            return add(expectations, "FreeWillPriorityExpectionsExceeded".TranslateSimple);
                        }
                        if (expectations < 0.4f)
                        {
                            return add(expectations, "FreeWillPriorityExpectionsMet".TranslateSimple);
                        }
                        if (expectations < 0.6f)
                        {
                            return add(expectations, "FreeWillPriorityExpectionsUnmet".TranslateSimple);
                        }
                        if (expectations < 0.8f)
                        {
                            return add(expectations, "FreeWillPriorityExpectionsLetDown".TranslateSimple);
                        }
                        return add(expectations, "FreeWillPriorityExpectionsIgnored".TranslateSimple);
                    case CLEANING:
                        // check for cleanable
                        if (!mapComp.AreaHasFilth)
                        {
                            // no cleaning job
                            return this;
                        }
                        const float ADJUSTMENT = 0.2f; // made to match change in base default from 0.3 to 0.5
                        if (expectations < 0.2f)
                        {
                            return add(expectations - ADJUSTMENT, "FreeWillPriorityExpectionsExceeded".TranslateSimple);
                        }
                        if (expectations < 0.4f)
                        {
                            return add(expectations - ADJUSTMENT, "FreeWillPriorityExpectionsMet".TranslateSimple);
                        }
                        if (expectations < 0.6f)
                        {
                            return add(expectations - ADJUSTMENT, "FreeWillPriorityExpectionsUnmet".TranslateSimple);
                        }
                        if (expectations < 0.8f)
                        {
                            return add(expectations - ADJUSTMENT, "FreeWillPriorityExpectionsLetDown".TranslateSimple);
                        }
                        return add(expectations - ADJUSTMENT, "FreeWillPriorityExpectionsIgnored".TranslateSimple);
                    default:
                        // any other work type is decreased if either job is present
                        if (WorkTypeDef.defName == SMITHING && MechanitorUtility.IsMechanitor(pawn))
                        {
                            // mechanitor smithing is not affected by beauty
                            return this;
                        }
                        if (!mapComp.AreaHasFilth && !mapComp.AreaHasHaulables)
                        {
                            // nothing to do
                            return this;
                        }
                        if (expectations < 0.2f)
                        {
                            return add(-expectations, "FreeWillPriorityExpectionsExceeded".TranslateSimple);
                        }
                        if (expectations < 0.4f)
                        {
                            return add(-expectations, "FreeWillPriorityExpectionsMet".TranslateSimple);
                        }
                        if (expectations < 0.6f)
                        {
                            return add(-expectations, "FreeWillPriorityExpectionsUnmet".TranslateSimple);
                        }
                        if (expectations < 0.8f)
                        {
                            return add(-expectations, "FreeWillPriorityExpectionsLetDown".TranslateSimple);
                        }
                        return add(-expectations, "FreeWillPriorityExpectionsIgnored".TranslateSimple);
                } // switch
            }
            catch (System.Exception err)
            {
                Log.ErrorOnce("could not consider beauty: " + "this consideration will be disabled in the mod settings to avoid future errors: " + err.ToString(), 228652891);
                worldComp.settings.ConsiderBeauty = 0.0f;
                return this;
            }
        }

        private Priority considerRelevantSkills(bool shouldAdd = false)
        {
            float _badSkillCutoff = Mathf.Min(3f, mapComp.NumPawns);
            float _goodSkillCutoff = _badSkillCutoff + (20f - _badSkillCutoff) / 2f;
            float _greatSkillCutoff = _goodSkillCutoff + (20f - _goodSkillCutoff) / 2f;
            float _excellentSkillCutoff = _greatSkillCutoff + (20f - _greatSkillCutoff) / 2f;

            float _avg = pawn.skills.AverageOfRelevantSkillsFor(WorkTypeDef);
            if (_avg >= _excellentSkillCutoff)
            {
                if (shouldAdd)
                {
                    return add(0.9f, () => string.Format("{0} {1:f0}", "FreeWillPrioritySkillLevel".TranslateSimple(), _avg));
                }
                return set(0.9f, () => string.Format("{0} {1:f0}", "FreeWillPrioritySkillLevel".TranslateSimple(), _avg));
            }
            if (_avg >= _greatSkillCutoff)
            {
                if (shouldAdd)
                {
                    return add(0.7f, () => string.Format("{0} {1:f0}", "FreeWillPrioritySkillLevel".TranslateSimple(), _avg));
                }
                return set(0.7f, () => string.Format("{0} {1:f0}", "FreeWillPrioritySkillLevel".TranslateSimple(), _avg));
            }
            if (_avg >= _goodSkillCutoff)
            {
                if (shouldAdd)
                {
                    return add(0.5f, () => string.Format("{0} {1:f0}", "FreeWillPrioritySkillLevel".TranslateSimple(), _avg));
                }
                return set(0.5f, () => string.Format("{0} {1:f0}", "FreeWillPrioritySkillLevel".TranslateSimple(), _avg));
            }
            if (_avg >= _badSkillCutoff)
            {
                if (shouldAdd)
                {
                    return add(0.3f, () => string.Format("{0} {1:f0}", "FreeWillPrioritySkillLevel".TranslateSimple(), _avg));
                }
                return set(0.3f, () => string.Format("{0} {1:f0}", "FreeWillPrioritySkillLevel".TranslateSimple(), _avg));
            }
            if (shouldAdd)
            {
                return add(0.1f, () => string.Format("{0} {1:f0}", "FreeWillPrioritySkillLevel".TranslateSimple(), _avg));
            }
            return set(0.1f, () => string.Format("{0} {1:f0}", "FreeWillPrioritySkillLevel".TranslateSimple(), _avg));
        }

        private bool notInHomeArea(Pawn pawn)
        {
            return !this.pawn.Map.areaManager.Home[pawn.Position];
        }

        private static Dictionary<string, Dictionary<BeautyCategory, float>> expectationGrid =
            new Dictionary<string, Dictionary<BeautyCategory, float>>
            {
                {
                    "ExtremelyLow", new Dictionary<BeautyCategory, float>
                        {
                            { BeautyCategory.Hideous, 0.3f },
                            { BeautyCategory.VeryUgly, 0.2f },
                            { BeautyCategory.Ugly, 0.1f },
                            { BeautyCategory.Neutral, 0.0f },
                            { BeautyCategory.Pretty, 0.0f },
                            { BeautyCategory.VeryPretty, 0.0f },
                            { BeautyCategory.Beautiful, 0.0f },
                        }
                },
                {
                    "VeryLow", new Dictionary<BeautyCategory, float>
                        {
                            { BeautyCategory.Hideous, 0.5f },
                            { BeautyCategory.VeryUgly, 0.3f },
                            { BeautyCategory.Ugly, 0.2f },
                            { BeautyCategory.Neutral, 0.1f },
                            { BeautyCategory.Pretty, 0.0f },
                            { BeautyCategory.VeryPretty, 0.0f },
                            { BeautyCategory.Beautiful, 0.0f },
                        }
                },
                {
                    "Low", new Dictionary<BeautyCategory, float>
                        {
                            { BeautyCategory.Hideous, 0.7f },
                            { BeautyCategory.VeryUgly, 0.5f },
                            { BeautyCategory.Ugly, 0.3f },
                            { BeautyCategory.Neutral, 0.2f },
                            { BeautyCategory.Pretty, 0.1f },
                            { BeautyCategory.VeryPretty, 0.0f },
                            { BeautyCategory.Beautiful, 0.0f },
                        }
                },
                {
                    "Moderate", new Dictionary<BeautyCategory, float>
                        {
                            { BeautyCategory.Hideous, 0.8f },
                            { BeautyCategory.VeryUgly, 0.7f },
                            { BeautyCategory.Ugly, 0.5f },
                            { BeautyCategory.Neutral, 0.3f },
                            { BeautyCategory.Pretty, 0.2f },
                            { BeautyCategory.VeryPretty, 0.1f },
                            { BeautyCategory.Beautiful, 0.0f },
                        }
                },
                {
                    "High", new Dictionary<BeautyCategory, float>
                        {
                            { BeautyCategory.Hideous, 0.9f },
                            { BeautyCategory.VeryUgly, 0.8f },
                            { BeautyCategory.Ugly, 0.7f },
                            { BeautyCategory.Neutral, 0.5f },
                            { BeautyCategory.Pretty, 0.3f },
                            { BeautyCategory.VeryPretty, 0.2f },
                            { BeautyCategory.Beautiful, 0.1f },
                        }
                },
                {
                    "SkyHigh", new Dictionary<BeautyCategory, float>
                        {
                            { BeautyCategory.Hideous, 1.0f },
                            { BeautyCategory.VeryUgly, 0.9f },
                            { BeautyCategory.Ugly, 0.8f },
                            { BeautyCategory.Neutral, 0.7f },
                            { BeautyCategory.Pretty, 0.5f },
                            { BeautyCategory.VeryPretty, 0.3f },
                            { BeautyCategory.Beautiful, 0.2f },
                        }
                },
                {
                    "Noble", new Dictionary<BeautyCategory, float>
                        {
                            { BeautyCategory.Hideous, 1.0f },
                            { BeautyCategory.VeryUgly, 1.0f },
                            { BeautyCategory.Ugly, 0.9f },
                            { BeautyCategory.Neutral, 0.8f },
                            { BeautyCategory.Pretty, 0.7f },
                            { BeautyCategory.VeryPretty, 0.5f },
                            { BeautyCategory.Beautiful, 0.3f },
                        }
                },
                {
                    "Royal", new Dictionary<BeautyCategory, float>
                        {
                            { BeautyCategory.Hideous, 1.0f },
                            { BeautyCategory.VeryUgly, 1.0f },
                            { BeautyCategory.Ugly, 1.0f },
                            { BeautyCategory.Neutral, 0.9f },
                            { BeautyCategory.Pretty, 0.8f },
                            { BeautyCategory.VeryPretty, 0.7f },
                            { BeautyCategory.Beautiful, 0.5f },
                        }
                },
            };
    }
}
