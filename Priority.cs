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
        private const int DISABLED_CUTOFF = 100 / (Pawn_WorkSettings.LowestPriority + 1); // 20 if LowestPriority is 4
        private const int DISABLED_CUTOFF_ACTIVE_WORK_AREA = 100 - DISABLED_CUTOFF; // 80 if LowestPriority is 4
        private const float ONE_PRIORITY_WIDTH = DISABLED_CUTOFF_ACTIVE_WORK_AREA / (float)Pawn_WorkSettings.LowestPriority; // ~20 if LowestPriority is 4

        private readonly Pawn pawn;
        private FreeWill_WorldComponent worldComp;
        private FreeWill_MapComponent mapComp;
        public WorkTypeDef WorkTypeDef { get; }
        public float Value { get; private set; }
        public List<Func<string>> AdjustmentStrings { get; private set; }

        public bool Enabled { get; private set; }
        public bool Disabled { get; private set; }

        // work types
        private const string FIREFIGHTER = "Firefighter";
        private const string PATIENT = "Patient";
        private const string DOCTOR = "Doctor";
        private const string PATIENT_BED_REST = "PatientBedRest";
        private const string CHILDCARE = "Childcare";
        private const string BASIC_WORKER = "BasicWorker";
        private const string WARDEN = "Warden";
        private const string HANDLING = "Handling";
        private const string COOKING = "Cooking";
        private const string HUNTING = "Hunting";
        private const string CONSTRUCTION = "Construction";
        private const string GROWING = "Growing";
        private const string MINING = "Mining";
        private const string PLANT_CUTTING = "PlantCutting";
        private const string SMITHING = "Smithing";
        private const string TAILORING = "Tailoring";
        private const string ART = "Art";
        private const string CRAFTING = "Crafting";
        private const string HAULING = "Hauling";
        private const string CLEANING = "Cleaning";
        private const string RESEARCHING = "Research";

        // supported modded work types
        private const string HAULING_URGENT = "HaulingUrgent";

        private static readonly int couldNotConvertToGamePriority = ("FreeWill" + "could not convert to game priority").GetHashCode();


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
                _ = Set(0.2f, "FreeWillPriorityGlobalDefault".TranslateSimple).InnerCompute();
                return;
            }
            catch (Exception e)
            {
                if (Prefs.DevMode)
                {
                    throw new Exception("could not compute " + WorkTypeDef.defName + " priority for pawn: " + pawn.Name + ": " + e.Message);
                }
                Log.ErrorOnce("could not compute " + WorkTypeDef.defName + " priority for pawn: " + pawn.Name + ": " + e.Message, 15448413);

                _ = AlwaysDo("FreeWillPriorityError".TranslateSimple)
                .Set(0.4f, "FreeWillPriorityError".TranslateSimple)
                ;
            }
        }

        int IComparable.CompareTo(object obj)
        {
            return obj == null ? 1 : !(obj is Priority p) ? 1 : Value.CompareTo(p.Value);
        }

        private Priority InnerCompute()
        {
            Enabled = false;
            Disabled = false;
            if (mapComp.DisabledWorkTypes.Contains(WorkTypeDef))
            {
                return NeverDo("FreeWillPriorityPermanentlyDisabled".TranslateSimple);
            }
            switch (WorkTypeDef.defName)
            {
                case FIREFIGHTER:
                    return
                        Set(0.0f, "FreeWillPriorityFirefightingDefault".TranslateSimple)
                        .AlwaysDo("FreeWillPriorityFirefightingAlways".TranslateSimple)
                        .NeverDoIf(pawn.Downed, "FreeWillPriorityPawnDowned".TranslateSimple)
                        .ConsiderFire()
                        .ConsiderBuildingImmunity()
                        .ConsiderCompletingTask()
                        .ConsiderColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .ConsiderColonyPolicy()
                        ;

                case PATIENT:
                    return
                        Set(0.0f, "FreeWillPriorityPatientDefault".TranslateSimple)
                        .AlwaysDo("FreeWillPriorityPatientAlways".TranslateSimple)
                        .ConsiderHealth()
                        .ConsiderBuildingImmunity()
                        .ConsiderCompletingTask()
                        .ConsiderColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .ConsiderOperation()
                        .ConsiderColonyPolicy()
                        ;

                case DOCTOR:
                    return
                        ConsiderRelevantSkills()
                        .ConsiderCarryingCapacity()
                        .ConsiderIsAnyoneElseDoing()
                        .ConsiderBestAtDoing()
                        .ConsiderPassion()
                        .ConsiderThoughts()
                        .ConsiderInspiration()
                        .ConsiderInjuredPets()
                        .ConsiderInjuredPrisoners()
                        .ConsiderColonistLeftUnburied()
                        .ConsiderHealth()
                        .ConsiderAteRawFood()
                        .ConsiderBored()
                        .ConsiderFire()
                        .ConsiderBuildingImmunity()
                        .ConsiderCompletingTask()
                        .ConsiderColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .ConsiderColonyPolicy()
                        ;

                case PATIENT_BED_REST:
                    return
                        Set(0.0f, "FreeWillPriorityBedrestDefault".TranslateSimple)
                        .AlwaysDo("FreeWillPriorityBedrestAlways".TranslateSimple)
                        .ConsiderHealth()
                        .ConsiderBuildingImmunity()
                        .ConsiderLowFood(-0.2f)
                        .ConsiderCompletingTask()
                        .ConsiderColonistsNeedingTreatment()
                        .ConsiderBored()
                        .ConsiderDownedColonists()
                        .ConsiderOperation()
                        .ConsiderColonyPolicy()
                        ;

                case CHILDCARE:
                    return
                        Set(0.5f, "FreeWillPriorityChildcareDefault".TranslateSimple)
                        .ConsiderRelevantSkills(shouldAdd: true)
                        .ConsiderPassion()
                        .ConsiderThoughts()
                        .ConsiderInspiration()
                        .ConsiderHealth()
                        .ConsiderAteRawFood()
                        .ConsiderBored()
                        .ConsiderFire()
                        .ConsiderBuildingImmunity()
                        .ConsiderCompletingTask()
                        .ConsiderColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .ConsiderColonyPolicy()
                        ;

                case BASIC_WORKER:
                    return
                        Set(0.5f, "FreeWillPriorityBasicWorkDefault".TranslateSimple)
                        .ConsiderThoughts()
                        .ConsiderHealth()
                        .ConsiderLowFood(-0.3f)
                        .ConsiderBored()
                        .NeverDoIf(pawn.Downed, "FreeWillPriorityPawnDowned".TranslateSimple)
                        .ConsiderBuildingImmunity()
                        .ConsiderCompletingTask()
                        .ConsiderColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .ConsiderColonyPolicy()
                        ;

                case WARDEN:
                    return
                        ConsiderRelevantSkills()
                        .ConsiderCarryingCapacity()
                        .ConsiderIsAnyoneElseDoing()
                        .ConsiderBestAtDoing()
                        .ConsiderPassion()
                        .ConsiderThoughts()
                        .ConsiderInspiration()
                        .ConsiderLowFood(-0.3f)
                        .ConsiderSuppressionNeed()
                        .ConsiderColonistLeftUnburied()
                        .ConsiderHealth()
                        .ConsiderAteRawFood()
                        .ConsiderBored()
                        .ConsiderFire()
                        .ConsiderBuildingImmunity()
                        .ConsiderCompletingTask()
                        .ConsiderColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .ConsiderColonyPolicy()
                        ;

                case HANDLING:
                    return
                        ConsiderRelevantSkills()
                        .ConsiderMovementSpeed()
                        .ConsiderCarryingCapacity()
                        .ConsiderIsAnyoneElseDoing()
                        .ConsiderBestAtDoing()
                        .ConsiderPassion()
                        .ConsiderThoughts()
                        .ConsiderInspiration()
                        .ConsiderAnimalsRoaming()
                        .ConsiderColonistLeftUnburied()
                        .ConsiderHealth()
                        .ConsiderAteRawFood()
                        .ConsiderBored()
                        .ConsiderFire()
                        .ConsiderBuildingImmunity()
                        .ConsiderCompletingTask()
                        .ConsiderColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .ConsiderColonyPolicy()
                        ;

                case COOKING:
                    return
                        ConsiderRelevantSkills()
                        .ConsiderCarryingCapacity()
                        .ConsiderIsAnyoneElseDoing()
                        .ConsiderBestAtDoing()
                        .ConsiderPassion()
                        .ConsiderThoughts()
                        .ConsiderInspiration()
                        .ConsiderLowFood(0.2f)
                        .ConsiderColonistLeftUnburied()
                        .ConsiderFoodPoisoning()
                        .ConsiderHealth()
                        .ConsiderAteRawFood()
                        .ConsiderBored()
                        .ConsiderFire()
                        .ConsiderBuildingImmunity()
                        .ConsiderCompletingTask()
                        .ConsiderColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .ConsiderColonyPolicy()
                        ;

                case HUNTING:
                    return
                        ConsiderRelevantSkills()
                        .ConsiderMovementSpeed()
                        .ConsiderCarryingCapacity()
                        .ConsiderIsAnyoneElseDoing()
                        .ConsiderBestAtDoing()
                        .ConsiderPassion()
                        .ConsiderThoughts()
                        .ConsiderInspiration()
                        .ConsiderLowFood(0.3f)
                        .ConsiderWeaponRange()
                        .ConsiderColonistLeftUnburied()
                        .ConsiderHealth()
                        .ConsiderAteRawFood()
                        .ConsiderBored()
                        .ConsiderHasHuntingWeapon()
                        .ConsiderBrawlersNotHunting()
                        .ConsiderFire()
                        .ConsiderBuildingImmunity()
                        .ConsiderCompletingTask()
                        .ConsiderColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .ConsiderColonyPolicy()
                        ;

                case CONSTRUCTION:
                    return
                        ConsiderRelevantSkills()
                        .ConsiderCarryingCapacity()
                        .ConsiderIsAnyoneElseDoing()
                        .ConsiderBestAtDoing()
                        .ConsiderPassion()
                        .ConsiderThoughts()
                        .ConsiderInspiration()
                        .ConsiderLowFood(-0.3f)
                        .ConsiderColonistLeftUnburied()
                        .ConsiderHealth()
                        .ConsiderAteRawFood()
                        .ConsiderBored()
                        .ConsiderFire()
                        .ConsiderBuildingImmunity()
                        .ConsiderCompletingTask()
                        .ConsiderColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .ConsiderColonyPolicy()
                        ;

                case GROWING:
                    return
                        ConsiderRelevantSkills()
                        .ConsiderCarryingCapacity()
                        .ConsiderIsAnyoneElseDoing()
                        .ConsiderBestAtDoing()
                        .ConsiderPassion()
                        .ConsiderThoughts()
                        .ConsiderInspiration()
                        .ConsiderLowFood(0.3f)
                        .ConsiderColonistLeftUnburied()
                        .ConsiderHealth()
                        .ConsiderAteRawFood()
                        .ConsiderBored()
                        .ConsiderFire()
                        .ConsiderBuildingImmunity()
                        .ConsiderCompletingTask()
                        .ConsiderColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .ConsiderColonyPolicy()
                        ;

                case MINING:
                    return
                        ConsiderRelevantSkills()
                        .ConsiderCarryingCapacity()
                        .ConsiderIsAnyoneElseDoing()
                        .ConsiderBestAtDoing()
                        .ConsiderPassion()
                        .ConsiderThoughts()
                        .ConsiderInspiration()
                        .ConsiderLowFood(-0.3f)
                        .ConsiderColonistLeftUnburied()
                        .ConsiderHealth()
                        .ConsiderAteRawFood()
                        .ConsiderBored()
                        .ConsiderFire()
                        .ConsiderBuildingImmunity()
                        .ConsiderCompletingTask()
                        .ConsiderColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .ConsiderColonyPolicy()
                        ;

                case PLANT_CUTTING:
                    return
                        ConsiderRelevantSkills()
                        .ConsiderIsAnyoneElseDoing()
                        .ConsiderBestAtDoing()
                        .ConsiderPassion()
                        .ConsiderThoughts()
                        .ConsiderInspiration()
                        .ConsiderGauranlenPruning()
                        .ConsiderLowFood(0.3f)
                        .ConsiderHealth()
                        .ConsiderPlantsBlighted()
                        .ConsiderBored()
                        .ConsiderFire()
                        .ConsiderBuildingImmunity()
                        .ConsiderCompletingTask()
                        .ConsiderColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .ConsiderColonyPolicy()
                        ;

                case SMITHING:
                    return
                        ConsiderRelevantSkills()
                        .ConsiderCarryingCapacity()
                        .ConsiderFinishedMechGestators()
                        .ConsiderRepairingMech()
                        .ConsiderIsAnyoneElseDoing()
                        .ConsiderBestAtDoing()
                        .ConsiderBeautyExpectations()
                        .ConsiderPassion()
                        .ConsiderThoughts()
                        .ConsiderInspiration()
                        .ConsiderLowFood(-0.3f)
                        .ConsiderColonistLeftUnburied()
                        .ConsiderHealth()
                        .ConsiderAteRawFood()
                        .ConsiderBored()
                        .ConsiderFire()
                        .ConsiderBuildingImmunity()
                        .ConsiderCompletingTask()
                        .ConsiderColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .ConsiderColonyPolicy()
                        ;

                case TAILORING:
                    return
                        ConsiderRelevantSkills()
                        .ConsiderCarryingCapacity()
                        .ConsiderIsAnyoneElseDoing()
                        .ConsiderBestAtDoing()
                        .ConsiderBeautyExpectations()
                        .ConsiderPassion()
                        .ConsiderThoughts()
                        .ConsiderInspiration()
                        .ConsiderLowFood(-0.3f)
                        .ConsiderNeedingWarmClothes()
                        .ConsiderColonistLeftUnburied()
                        .ConsiderHealth()
                        .ConsiderAteRawFood()
                        .ConsiderBored()
                        .ConsiderFire()
                        .ConsiderBuildingImmunity()
                        .ConsiderCompletingTask()
                        .ConsiderColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .ConsiderColonyPolicy()
                        ;

                case ART:
                    return
                        ConsiderRelevantSkills()
                        .ConsiderCarryingCapacity()
                        .ConsiderIsAnyoneElseDoing()
                        .ConsiderBestAtDoing()
                        .ConsiderBeautyExpectations()
                        .ConsiderPassion()
                        .ConsiderThoughts()
                        .ConsiderInspiration()
                        .ConsiderLowFood(-0.3f)
                        .ConsiderColonistLeftUnburied()
                        .ConsiderHealth()
                        .ConsiderAteRawFood()
                        .ConsiderBored()
                        .ConsiderFire()
                        .ConsiderBuildingImmunity()
                        .ConsiderCompletingTask()
                        .ConsiderColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .ConsiderColonyPolicy()
                        ;

                case CRAFTING:
                    return
                        ConsiderRelevantSkills()
                        .ConsiderCarryingCapacity()
                        .ConsiderIsAnyoneElseDoing()
                        .ConsiderBestAtDoing()
                        .ConsiderBeautyExpectations()
                        .ConsiderPassion()
                        .ConsiderThoughts()
                        .ConsiderInspiration()
                        .ConsiderLowFood(-0.3f)
                        .ConsiderColonistLeftUnburied()
                        .ConsiderHealth()
                        .ConsiderAteRawFood()
                        .ConsiderBored()
                        .ConsiderFire()
                        .ConsiderBuildingImmunity()
                        .ConsiderCompletingTask()
                        .ConsiderColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .ConsiderColonyPolicy()
                        ;

                case HAULING:
                    return
                        Set(0.3f, "FreeWillPriorityHaulingDefault".TranslateSimple)
                        .ConsiderBeautyExpectations()
                        .ConsiderMovementSpeed()
                        .ConsiderCarryingCapacity()
                        .ConsiderIsAnyoneElseDoing()
                        .ConsiderPassion()
                        .ConsiderThoughts()
                        .ConsiderInspiration()
                        .ConsiderRefueling()
                        .ConsiderLowFood(0.2f)
                        .ConsiderColonistLeftUnburied()
                        .ConsiderHealth()
                        .ConsiderAteRawFood()
                        .ConsiderThingsDeteriorating()
                        .ConsiderMechHaulers()
                        .ConsiderBored()
                        .ConsiderFire()
                        .ConsiderBuildingImmunity()
                        .ConsiderCompletingTask()
                        .ConsiderColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .ConsiderColonyPolicy()
                        ;

                case CLEANING:
                    return
                        Set(0.5f, "FreeWillPriorityCleaningDefault".TranslateSimple)
                        .ConsiderBeautyExpectations()
                        .ConsiderIsAnyoneElseDoing()
                        .ConsiderThoughts()
                        .ConsiderOwnRoom()
                        .ConsiderLowFood(-0.2f)
                        .ConsiderFoodPoisoning()
                        .ConsiderHealth()
                        .ConsiderBored()
                        .NeverDoIf(NotInHomeArea(pawn), "FreeWillPriorityNotInHomeArea".TranslateSimple)
                        .ConsiderBuildingImmunity()
                        .ConsiderCompletingTask()
                        .ConsiderColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .ConsiderColonyPolicy()
                        ;

                case RESEARCHING:
                    return
                        ConsiderRelevantSkills()
                        .ConsiderIsAnyoneElseDoing()
                        .ConsiderBestAtDoing()
                        .ConsiderBeautyExpectations()
                        .ConsiderPassion()
                        .ConsiderThoughts()
                        .ConsiderInspiration()
                        .ConsiderLowFood(-0.4f)
                        .ConsiderColonistLeftUnburied()
                        .ConsiderHealth()
                        .ConsiderAteRawFood()
                        .ConsiderBored()
                        .ConsiderFire()
                        .ConsiderBuildingImmunity()
                        .ConsiderColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .ConsiderColonyPolicy()
                        ;

                case HAULING_URGENT:
                    return
                        Set(0.5f, "FreeWillPriorityUrgentHaulingDefault".TranslateSimple)
                        .ConsiderBeautyExpectations()
                        .ConsiderMovementSpeed()
                        .ConsiderCarryingCapacity()
                        .ConsiderIsAnyoneElseDoing()
                        .ConsiderPassion()
                        .ConsiderThoughts()
                        .ConsiderInspiration()
                        .ConsiderRefueling()
                        .ConsiderLowFood(0.3f)
                        .ConsiderColonistLeftUnburied()
                        .ConsiderHealth()
                        .ConsiderAteRawFood()
                        .ConsiderThingsDeteriorating()
                        .ConsiderBored()
                        .ConsiderFire()
                        .ConsiderBuildingImmunity()
                        .ConsiderCompletingTask()
                        .ConsiderColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .ConsiderColonyPolicy()
                        ;

                default:
                    return
                        ConsiderRelevantSkills()
                        .ConsiderMovementSpeed()
                        .ConsiderCarryingCapacity()
                        .ConsiderBeautyExpectations()
                        .ConsiderIsAnyoneElseDoing()
                        .ConsiderBestAtDoing()
                        .ConsiderPassion()
                        .ConsiderThoughts()
                        .ConsiderInspiration()
                        .ConsiderLowFood(-0.3f)
                        .ConsiderColonistLeftUnburied()
                        .ConsiderHealth()
                        .ConsiderAteRawFood()
                        .ConsiderBored()
                        .ConsiderFire()
                        .ConsiderBuildingImmunity()
                        .ConsiderCompletingTask()
                        .ConsiderColonistsNeedingTreatment()
                        .ConsiderDownedColonists()
                        .ConsiderColonyPolicy()
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
                int valueInt = Mathf.Clamp(Mathf.RoundToInt(Value * 100), 0, 100);
                if (valueInt <= DISABLED_CUTOFF)
                {
                    return Enabled ? Pawn_WorkSettings.LowestPriority : 0;
                }
                if (Disabled)
                {
                    return 0;
                }
                int invertedValueRange = DISABLED_CUTOFF_ACTIVE_WORK_AREA - (valueInt - DISABLED_CUTOFF); // 0-80 if LowestPriority is 4
                int gamePriorityValue = Mathf.FloorToInt(invertedValueRange / ONE_PRIORITY_WIDTH) + 1;
                if (gamePriorityValue > Pawn_WorkSettings.LowestPriority || gamePriorityValue < 1)
                {
                    Log.Error("calculated an invalid game priority value of " + gamePriorityValue.ToString());
                    gamePriorityValue = Mathf.Clamp(gamePriorityValue, 1, Pawn_WorkSettings.LowestPriority);
                }

                return gamePriorityValue;
            }
            catch (Exception e)
            {
                Log.ErrorOnce("could not convert to game priority: " + e.ToString(), couldNotConvertToGamePriority);
                return 0;
            }
        }

        private Priority Set(float x, Func<string> description)
        {
            Value = Mathf.Clamp01(x);
            string adjustmentString()
            {
                // Create string builder
                StringBuilder stringBuilder = new StringBuilder()
                    .Append(" - ")
                    .Append(description().CapitalizeFirst())
                    .Append(": ")
                    .Append(Value.ToStringPercent());
                return stringBuilder.ToString();
            }
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

        private Priority Add(float x, Func<string> description)
        {
            if (Disabled)
            {
                return this;
            }
            float newValue = Mathf.Clamp01(Value + x);
            if (newValue > Value)
            {
                string adjustmentString()
                {
                    // Create string builder
                    StringBuilder stringBuilder = new StringBuilder()
                        .Append(" - ")
                        .Append(description().CapitalizeFirst())
                        .Append(": +")
                        .Append((newValue - Value).ToStringPercent());
                    return stringBuilder.ToString();
                }
                AdjustmentStrings.Add(adjustmentString);
                Value = newValue;
            }
            else if (newValue < Value)
            {
                string adjustmentString()
                {
                    // Create string builder
                    StringBuilder stringBuilder = new StringBuilder()
                        .Append(" - ")
                        .Append(description().CapitalizeFirst())
                        .Append(": -")
                        .Append((Value - newValue).ToStringPercent());
                    return stringBuilder.ToString();
                }
                AdjustmentStrings.Add(adjustmentString);
                Value = newValue;
            }
            else if (newValue == Value && Prefs.DevMode)
            {
                string adjustmentString()
                {
                    // Create string builder
                    StringBuilder stringBuilder = new StringBuilder()
                        .Append(" - ")
                        .Append(description().CapitalizeFirst())
                        .Append(": +")
                        .Append((newValue - Value).ToStringPercent());
                    return stringBuilder.ToString();
                }
                AdjustmentStrings.Add(adjustmentString);
                Value = newValue;
            }
            return this;
        }

        private Priority Multiply(float x, Func<string> description)
        {
            if (Disabled || Value == 0.0f)
            {
                return this;
            }
            float newClampedValue = Mathf.Clamp01(Value * x);
            if (newClampedValue != Value)
            {
                string adjustmentString()
                {
                    // Create string builder
                    StringBuilder stringBuilder = new StringBuilder()
                        .Append(" - ")
                        .Append(description().CapitalizeFirst())
                        .Append(": x")
                        .Append((newClampedValue / Value).ToStringPercent());
                    return stringBuilder.ToString();
                }
                AdjustmentStrings.Add(adjustmentString);
                Value = newClampedValue;
            }
            else if (newClampedValue == Value && Prefs.DevMode)
            {
                string adjustmentString()
                {
                    // Create string builder
                    StringBuilder stringBuilder = new StringBuilder()
                        .Append(" - ")
                        .Append(description().CapitalizeFirst())
                        .Append(": x")
                        .Append((newClampedValue / Value).ToStringPercent());
                    return stringBuilder.ToString();
                }
                AdjustmentStrings.Add(adjustmentString);
                Value = newClampedValue;
            }
            return this;
        }

        private Priority AlwaysDoIf(bool cond, Func<string> description)
        {
            if (!cond || Enabled)
            {
                return this;
            }
            if (Prefs.DevMode || Disabled || ToGamePriority() == 0)
            {
                string adjustmentString()
                {
                    // Create string builder
                    StringBuilder stringBuilder = new StringBuilder()
                        .Append(" - ")
                        .Append(description().CapitalizeFirst())
                        .Append(": ")
                        .Append("FreeWillPriorityEnabled".TranslateSimple().CapitalizeFirst());
                    return stringBuilder.ToString();
                }
                AdjustmentStrings.Add(adjustmentString);
            }
            Enabled = true;
            Disabled = false;
            return this;
        }

        private Priority AlwaysDo(Func<string> description)
        {
            return AlwaysDoIf(true, description);
        }

        private Priority NeverDoIf(bool cond, Func<string> description)
        {
            if (!cond || Disabled)
            {
                return this;
            }
            if (Prefs.DevMode || Enabled || ToGamePriority() >= 0)
            {
                string adjustmentString()
                {
                    // Create string builder
                    StringBuilder stringBuilder = new StringBuilder()
                        .Append(" - ")
                        .Append(description().CapitalizeFirst())
                        .Append(": ")
                        .Append("FreeWillPriorityDisabled".TranslateSimple().CapitalizeFirst());
                    return stringBuilder.ToString();
                }
                AdjustmentStrings.Add(adjustmentString);
            }
            Disabled = true;
            Enabled = false;
            return this;
        }

        private Priority NeverDo(Func<string> description)
        {
            return NeverDoIf(true, description);
        }

        private Priority ConsiderInspiration()
        {
            if (!pawn.mindState.inspirationHandler.Inspired)
            {
                return this;
            }
            Inspiration inspiration = pawn.mindState.inspirationHandler.CurState;
            if (WorkTypeDef.defName == HUNTING && inspiration.def.defName == "Frenzy_Shoot")
            {
                return Add(0.4f, "FreeWillPriorityInspired".TranslateSimple);
            }
            foreach (WorkTypeDef workTypeDefB in inspiration?.def?.requiredNonDisabledWorkTypes ?? new List<WorkTypeDef>())
            {
                if (WorkTypeDef.defName == workTypeDefB.defName)
                {
                    return Add(0.4f, "FreeWillPriorityInspired".TranslateSimple);
                }
            }
            foreach (WorkTypeDef workTypeDefB in inspiration?.def?.requiredAnyNonDisabledWorkType ?? new List<WorkTypeDef>())
            {
                if (WorkTypeDef.defName == workTypeDefB.defName)
                {
                    return Add(0.4f, "FreeWillPriorityInspired".TranslateSimple);
                }
            }
            return this;
        }

        private Priority ConsiderThoughts()
        {
            foreach (Thought thought in mapComp.AllThoughts)
            {
                if (thought.def.defName == "NeedFood")
                {
                    return WorkTypeDef.defName == COOKING
                        ? Add(-0.01f * thought.CurStage.baseMoodEffect, "FreeWillPriorityHungerLevel".TranslateSimple)
                        : WorkTypeDef.defName == HUNTING || WorkTypeDef.defName == PLANT_CUTTING
                        ? Add(-0.005f * thought.CurStage.baseMoodEffect, "FreeWillPriorityHungerLevel".TranslateSimple)
                        : Add(0.005f * thought.CurStage.baseMoodEffect, "FreeWillPriorityHungerLevel".TranslateSimple);
                }
            }
            return this;
        }

        private Priority ConsiderNeedingWarmClothes()
        {
            return mapComp.NeedWarmClothes ? Add(0.2f, "FreeWillPriorityNeedWarmClothes".TranslateSimple) : this;
        }

        private Priority ConsiderAnimalsRoaming()
        {
            return mapComp.AlertAnimalRoaming ? Add(0.4f, "FreeWillPriorityAnimalsRoaming".TranslateSimple) : this;
        }

        private Priority ConsiderSuppressionNeed()
        {
            return mapComp.SuppressionNeed != 0.0f ? Add(mapComp.SuppressionNeed, "FreeWillPrioritySuppressionNeed".TranslateSimple) : this;
        }

        private Priority ConsiderColonistLeftUnburied()
        {
            return mapComp.AlertColonistLeftUnburied && (WorkTypeDef.defName == HAULING || WorkTypeDef.defName == HAULING_URGENT)
                ? Add(0.4f, "FreeWillPriorityColonistLeftUnburied".TranslateSimple)
                : this;
        }

        private Priority ConsiderBored()
        {
            const int boredomMemory = 2500; // 1 hour in game
            if (pawn.mindState.IsIdle)
            {
                mapComp?.UpdateLastBored(pawn);
                return AlwaysDoIf(pawn.mindState.IsIdle, "FreeWillPriorityBored".TranslateSimple);
            }
            int? lastBored = mapComp?.GetLastBored(pawn);
            bool wasBored = lastBored != 0 && Find.TickManager.TicksGame - lastBored < boredomMemory;
            return AlwaysDoIf(wasBored, "FreeWillPriorityWasBored".TranslateSimple);
        }

        private Priority ConsiderHasHuntingWeapon()
        {
            try
            {
                return !worldComp.Settings.ConsiderHasHuntingWeapon
                    ? this
                    : NeverDoIf(!WorkGiver_HunterHunt.HasHuntingWeapon(pawn), "FreeWillPriorityNoHuntingWeapon".TranslateSimple);
            }
            catch (Exception err)
            {
                Log.Error(pawn.Name + " could not consider has hunting weapon to adjust " + WorkTypeDef.defName);
                Log.Message(err.ToString());
                Log.Message("this consideration will be disabled in the mod settings to avoid future errors");
                worldComp.Settings.ConsiderHasHuntingWeapon = false;
                return this;
            }
        }

        private Priority ConsiderBrawlersNotHunting()
        {
            if (!worldComp.Settings.ConsiderBrawlersNotHunting)
            {
                return this;
            }
            try
            {
                return WorkTypeDef.defName != HUNTING
                    ? this
                    : NeverDoIf(pawn.story.traits.HasTrait(DefDatabase<TraitDef>.GetNamed("Brawler")), "FreeWillPriorityBrawler".TranslateSimple);
            }
            catch (Exception err)
            {
                Log.Error(pawn.Name + " could not consider brawlers can hunt to adjust " + WorkTypeDef.defName);
                Log.Message(err.ToString());
                Log.Message("this consideration will be disabled in the mod settings to avoid future errors");
                worldComp.Settings.ConsiderBrawlersNotHunting = false;
                return this;
            }
        }

        private Priority ConsiderCompletingTask()
        {
            return pawn.CurJob != null && pawn.CurJob.workGiverDef != null && pawn.CurJob.workGiverDef.workType == WorkTypeDef
                ? AlwaysDo("FreeWillPriorityCurrentlyDoing".TranslateSimple)

                    // pawns prefer the work they are current doing
                    .Multiply(1.8f, "FreeWillPriorityCurrentlyDoing".TranslateSimple)
                : this;
        }

        private Priority ConsiderMovementSpeed()
        {
            try
            {
                return worldComp.Settings.ConsiderMovementSpeed == 0.0f
                    ? this
                    : Multiply(worldComp.Settings.ConsiderMovementSpeed * 0.25f * pawn.GetStatValue(StatDefOf.MoveSpeed, true), "FreeWillPriorityMovementSpeed".TranslateSimple);
            }
            catch (Exception err)
            {
                Log.Message(pawn.Name + " could not consider movement speed to adjust " + WorkTypeDef.defName);
                Log.Message(err.ToString());
                Log.Message("this consideration will be disabled in the mod settings to avoid future errors");
                worldComp.Settings.ConsiderMovementSpeed = 0.0f;
                return this;
            }
        }

        private Priority ConsiderCarryingCapacity()
        {
            float _baseCarryingCapacity = 75.0f;
            if (WorkTypeDef.defName != HAULING && WorkTypeDef.defName != HAULING_URGENT)
            {
                return this;
            }
            float _carryingCapacity = pawn.GetStatValue(StatDefOf.CarryingCapacity, true);
            return _carryingCapacity >= _baseCarryingCapacity
                ? this
                : Multiply(_carryingCapacity / _baseCarryingCapacity, "FreeWillPriorityCarryingCapacity".TranslateSimple);
        }

        private Priority ConsiderPassion()
        {
            try
            {
                if (worldComp.Settings.ConsiderPassions == 0f)
                {
                    return this;
                }
                List<SkillDef> relevantSkills = WorkTypeDef.relevantSkills;
                float x;
                for (int i = 0; i < relevantSkills.Count; i++)
                {
                    int index = i;
                    switch (pawn.skills.GetSkill(relevantSkills[i]).passion)
                    {
                        case Passion.None:
                            continue;
                        case Passion.Major:
                            x = worldComp.Settings.ConsiderPassions * pawn.needs.mood.CurLevel * 0.5f / relevantSkills.Count;

                            _ = AlwaysDo(() => "FreeWillPriorityMajorPassionFor".Translate(relevantSkills[index].skillLabel))
                                .Add(x, () => "FreeWillPriorityMajorPassionFor".Translate(relevantSkills[index].skillLabel));
                            continue;
                        case Passion.Minor:
                            x = worldComp.Settings.ConsiderPassions * pawn.needs.mood.CurLevel * 0.25f / relevantSkills.Count;

                            _ = AlwaysDo(() => "FreeWillPriorityMinorPassionFor".Translate(relevantSkills[index].skillLabel))
                                .Add(x, () => "FreeWillPriorityMinorPassionFor".Translate(relevantSkills[index].skillLabel));
                            continue;
                        default:
                            continue;
                    }
                }
            }
            catch (Exception err)
            {
                Log.ErrorOnce("could not consider passions: " + "this consideration will be disabled in the mod settings to avoid future errors: " + err.ToString(), 228486541);
                worldComp.Settings.ConsiderPassions = 0.0f;
            }
            return this;
        }

        private Priority ConsiderFinishedMechGestators()
        {
            List<Thing> mechGestators = pawn?.Map?.listerThings?.ThingsInGroup(ThingRequestGroup.MechGestator);
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
                    return Add(0.4f, "FreeWillPriorityMechGestator".TranslateSimple);
                }
            }
            return this;
        }

        private Priority ConsiderDownedColonists()
        {
            return pawn.Downed
                ? WorkTypeDef.defName == PATIENT || WorkTypeDef.defName == PATIENT_BED_REST
                    ? AlwaysDo("FreeWillPriorityPawnDowned".TranslateSimple).Set(1.0f, "FreeWillPriorityPawnDowned".TranslateSimple)
                    : NeverDo("FreeWillPriorityPawnDowned".TranslateSimple)
                : mapComp.PercentPawnsDowned <= 0.0f
                ? this
                : WorkTypeDef.defName == DOCTOR
                ? Add(mapComp.PercentPawnsDowned, "FreeWillPriorityOtherPawnsDowned".TranslateSimple)
                : WorkTypeDef.defName == SMITHING ||
                WorkTypeDef.defName == TAILORING ||
                WorkTypeDef.defName == ART ||
                WorkTypeDef.defName == CRAFTING ||
                WorkTypeDef.defName == RESEARCHING
                ? NeverDo("FreeWillPriorityOtherPawnsDowned".TranslateSimple)
                : this;
        }

        private Priority ConsiderColonyPolicy()
        {
            try
            {
                return Add(worldComp.Settings.globalWorkAdjustments[WorkTypeDef.defName], "FreeWillPriorityColonyPolicy".TranslateSimple);
            }
            catch (Exception)
            {
                worldComp.Settings.globalWorkAdjustments[WorkTypeDef.defName] = 0.0f;
            }
            return this;
        }

        private Priority ConsiderRefueling()
        {
            return mapComp.RefuelNeededNow ? Add(0.35f, "FreeWillPriorityRefueling".TranslateSimple)
                : mapComp.RefuelNeeded ? Add(0.20f, "FreeWillPriorityRefueling".TranslateSimple)
                : this;
        }

        private Priority ConsiderFire()
        {
            return mapComp.HomeFire ? WorkTypeDef.defName != FIREFIGHTER ? Add(-0.2f, "FreeWillPriorityFireInHomeArea".TranslateSimple)
                    : Set(1.0f, "FreeWillPriorityFireInHomeArea".TranslateSimple)
                : mapComp.MapFires > 0 && WorkTypeDef.defName == FIREFIGHTER ? Add(Mathf.Clamp01(mapComp.MapFires * 0.01f), "FreeWillPriorityFireOnMap".TranslateSimple)
                : this;
        }

        private Priority ConsiderOperation()
        {
            return HealthAIUtility.ShouldHaveSurgeryDoneNow(pawn) ? Set(1.0f, "FreeWillPriorityOperation".TranslateSimple) : this;
        }

        private Priority ConsiderBuildingImmunity()
        {
            try
            {
                return !pawn.health.hediffSet.HasImmunizableNotImmuneHediff()
                    ? this
                    : WorkTypeDef.defName == PATIENT_BED_REST
                    ? Add(0.4f, "FreeWillPriorityBuildingImmunity".TranslateSimple)
                    : WorkTypeDef.defName == PATIENT ? this : Add(-0.2f, "FreeWillPriorityBuildingImmunity".TranslateSimple);
            }
            catch
            {
                Log.Message("could not consider pawn building immunity");
                return this;
            }
        }

        private Priority ConsiderColonistsNeedingTreatment()
        {
            if (mapComp.PercentPawnsNeedingTreatment <= 0.0f)
            {
                return this;
            }

            if (pawn.health.HasHediffsNeedingTend())
            {
                // this pawn needs treatment
                return ConsiderThisPawnNeedsTreatment();
            }
            else
            {
                // another pawn needs treatment
                return ConsiderAnotherPawnNeedsTreatment();
            }
        }

        private Priority ConsiderThisPawnNeedsTreatment()
        {

            if (WorkTypeDef.defName == PATIENT || WorkTypeDef.defName == PATIENT_BED_REST)
            {
                // patient and bed rest are activated and set to 100%
                return
                    AlwaysDo("FreeWillPriorityNeedTreatment".TranslateSimple)
                    .Set(1.0f, "FreeWillPriorityNeedTreatment".TranslateSimple)
                    ;
            }
            if (WorkTypeDef.defName == DOCTOR)
            {
                if (pawn.playerSettings.selfTend)
                {
                    // this pawn can self tend, so activate doctor skill and set
                    // to 100%
                    return
                        AlwaysDo("FreeWillPriorityNeedTreatmentSelfTend".TranslateSimple)
                        .Set(1.0f, "FreeWillPriorityNeedTreatmentSelfTend".TranslateSimple)
                        ;
                }
                // doctoring stays the same
                return this;
            }
            // don't do other work types
            return NeverDo("FreeWillPriorityNeedTreatment".TranslateSimple);
        }

        private Priority ConsiderAnotherPawnNeedsTreatment()
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
                return Add(mapComp.PercentPawnsNeedingTreatment, "FreeWillPriorityOthersNeedTreatment".TranslateSimple);
            }

            if (WorkTypeDef.defName == RESEARCHING)
            {
                // don't research when someone is dying please... it's rude
                return NeverDo("FreeWillPriorityOthersNeedTreatment".TranslateSimple);
            }

            if (WorkTypeDef.defName == SMITHING ||
                WorkTypeDef.defName == TAILORING ||
                WorkTypeDef.defName == ART ||
                WorkTypeDef.defName == CRAFTING
                )
            {
                // crafting work types are low priority when someone is injured
                return Value > 0.3f ? Add(-(Value - 0.3f), "FreeWillPriorityOthersNeedTreatment".TranslateSimple) : this;
            }

            // any other work type is capped at 0.6
            return Value > 0.6f ? Add(-(Value - 0.6f), "FreeWillPriorityOthersNeedTreatment".TranslateSimple) : this;
        }

        private Priority ConsiderHealth()
        {
            return WorkTypeDef.defName == PATIENT || WorkTypeDef.defName == PATIENT_BED_REST
                ? Add(1 - Mathf.Pow(pawn.health.summaryHealth.SummaryHealthPercent, 7.0f), "FreeWillPriorityHealth".TranslateSimple)
                : Multiply(pawn.health.summaryHealth.SummaryHealthPercent, "FreeWillPriorityHealth".TranslateSimple);
        }

        private Priority ConsiderFoodPoisoning()
        {
            if (worldComp.Settings.ConsiderFoodPoisoning == 0.0f)
            {
                return this;
            }
            try
            {
                if (WorkTypeDef.defName != CLEANING && WorkTypeDef.defName != COOKING)
                {
                    return this;
                }

                float adjustment = 0.0f;
                Room room = pawn.GetRoom();
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
                        adjustment = worldComp.Settings.ConsiderFoodPoisoning * 20.0f * pawn.GetRoom().GetStat(RoomStatDefOf.FoodPoisonChance);
                        if (WorkTypeDef.defName == CLEANING)
                        {
                            return Add(adjustment, "FreeWillPriorityFilthyCookingArea".TranslateSimple);
                        }
                        if (WorkTypeDef.defName == COOKING)
                        {
                            return Add(-adjustment, "FreeWillPriorityFilthyCookingArea".TranslateSimple);
                        }
                    }
                }
                return this;
            }
            catch (Exception err)
            {
                Log.Error(pawn.Name + " could not consider food poisoning to adjust " + WorkTypeDef.defName);
                Log.Message(err.ToString());
                Log.Message("this consideration will be disabled in the mod settings to avoid future errors");
                worldComp.Settings.ConsiderFoodPoisoning = 0.0f;
                return this;
            }
        }

        private Priority ConsiderOwnRoom()
        {
            if (worldComp.Settings.ConsiderOwnRoom == 0.0f)
            {
                return this;
            }
            try
            {
                if (WorkTypeDef.defName != CLEANING)
                {
                    return this;
                }
                Room room = pawn.GetRoom();
                bool isPawnsRoom = false;
                foreach (Pawn owner in room.Owners)
                {
                    if (pawn == owner)
                    {
                        isPawnsRoom = true;
                        break;
                    }
                }
                return !isPawnsRoom ? this : Multiply(worldComp.Settings.ConsiderOwnRoom * 2.0f, "FreeWillPriorityOwnRoom".TranslateSimple);
            }
            catch (Exception err)
            {
                Log.Message(pawn.Name + " could not consider being in own room to adjust " + WorkTypeDef.defName);
                Log.Message(err.ToString());
                Log.Message("this consideration will be disabled in the mod settings to avoid future errors");
                worldComp.Settings.ConsiderOwnRoom = 0.0f;
                return this;
            }
        }

        private Priority ConsiderRepairingMech()
        {
            return !mapComp.AlertMechDamaged
                ? this
                : !MechanitorUtility.IsMechanitor(pawn) ? this : Add(0.6f, "FreeWillPriorityMechanoidDamaged".TranslateSimple);
        }

        private Priority ConsiderIsAnyoneElseDoing()
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
                return AlwaysDo("FreeWillPriorityNoOneElseDoing".TranslateSimple);
            }
            catch (Exception err)
            {
                Log.ErrorOnce(pawn.Name + " could not consider if anyone else is doing " + WorkTypeDef.defName + ": " + err.ToString(), 59947211);
                return this;
            }

        }

        private Priority ConsiderBestAtDoing()
        {
            try
            {
                if (worldComp.Settings.ConsiderBestAtDoing == 0.0f)
                {
                    return this;
                }
                List<Pawn> allPawns = mapComp.PawnsInFaction;
                if (allPawns.Count() <= 1)
                {
                    return this;
                }
                bool isBest = true;
                float pawnSkill = pawn.skills.AverageOfRelevantSkillsFor(WorkTypeDef);
                float impactPerPawn = worldComp.Settings.ConsiderBestAtDoing / allPawns.Count();
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
                    float otherSkill = other.IsColonistPlayerControlled ? other.skills.AverageOfRelevantSkillsFor(WorkTypeDef) : other.RaceProps.mechFixedSkillLevel;
                    float skillDiff = otherSkill - pawnSkill;
                    if (skillDiff > 0.0f)
                    {
                        // not the best
                        isBest = false;
                        bool isDoing = other.CurJob != null && other.CurJob.workGiverDef != null && other.CurJob.workGiverDef.workType == WorkTypeDef;
                        bool isMuchBetter = skillDiff >= 5.0f;
                        bool isMuchMuchBetter = skillDiff >= 10.0f;
                        bool isMuchMuchMuchBetter = skillDiff >= 15.0f;
                        _ = isDoing
                            ? isMuchMuchMuchBetter ? Add(1.5f * -impactPerPawn, () => "FreeWillPrioritySomeoneMuchMuchMuchBetterIsDoing".Translate(other.LabelShortCap))
                              : isMuchMuchBetter ? Add(1.5f * -impactPerPawn * 0.8f, () => "FreeWillPrioritySomeoneMuchMuchBetterIsDoing".Translate(other.LabelShortCap))
                              : isMuchBetter ? Add(1.5f * -impactPerPawn * 0.6f, () => "FreeWillPrioritySomeoneMuchBetterIsDoing".Translate(other.LabelShortCap))
                              : Add(1.5f * -impactPerPawn * 0.4f, () => "FreeWillPrioritySomeoneBetterIsDoing".Translate(other.LabelShortCap))
                            : isMuchMuchMuchBetter ? Add(-impactPerPawn, () => "FreeWillPrioritySomeoneMuchMuchMuchBetterAtDoing".Translate(other.LabelShortCap))
                              : isMuchMuchBetter ? Add(-impactPerPawn * 0.8f, () => "FreeWillPrioritySomeoneMuchMuchBetterAtDoing".Translate(other.LabelShortCap))
                              : isMuchBetter ? Add(-impactPerPawn * 0.6f, () => "FreeWillPrioritySomeoneMuchBetterAtDoing".Translate(other.LabelShortCap))
                              : Add(-impactPerPawn * 0.4f, () => "FreeWillPrioritySomeoneBetterAtDoing".Translate(other.LabelShortCap));
                    }
                }
                return isBest ? Multiply(1.5f * worldComp.Settings.ConsiderBestAtDoing, "FreeWillPriorityBestAtDoing".TranslateSimple) : this;
            }
            catch (Exception err)
            {
                Log.ErrorOnce("could not consider being the best at something: " + "this consideration will be disabled in the mod settings to avoid future errors: " + err.ToString(), 76898214);
                worldComp.Settings.ConsiderBestAtDoing = 0.0f;
            }
            return this;
        }

        private Priority ConsiderInjuredPets()
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
            return Add(Mathf.Clamp01(numPetsNeedingTreatment / n) * 0.5f, "FreeWillPriorityPetsInjured".TranslateSimple);
        }

        private Priority ConsiderMechHaulers()
        {
            float percentPawnsMechHaulers = mapComp.PercentPawnsMechHaulers;
            return Add(-percentPawnsMechHaulers, "FreeWillPriorityMechHaulers".TranslateSimple);
        }

        private Priority ConsiderInjuredPrisoners()
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
            return Add(Mathf.Clamp01(numPrisonersNeedingTreatment / n) * 0.5f, "FreeWillPriorityPrisonersInjured".TranslateSimple);
        }

        private Priority ConsiderLowFood(float adjustment)
        {
            try
            {
                if (worldComp.Settings.ConsiderLowFood == 0.0f || !mapComp.AlertLowFood)
                {
                    return this;
                }

                // don't adjust hauling if nothing deteriorating (i.e. food in the field)
                return (WorkTypeDef.defName == HAULING || WorkTypeDef.defName == HAULING_URGENT)
                    && mapComp.ThingsDeteriorating == null
                    ? this
                    : Add(adjustment * worldComp.Settings.ConsiderLowFood, "FreeWillPriorityLowFood".TranslateSimple);
            }
            catch (Exception err)
            {
                Log.ErrorOnce("could not consider low food: " + "this consideration will be disabled in the mod settings to avoid future errors: " + err.ToString(), 10979710);
                worldComp.Settings.ConsiderLowFood = 0.0f;
            }
            return this;
        }

        private Priority ConsiderWeaponRange()
        {
            if ((worldComp?.Settings?.ConsiderWeaponRange ?? 0.0f) == 0.0f)
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
            return Multiply(relativeRange * worldComp.Settings.ConsiderWeaponRange, "FreeWillPriorityWeaponRange".TranslateSimple);
        }

        private Priority ConsiderAteRawFood()
        {
            if (WorkTypeDef.defName != COOKING)
            {
                return this;
            }

            foreach (Thought thought in mapComp.AllThoughts)
            {
                if (thought.def.defName == "AteRawFood" && 0.6f > Value)
                {
                    return Set(0.6f, "FreeWillPriorityAteRawFood".TranslateSimple);
                }
            }
            return this;
        }

        private Priority ConsiderThingsDeteriorating()
        {
            if (WorkTypeDef.defName == HAULING || WorkTypeDef.defName == HAULING_URGENT)
            {
                if (mapComp.ThingsDeteriorating != null)
                {
                    if (Prefs.DevMode)
                    {
                        string adjustmentString()
                        {
                            // Create string builder
                            StringBuilder stringBuilder = new StringBuilder()
                                .Append("FreeWillPriorityThingsDeteriorating".TranslateSimple())
                                .Append(": ")
                                .Append(mapComp.ThingsDeteriorating.def.defName);
                            return stringBuilder.ToString();
                        }
                        return Multiply(2.0f, adjustmentString);
                    }
                    return Multiply(2.0f, "FreeWillPriorityThingsDeteriorating".TranslateSimple);
                }
            }
            return this;
        }

        private Priority ConsiderPlantsBlighted()
        {
            try
            {
                if (worldComp.Settings.ConsiderPlantsBlighted == 0.0f)
                {
                    // no point checking if it is disabled
                    return this;
                }
                if (mapComp.PlantsBlighted)
                {
                    return Add(0.4f * worldComp.Settings.ConsiderPlantsBlighted, "FreeWillPriorityBlight".TranslateSimple);
                }
            }
            catch (Exception err)
            {
                Log.Message("could not consider blight levels");
                Log.Message(err.ToString());
                Log.Message("this consideration will be disabled in the mod settings to avoid future errors");
                worldComp.Settings.ConsiderPlantsBlighted = 0.0f;
            }
            return this;
        }

        private Priority ConsiderGauranlenPruning()
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
                        return !compTreeConnection.ShouldBePrunedNow(false)
                            ? this
                            : Multiply(2.0f * worldComp.Settings.ConsiderGauranlenPruning, "FreeWillPriorityPruneGauranlenTree".TranslateSimple);
                        {
                        }
                    }
                }
            }
            catch (Exception err)
            {
                Log.ErrorOnce("could not consider pruning gauranlen tree: " + "this consideration will be disabled in the mod settings to avoid future errors: " + err.ToString(), 45846314);
                worldComp.Settings.ConsiderGauranlenPruning = 0.0f;
            }
            return this;
        }

        private Priority ConsiderBeautyExpectations()
        {
            try
            {
                if (worldComp.Settings.ConsiderBeauty == 0.0f)
                {
                    return this;
                }
                float expectations = worldComp.Settings.ConsiderBeauty * expectationGrid[ExpectationsUtility.CurrentExpectationFor(pawn).defName][pawn.needs.beauty.CurCategory];
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
                            return Add(expectations, "FreeWillPriorityExpectionsExceeded".TranslateSimple);
                        }
                        if (expectations < 0.4f)
                        {
                            return Add(expectations, "FreeWillPriorityExpectionsMet".TranslateSimple);
                        }
                        if (expectations < 0.6f)
                        {
                            return Add(expectations, "FreeWillPriorityExpectionsUnmet".TranslateSimple);
                        }
                        if (expectations < 0.8f)
                        {
                            return Add(expectations, "FreeWillPriorityExpectionsLetDown".TranslateSimple);
                        }
                        return Add(expectations, "FreeWillPriorityExpectionsIgnored".TranslateSimple);
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
                            return Add(expectations - ADJUSTMENT, "FreeWillPriorityExpectionsExceeded".TranslateSimple);
                        }
                        if (expectations < 0.4f)
                        {
                            return Add(expectations - ADJUSTMENT, "FreeWillPriorityExpectionsMet".TranslateSimple);
                        }
                        if (expectations < 0.6f)
                        {
                            return Add(expectations - ADJUSTMENT, "FreeWillPriorityExpectionsUnmet".TranslateSimple);
                        }
                        if (expectations < 0.8f)
                        {
                            return Add(expectations - ADJUSTMENT, "FreeWillPriorityExpectionsLetDown".TranslateSimple);
                        }
                        return Add(expectations - ADJUSTMENT, "FreeWillPriorityExpectionsIgnored".TranslateSimple);
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
                            return Add(-expectations, "FreeWillPriorityExpectionsExceeded".TranslateSimple);
                        }
                        if (expectations < 0.4f)
                        {
                            return Add(-expectations, "FreeWillPriorityExpectionsMet".TranslateSimple);
                        }
                        if (expectations < 0.6f)
                        {
                            return Add(-expectations, "FreeWillPriorityExpectionsUnmet".TranslateSimple);
                        }
                        if (expectations < 0.8f)
                        {
                            return Add(-expectations, "FreeWillPriorityExpectionsLetDown".TranslateSimple);
                        }
                        return Add(-expectations, "FreeWillPriorityExpectionsIgnored".TranslateSimple);
                } // switch
            }
            catch (Exception err)
            {
                Log.ErrorOnce("could not consider beauty: " + "this consideration will be disabled in the mod settings to avoid future errors: " + err.ToString(), 228652891);
                worldComp.Settings.ConsiderBeauty = 0.0f;
                return this;
            }
        }

        private Priority ConsiderRelevantSkills(bool shouldAdd = false)
        {
            float _badSkillCutoff = Mathf.Min(3f, mapComp.NumPawns);
            float _goodSkillCutoff = _badSkillCutoff + ((20f - _badSkillCutoff) / 2f);
            float _greatSkillCutoff = _goodSkillCutoff + ((20f - _goodSkillCutoff) / 2f);
            float _excellentSkillCutoff = _greatSkillCutoff + ((20f - _greatSkillCutoff) / 2f);

            float _avg = pawn.skills.AverageOfRelevantSkillsFor(WorkTypeDef);
            return _avg >= _excellentSkillCutoff
                ? shouldAdd
                    ? Add(0.9f, () => string.Format("{0} {1:f0}", "FreeWillPrioritySkillLevel".TranslateSimple(), _avg))
                    : Set(0.9f, () => string.Format("{0} {1:f0}", "FreeWillPrioritySkillLevel".TranslateSimple(), _avg))
                : _avg >= _greatSkillCutoff
                ? shouldAdd
                    ? Add(0.7f, () => string.Format("{0} {1:f0}", "FreeWillPrioritySkillLevel".TranslateSimple(), _avg))
                    : Set(0.7f, () => string.Format("{0} {1:f0}", "FreeWillPrioritySkillLevel".TranslateSimple(), _avg))
                : _avg >= _goodSkillCutoff
                ? shouldAdd
                    ? Add(0.5f, () => string.Format("{0} {1:f0}", "FreeWillPrioritySkillLevel".TranslateSimple(), _avg))
                    : Set(0.5f, () => string.Format("{0} {1:f0}", "FreeWillPrioritySkillLevel".TranslateSimple(), _avg))
                : _avg >= _badSkillCutoff
                ? shouldAdd
                    ? Add(0.3f, () => string.Format("{0} {1:f0}", "FreeWillPrioritySkillLevel".TranslateSimple(), _avg))
                    : Set(0.3f, () => string.Format("{0} {1:f0}", "FreeWillPrioritySkillLevel".TranslateSimple(), _avg))
                : shouldAdd
                ? Add(0.1f, () => string.Format("{0} {1:f0}", "FreeWillPrioritySkillLevel".TranslateSimple(), _avg))
                : Set(0.1f, () => string.Format("{0} {1:f0}", "FreeWillPrioritySkillLevel".TranslateSimple(), _avg));
        }

        private bool NotInHomeArea(Pawn pawn)
        {
            return !this.pawn.Map.areaManager.Home[pawn.Position];
        }

        private static readonly Dictionary<string, Dictionary<BeautyCategory, float>> expectationGrid =
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
