using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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
                Set(0.2f, "FreeWillPriorityGlobalDefault".TranslateSimple);
                _ = InnerCompute();
                return;
            }
            catch (Exception e)
            {
                if (Prefs.DevMode)
                {
                    throw new Exception("could not compute " + WorkTypeDef.defName + " priority for pawn: " + e.Message);
                }
                Log.ErrorOnce("could not compute " + WorkTypeDef.defName + " priority for pawn: " + pawn.Name + ": " + e.Message, 15448413);

                _ = AlwaysDo("FreeWillPriorityError".TranslateSimple);
                Set(0.4f, "FreeWillPriorityError".TranslateSimple);
            }
        }

        int IComparable.CompareTo(object obj)
        {
            return obj == null ? 1 : !(obj is Priority p) ? 1 : Value.CompareTo(p.Value);
        }

        private Priority InnerCompute()
        {
            try
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
                        Set(0.0f, "FreeWillPriorityFirefightingDefault".TranslateSimple);
                        return
                            AlwaysDo("FreeWillPriorityFirefightingAlways".TranslateSimple)
                            .NeverDoIf(pawn.Downed, "FreeWillPriorityPawnDowned".TranslateSimple)
                            .ConsiderFire()
                            .ConsiderBuildingImmunity()
                            .ConsiderCompletingTask()
                            .ConsiderColonistsNeedingTreatment()
                            .ConsiderDownedColonists()
                            .ConsiderColonyPolicy()
                            ;

                    case PATIENT:
                        Set(0.0f, "FreeWillPriorityPatientDefault".TranslateSimple);
                        return
                            AlwaysDo("FreeWillPriorityPatientAlways".TranslateSimple)
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
                        Set(0.0f, "FreeWillPriorityBedrestDefault".TranslateSimple);
                        return
                            AlwaysDo("FreeWillPriorityBedrestAlways".TranslateSimple)
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
                        Set(0.5f, "FreeWillPriorityChildcareDefault".TranslateSimple);
                        return
                            ConsiderRelevantSkills(shouldAdd: true)
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
                        Set(0.5f, "FreeWillPriorityBasicWorkDefault".TranslateSimple);
                        return
                            ConsiderThoughts()
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
                        Set(0.3f, "FreeWillPriorityHaulingDefault".TranslateSimple);
                        return
                            ConsiderBeautyExpectations()
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
                        Set(0.5f, "FreeWillPriorityCleaningDefault".TranslateSimple);
                        return
                            ConsiderBeautyExpectations()
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
                        Set(0.5f, "FreeWillPriorityUrgentHaulingDefault".TranslateSimple);
                        return
                            ConsiderBeautyExpectations()
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
            catch (Exception e)
            {
                throw new Exception("could not run inner compute: " + e.Message);
            }
        }

        public void ApplyPriorityToGame()
        {
            try
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
            catch (Exception e)
            {
                throw new Exception("could not apply priority to game: " + e.Message);
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
                throw new Exception("could not convert to game priority: " + e.Message);
            }
        }

        private void Set(float x, Func<string> description)
        {
            try
            {
                Value = Mathf.Clamp01(x);
                float adjustmentValue = Value;
                string adjustmentString()
                {
                    return new StringBuilder()
                        .Append(" - ")
                        .Append(description().CapitalizeFirst())
                        .Append(": ")
                        .Append(adjustmentValue.ToStringPercent())
                        .ToString();
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
            }
            catch (Exception e)
            {
                throw new Exception("could not set priority: " + e.Message);
            }
        }

        private void Add(float x, Func<string> description)
        {
            try
            {
                if (Disabled)
                {
                    return;
                }
                float newValue = Mathf.Clamp01(Value + x);
                if (newValue > Value)
                {
                    float adjustmentValue = newValue - Value;
                    string adjustmentString()
                    {
                        return new StringBuilder()
                            .Append(" - ")
                            .Append(description().CapitalizeFirst())
                            .Append(": +")
                            .Append(adjustmentValue.ToStringPercent())
                            .ToString();
                    }
                    AdjustmentStrings.Add(adjustmentString);
                    Value = newValue;
                }
                else if (newValue < Value)
                {
                    float adjustmentValue = Value - newValue;
                    string adjustmentString()
                    {
                        return new StringBuilder()
                            .Append(" - ")
                            .Append(description().CapitalizeFirst())
                            .Append(": -")
                            .Append(adjustmentValue.ToStringPercent())
                            .ToString();
                    }
                    AdjustmentStrings.Add(adjustmentString);
                    Value = newValue;
                }
                else if (newValue == Value && Prefs.DevMode)
                {
                    float adjustmentValue = newValue - Value;
                    string adjustmentString()
                    {
                        return new StringBuilder()
                            .Append(" - ")
                            .Append(description().CapitalizeFirst())
                            .Append(": +")
                            .Append(adjustmentValue.ToStringPercent())
                            .ToString();
                    }
                    AdjustmentStrings.Add(adjustmentString);
                    Value = newValue;
                }
                return;
            }
            catch (Exception e)
            {
                throw new Exception("could not add to priority: " + e.Message);
            }
        }

        private Priority Multiply(float x, Func<string> description)
        {
            try
            {
                if (Disabled || Value == 0.0f)
                {
                    return this;
                }
                float newClampedValue = Mathf.Clamp01(Value * x);
                if (newClampedValue != Value)
                {
                    float adjustmentValue = newClampedValue / Value;
                    string adjustmentString()
                    {
                        return new StringBuilder()
                            .Append(" - ")
                            .Append(description().CapitalizeFirst())
                            .Append(": x")
                            .Append(adjustmentValue.ToStringPercent())
                            .ToString();
                    }
                    AdjustmentStrings.Add(adjustmentString);
                    Value = newClampedValue;
                }
                else if (newClampedValue == Value && Prefs.DevMode)
                {
                    float adjustmentValue = newClampedValue / Value;
                    string adjustmentString()
                    {
                        return new StringBuilder()
                            .Append(" - ")
                            .Append(description().CapitalizeFirst())
                            .Append(": x")
                            .Append(adjustmentValue.ToStringPercent())
                            .ToString();
                    }
                    AdjustmentStrings.Add(adjustmentString);
                    Value = newClampedValue;
                }
                return this;
            }
            catch (Exception e)
            {
                throw new Exception("could not multiply priority: " + e.Message);
            }
        }

        private Priority AlwaysDoIf(bool cond, Func<string> description)
        {
            try
            {
                if (!cond || Enabled)
                {
                    return this;
                }
                if (Prefs.DevMode || Disabled || ToGamePriority() == 0)
                {
                    string adjustmentString()
                    {
                        return new StringBuilder()
                            .Append(" - ")
                            .Append(description().CapitalizeFirst())
                            .Append(": ")
                            .Append("FreeWillPriorityEnabled".TranslateSimple().CapitalizeFirst())
                            .ToString();
                    }
                    AdjustmentStrings.Add(adjustmentString);
                }
                Enabled = true;
                Disabled = false;
                return this;
            }
            catch (Exception e)
            {
                throw new Exception("could not always do if: " + e.Message);
            }
        }

        private Priority AlwaysDo(Func<string> description)
        {
            try
            {
                return AlwaysDoIf(true, description);
            }
            catch (Exception e)
            {
                throw new Exception("could not always do: " + e.Message);
            }
        }

        private Priority NeverDoIf(bool cond, Func<string> description)
        {
            try
            {
                if (!cond || Disabled)
                {
                    return this;
                }
                if (Prefs.DevMode || Enabled || ToGamePriority() >= 0)
                {
                    string adjustmentString()
                    {
                        return new StringBuilder()
                            .Append(" - ")
                            .Append(description().CapitalizeFirst())
                            .Append(": ")
                            .Append("FreeWillPriorityDisabled".TranslateSimple().CapitalizeFirst())
                            .ToString();
                    }
                    AdjustmentStrings.Add(adjustmentString);
                }
                Disabled = true;
                Enabled = false;
                return this;
            }
            catch (Exception e)
            {
                throw new Exception("could not never do if: " + e.Message);
            }
        }

        private Priority NeverDo(Func<string> description)
        {
            try
            {
                return NeverDoIf(true, description);
            }
            catch (Exception e)
            {
                throw new Exception("could not never do: " + e.Message);
            }
        }

        private Priority ConsiderInspiration()
        {
            try
            {
                if (!pawn.mindState.inspirationHandler.Inspired)
                {
                    return this;
                }
                Inspiration inspiration = pawn.mindState.inspirationHandler.CurState;
                if (WorkTypeDef.defName == HUNTING && inspiration.def.defName == "Frenzy_Shoot")
                {
                    Add(0.4f, "FreeWillPriorityInspired".TranslateSimple);
                    return this;
                }
                foreach (WorkTypeDef workTypeDefB in inspiration?.def?.requiredNonDisabledWorkTypes ?? new List<WorkTypeDef>())
                {
                    if (WorkTypeDef.defName == workTypeDefB.defName)
                    {
                        Add(0.4f, "FreeWillPriorityInspired".TranslateSimple);
                        return this;
                    }
                }
                foreach (WorkTypeDef workTypeDefB in inspiration?.def?.requiredAnyNonDisabledWorkType ?? new List<WorkTypeDef>())
                {
                    if (WorkTypeDef.defName == workTypeDefB.defName)
                    {
                        Add(0.4f, "FreeWillPriorityInspired".TranslateSimple);
                        return this;
                    }
                }
                return this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider inspiration: " + e.Message);
            }
        }

        private Priority ConsiderThoughts()
        {
            try
            {
                mapComp = mapComp ?? pawn.Map.GetComponent<FreeWill_MapComponent>();
                foreach (Thought thought in mapComp.AllThoughts)
                {
                    if (thought.def.defName == "NeedFood")
                    {
                        if (WorkTypeDef.defName == COOKING)
                        {
                            Add(-0.01f * GetMoodEffect(thought), "FreeWillPriorityHungerLevel".TranslateSimple);
                            return this;
                        }
                        if (WorkTypeDef.defName == HUNTING || WorkTypeDef.defName == PLANT_CUTTING)
                        {
                            Add(-0.005f * GetMoodEffect(thought), "FreeWillPriorityHungerLevel".TranslateSimple);
                            return this;
                        }
                        Add(0.005f * GetMoodEffect(thought), "FreeWillPriorityHungerLevel".TranslateSimple);
                        return this;
                    }
                }
                return this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider thoughts: " + e);
            }
        }

        private static float GetMoodEffect(Thought thought)
        {
            float moodEffect;

            // This can produce an ArgumentOutOfRangeException if the thought is
            // modified during access. So, in case of an exception, we return
            // 0.0f.
            try
            {
                ThoughtStage curStage = thought.CurStage;
                moodEffect = curStage.baseMoodEffect;
            }
            catch (ArgumentOutOfRangeException)
            {
                moodEffect = 0.0f;
            }
            return moodEffect;
        }

        private Priority ConsiderNeedingWarmClothes()
        {
            try
            {
                if (mapComp.NeedWarmClothes)
                {
                    Add(0.2f, "FreeWillPriorityNeedWarmClothes".TranslateSimple);
                    return this;
                }
                return this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider needing warm clothes: " + e.Message);
            }
        }

        private Priority ConsiderAnimalsRoaming()
        {
            try
            {
                if (mapComp.AlertAnimalRoaming)
                {
                    Add(0.4f, "FreeWillPriorityAnimalsRoaming".TranslateSimple);
                    return this;
                }
                return this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider animals roaming: " + e.Message);
            }
        }

        private Priority ConsiderSuppressionNeed()
        {
            try
            {
                if (mapComp.SuppressionNeed != 0.0f)
                {
                    Add(mapComp.SuppressionNeed, "FreeWillPrioritySuppressionNeed".TranslateSimple);
                    return this;
                }
                return this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider suppression need: " + e.Message);
            }
        }

        private Priority ConsiderColonistLeftUnburied()
        {
            try
            {
                if (mapComp.AlertColonistLeftUnburied && (WorkTypeDef.defName == HAULING || WorkTypeDef.defName == HAULING_URGENT))
                {
                    Add(0.4f, "FreeWillPriorityColonistLeftUnburied".TranslateSimple);
                    return this;
                }
                return this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider colonist left unburied: " + e.Message);
            }
        }

        private Priority ConsiderBored()
        {
            try
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
            catch (Exception e)
            {
                throw new Exception("could not consider bored: " + e.Message);
            }
        }

        private Priority ConsiderHasHuntingWeapon()
        {
            try
            {
                return worldComp.Settings.ConsiderHasHuntingWeapon
                    ? NeverDoIf(!WorkGiver_HunterHunt.HasHuntingWeapon(pawn), "FreeWillPriorityNoHuntingWeapon".TranslateSimple)
                    : this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider has hunting weapon: " + e.Message);
            }
        }

        private Priority ConsiderBrawlersNotHunting()
        {
            try
            {
                return worldComp.Settings.ConsiderBrawlersNotHunting && WorkTypeDef.defName == HUNTING
                    ? NeverDoIf(pawn.story.traits.HasTrait(DefDatabase<TraitDef>.GetNamed("Brawler")), "FreeWillPriorityBrawler".TranslateSimple)
                    : this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider brawlers not hunting: " + e.Message);
            }
        }

        private Priority ConsiderCompletingTask()
        {
            try
            {
                // pawns prefer the work they are current doing
                return pawn.CurJob?.workGiverDef?.workType == WorkTypeDef
                    ? AlwaysDo("FreeWillPriorityCurrentlyDoing".TranslateSimple)
                        .Multiply(1.8f, "FreeWillPriorityCurrentlyDoing".TranslateSimple)
                    : this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider completing task: " + e.Message);
            }
        }

        private Priority ConsiderMovementSpeed()
        {
            try
            {
                return worldComp.Settings.ConsiderMovementSpeed != 0.0f
                    ? Multiply(worldComp.Settings.ConsiderMovementSpeed * 0.25f * pawn.GetStatValue(StatDefOf.MoveSpeed, true), "FreeWillPriorityMovementSpeed".TranslateSimple)
                    : this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider movement speed: " + e.Message);
            }
        }

        private Priority ConsiderCarryingCapacity()
        {
            try
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
            catch (Exception e)
            {
                throw new Exception("could not consider carrying capacity: " + e.Message);
            }
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

                            _ = AlwaysDo(() => "FreeWillPriorityMajorPassionFor".Translate(relevantSkills[index].skillLabel));
                            Add(x, () => "FreeWillPriorityMajorPassionFor".Translate(relevantSkills[index].skillLabel));
                            continue;
                        case Passion.Minor:
                            x = worldComp.Settings.ConsiderPassions * pawn.needs.mood.CurLevel * 0.25f / relevantSkills.Count;

                            _ = AlwaysDo(() => "FreeWillPriorityMinorPassionFor".Translate(relevantSkills[index].skillLabel));
                            Add(x, () => "FreeWillPriorityMinorPassionFor".Translate(relevantSkills[index].skillLabel));
                            continue;
                        default:
                            continue;
                    }
                }
                return this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider passion: " + e.Message);
            }
        }

        private Priority ConsiderFinishedMechGestators()
        {
            try
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
                        Add(0.4f, "FreeWillPriorityMechGestator".TranslateSimple);
                        return this;
                    }
                }
                return this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider finished mech gestators: " + e.Message);
            }
        }

        private Priority ConsiderDownedColonists()
        {
            try
            {
                if (pawn.Downed)
                {
                    if (WorkTypeDef.defName != PATIENT && WorkTypeDef.defName != PATIENT_BED_REST)
                    {
                        _ = NeverDo("FreeWillPriorityPawnDowned".TranslateSimple);
                        return this;
                    }
                    else
                    {
                        _ = AlwaysDo("FreeWillPriorityPawnDowned".TranslateSimple);
                        Set(1.0f, "FreeWillPriorityPawnDowned".TranslateSimple);
                        return this;
                    }
                }
                if (mapComp.PercentPawnsDowned <= 0.0f)
                {
                    return this;
                }
                if (WorkTypeDef.defName == DOCTOR)
                {
                    Add(mapComp.PercentPawnsDowned, "FreeWillPriorityOtherPawnsDowned".TranslateSimple);
                    return this;
                }
                if (WorkTypeDef.defName == SMITHING ||
                    WorkTypeDef.defName == TAILORING ||
                    WorkTypeDef.defName == ART ||
                    WorkTypeDef.defName == CRAFTING ||
                    WorkTypeDef.defName == RESEARCHING
                    )
                {
                    _ = NeverDo("FreeWillPriorityOtherPawnsDowned".TranslateSimple);
                    return this;
                }
                return this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider downed colonists: " + e.Message);
            }
        }

        private Priority ConsiderColonyPolicy()
        {
            try
            {
                if (!worldComp.Settings.globalWorkAdjustments.ContainsKey(WorkTypeDef.defName))
                {
                    worldComp.Settings.globalWorkAdjustments.Add(WorkTypeDef.defName, 0.0f);
                }
                float adj = worldComp.Settings.globalWorkAdjustments[WorkTypeDef.defName];
                Add(adj, "FreeWillPriorityColonyPolicy".TranslateSimple);
                return this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider colony policy: " + e.Message);
            }
        }

        private Priority ConsiderRefueling()
        {
            try
            {
                if (mapComp.RefuelNeededNow)
                {
                    Add(0.35f, "FreeWillPriorityRefueling".TranslateSimple);
                    return this;
                }
                if (mapComp.RefuelNeeded)
                {
                    Add(0.20f, "FreeWillPriorityRefueling".TranslateSimple);
                    return this;
                }
                return this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider refueling: " + e.Message);
            }
        }

        private Priority ConsiderFire()
        {
            try
            {
                if (WorkTypeDef.defName != FIREFIGHTER)
                {
                    if (mapComp.HomeFire)
                    {
                        Add(-0.2f, "FreeWillPriorityFireInHomeArea".TranslateSimple);
                        return this;
                    }
                    if (mapComp.MapFires > 0 && WorkTypeDef.defName == FIREFIGHTER)
                    {
                        Add(Mathf.Clamp01(mapComp.MapFires * 0.01f), "FreeWillPriorityFireOnMap".TranslateSimple);
                        return this;
                    }
                    return this;
                }
                if (mapComp.HomeFire)
                {
                    Set(1.0f, "FreeWillPriorityFireInHomeArea".TranslateSimple);
                    return this;
                }
                if (mapComp.MapFires > 0 && WorkTypeDef.defName == FIREFIGHTER)
                {
                    Add(Mathf.Clamp01(mapComp.MapFires * 0.01f), "FreeWillPriorityFireOnMap".TranslateSimple);
                    return this;
                }
                return this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider fire: " + e.Message);
            }
        }

        private Priority ConsiderOperation()
        {
            try
            {
                if (HealthAIUtility.ShouldHaveSurgeryDoneNow(pawn))
                {
                    Set(1.0f, "FreeWillPriorityOperation".TranslateSimple);
                    return this;
                }
                else
                {
                    return this;
                }
            }
            catch (Exception e)
            {
                throw new Exception("could not consider operation: " + e.Message);
            }
        }

        private Priority ConsiderBuildingImmunity()
        {
            try
            {
                if (pawn.health.hediffSet.HasImmunizableNotImmuneHediff())
                {
                    if (WorkTypeDef.defName == PATIENT_BED_REST)
                    {
                        Add(0.4f, "FreeWillPriorityBuildingImmunity".TranslateSimple);
                        return this;
                    }
                    if (WorkTypeDef.defName != PATIENT)
                    {
                        Add(-0.2f, "FreeWillPriorityBuildingImmunity".TranslateSimple);
                        return this;
                    }
                    return this;
                }
                return this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider building immunity: " + e.Message);
            }
        }

        private Priority ConsiderColonistsNeedingTreatment()
        {
            try
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
            catch (Exception e)
            {
                throw new Exception("could not consider colonists needing treatment: " + e.Message);
            }
        }

        private Priority ConsiderThisPawnNeedsTreatment()
        {

            try
            {
                if (WorkTypeDef.defName == PATIENT || WorkTypeDef.defName == PATIENT_BED_REST)
                {
                    // patient and bed rest are activated and set to 100%
                    _ = AlwaysDo("FreeWillPriorityNeedTreatment".TranslateSimple);
                    Set(1.0f, "FreeWillPriorityNeedTreatment".TranslateSimple);
                    return this;
                }
                if (WorkTypeDef.defName == DOCTOR)
                {
                    if (pawn.playerSettings.selfTend)
                    {
                        // this pawn can self tend, so activate doctor skill and set
                        // to 100%
                        _ = AlwaysDo("FreeWillPriorityNeedTreatmentSelfTend".TranslateSimple);
                        Set(1.0f, "FreeWillPriorityNeedTreatmentSelfTend".TranslateSimple);
                        return this;
                    }
                    // doctoring stays the same
                    return this;
                }
                // don't do other work types
                return NeverDo("FreeWillPriorityNeedTreatment".TranslateSimple);
            }
            catch (Exception e)
            {
                throw new Exception("could not consider this pawn needs treatment: " + e.Message);
            }
        }

        private Priority ConsiderAnotherPawnNeedsTreatment()
        {
            try
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
                    Add(mapComp.PercentPawnsNeedingTreatment, "FreeWillPriorityOthersNeedTreatment".TranslateSimple);
                    return this;
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
                    if (Value > 0.3f)
                    {
                        // crafting work types are low priority when someone is injured
                        Add(-(Value - 0.3f), "FreeWillPriorityOthersNeedTreatment".TranslateSimple);
                        return this;
                    }
                    // crafting work types are low priority when someone is injured
                    return this;
                }

                // any other work type is capped at 0.6
                if (Value > 0.6f)
                {
                    Add(-(Value - 0.6f), "FreeWillPriorityOthersNeedTreatment".TranslateSimple);
                    return this;
                }
                return this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider another pawn needs treatment: " + e.Message);
            }
        }

        private Priority ConsiderHealth()
        {
            try
            {
                if (WorkTypeDef.defName == PATIENT || WorkTypeDef.defName == PATIENT_BED_REST)
                {
                    Add(1 - Mathf.Pow(pawn.health.summaryHealth.SummaryHealthPercent, 7.0f), "FreeWillPriorityHealth".TranslateSimple);
                    return this;
                }
                return Multiply(pawn.health.summaryHealth.SummaryHealthPercent, "FreeWillPriorityHealth".TranslateSimple);
            }
            catch (Exception e)
            {
                throw new Exception("could not consider health: " + e.Message);
            }
        }

        private Priority ConsiderFoodPoisoning()
        {
            try
            {
                if (worldComp.Settings.ConsiderFoodPoisoning == 0.0f)
                {
                    return this;
                }
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
                            Add(adjustment, "FreeWillPriorityFilthyCookingArea".TranslateSimple);
                            return this;
                        }
                        if (WorkTypeDef.defName == COOKING)
                        {
                            Add(-adjustment, "FreeWillPriorityFilthyCookingArea".TranslateSimple);
                            return this;
                        }
                    }
                }
                return this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider food poisoning: " + e.Message);
            }
        }

        private Priority ConsiderOwnRoom()
        {
            try
            {
                if (worldComp.Settings.ConsiderOwnRoom == 0.0f)
                {
                    return this;
                }
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
            catch (Exception e)
            {
                throw new Exception("could not consider own room: " + e.Message);
            }
        }

        private Priority ConsiderRepairingMech()
        {
            try
            {
                if (mapComp.AlertMechDamaged)
                {
                    if (MechanitorUtility.IsMechanitor(pawn))
                    {
                        Add(0.6f, "FreeWillPriorityMechanoidDamaged".TranslateSimple);
                        return this;
                    }
                    return this;
                }
                return this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider repairing mech: " + e.Message);
            }
        }

        private Priority ConsiderIsAnyoneElseDoing()
        {
            try
            {
                if (pawn?.Map?.mapPawns == null)
                {
                    throw new Exception("map pawns is null");
                }

                foreach (Pawn other in pawn.Map.mapPawns.PawnsInFaction(Faction.OfPlayer))
                {
                    if (IsDoing(other))
                    {
                        return this;
                    }
                }

                return AlwaysDo("FreeWillPriorityNoOneElseDoing".TranslateSimple);
            }
            catch (Exception e)
            {
                throw new Exception("could not consider if anyone else is doing: " + e.Message);
            }
        }

        private bool IsDoing(Pawn other)
        {
            try
            {
                if (other == null || other == pawn)
                {
                    return false;
                }
                if (!other.IsColonistPlayerControlled && !other.IsColonyMechPlayerControlled)
                {
                    return false;
                }
                if (!other.Awake() || other.Downed || other.Dead || other.IsCharging())
                {
                    return false;
                }

                Pawn_WorkSettings workSettings = other.workSettings ?? throw new Exception("could not get work settings");
                int priority = workSettings.GetPriority(WorkTypeDef);
                bool isOtherPawnDoing = priority != 0;
                if (isOtherPawnDoing)
                {
                    return true;
                }

                RaceProperties raceProps = other.RaceProps ?? throw new Exception("could not get race props");
                List<WorkTypeDef> mechEnabledWorkTypes = raceProps.mechEnabledWorkTypes ?? throw new Exception("could not get mech enabled work types");
                bool isMechDoing = mechEnabledWorkTypes.Contains(WorkTypeDef);

                return isMechDoing;
            }
            catch (Exception e)
            {
                throw new Exception("could not determine if other is doing: " + e.Message);
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

                MapPawns mapPawns = (pawn?.Map?.mapPawns) ?? throw new Exception("map pawns is null");
                List<Pawn> allPawns = mapPawns.PawnsInFaction(Faction.OfPlayer);
                if (allPawns.Count() <= 1)
                {
                    return this;
                }
                bool isBestAtDoing = true;
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
                    if (skillDiff <= 0.0f)
                    {
                        continue;
                    }
                    // not the best
                    isBestAtDoing = false;
                    bool isDoing = other.CurJob != null && other.CurJob.workGiverDef != null && other.CurJob.workGiverDef.workType == WorkTypeDef;
                    bool isMuchBetter = skillDiff >= 5.0f;
                    bool isMuchMuchBetter = skillDiff >= 10.0f;
                    bool isMuchMuchMuchBetter = skillDiff >= 15.0f;
                    if (isDoing)
                    {
                        if (isMuchMuchMuchBetter)
                        {
                            Add(1.5f * -impactPerPawn, () => "FreeWillPrioritySomeoneMuchMuchMuchBetterIsDoing".Translate(other.LabelShortCap));
                        }
                        else if (isMuchMuchBetter)
                        {
                            Add(1.5f * -impactPerPawn * 0.8f, () => "FreeWillPrioritySomeoneMuchMuchBetterIsDoing".Translate(other.LabelShortCap));
                        }
                        else if (isMuchBetter)
                        {
                            Add(1.5f * -impactPerPawn * 0.6f, () => "FreeWillPrioritySomeoneMuchBetterIsDoing".Translate(other.LabelShortCap));
                        }
                        else
                        {
                            Add(1.5f * -impactPerPawn * 0.4f, () => "FreeWillPrioritySomeoneBetterIsDoing".Translate(other.LabelShortCap));
                        }
                    }
                    else
                    {
                        if (isMuchMuchMuchBetter)
                        {
                            Add(-impactPerPawn, () => "FreeWillPrioritySomeoneMuchMuchMuchBetterAtDoing".Translate(other.LabelShortCap));
                        }
                        else if (isMuchMuchBetter)
                        {
                            Add(-impactPerPawn * 0.8f, () => "FreeWillPrioritySomeoneMuchMuchBetterAtDoing".Translate(other.LabelShortCap));
                        }
                        else if (isMuchBetter)
                        {
                            Add(-impactPerPawn * 0.6f, () => "FreeWillPrioritySomeoneMuchBetterAtDoing".Translate(other.LabelShortCap));
                        }
                        else
                        {
                            Add(-impactPerPawn * 0.4f, () => "FreeWillPrioritySomeoneBetterAtDoing".Translate(other.LabelShortCap));
                        }
                    }
                }
                return isBestAtDoing ? Multiply(1.5f * worldComp.Settings.ConsiderBestAtDoing, "FreeWillPriorityBestAtDoing".TranslateSimple) : this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider best at doing: " + e.Message);
            }
        }

        private Priority ConsiderInjuredPets()
        {
            try
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
                Add(Mathf.Clamp01(numPetsNeedingTreatment / n) * 0.5f, "FreeWillPriorityPetsInjured".TranslateSimple);
                return this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider injured pets: " + e.Message);
            }
        }

        private Priority ConsiderMechHaulers()
        {
            try
            {
                float percentPawnsMechHaulers = mapComp.PercentPawnsMechHaulers;
                Add(-percentPawnsMechHaulers, "FreeWillPriorityMechHaulers".TranslateSimple);
                return this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider mech haulers: " + e.Message);
            }
        }

        private Priority ConsiderInjuredPrisoners()
        {
            try
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
                Add(Mathf.Clamp01(numPrisonersNeedingTreatment / n) * 0.5f, "FreeWillPriorityPrisonersInjured".TranslateSimple);
                return this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider injured prisoners: " + e.Message);
            }
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
                if ((WorkTypeDef.defName != HAULING && WorkTypeDef.defName != HAULING_URGENT) || mapComp.ThingsDeteriorating != null)
                {
                    Add(adjustment * worldComp.Settings.ConsiderLowFood, "FreeWillPriorityLowFood".TranslateSimple);
                    return this;
                }
                return this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider low food: " + e.Message);
            }
        }

        private Priority ConsiderWeaponRange()
        {
            try
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
            catch (Exception e)
            {
                throw new Exception("could not consider weapon range: " + e.Message);
            }
        }

        private Priority ConsiderAteRawFood()
        {
            try
            {
                if (WorkTypeDef.defName != COOKING)
                {
                    return this;
                }

                foreach (Thought thought in mapComp.AllThoughts)
                {
                    if (thought.def.defName == "AteRawFood" && 0.6f > Value)
                    {
                        Set(0.6f, "FreeWillPriorityAteRawFood".TranslateSimple);
                        return this;
                    }
                }
                return this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider ate raw food: " + e.Message);
            }
        }

        private Priority ConsiderThingsDeteriorating()
        {
            try
            {
                if (WorkTypeDef.defName == HAULING || WorkTypeDef.defName == HAULING_URGENT)
                {
                    if (mapComp.ThingsDeteriorating != null)
                    {
                        if (Prefs.DevMode)
                        {
                            string name = mapComp.ThingsDeteriorating.def.defName;
                            string adjustmentString()
                            {
                                return new StringBuilder()
                                    .Append("FreeWillPriorityThingsDeteriorating".TranslateSimple())
                                    .Append(": ")
                                    .Append(name)
                                    .ToString();
                            }
                            return Multiply(2.0f, adjustmentString);
                        }
                        return Multiply(2.0f, "FreeWillPriorityThingsDeteriorating".TranslateSimple);
                    }
                }
                return this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider things deteriorating: " + e.Message);
            }
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
                    Add(0.4f * worldComp.Settings.ConsiderPlantsBlighted, "FreeWillPriorityBlight".TranslateSimple);
                    return this;
                }
            }
            catch (Exception e)
            {
                throw new Exception("could not consider plants blighted: " + e.Message);
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
            catch (Exception e)
            {
                throw new Exception("could not consider gauranlen pruning: " + e.Message);
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
                            Add(expectations, "FreeWillPriorityExpectionsExceeded".TranslateSimple);
                            return this;
                        }
                        if (expectations < 0.4f)
                        {
                            Add(expectations, "FreeWillPriorityExpectionsMet".TranslateSimple);
                            return this;
                        }
                        if (expectations < 0.6f)
                        {
                            Add(expectations, "FreeWillPriorityExpectionsUnmet".TranslateSimple);
                            return this;
                        }
                        if (expectations < 0.8f)
                        {
                            Add(expectations, "FreeWillPriorityExpectionsLetDown".TranslateSimple);
                            return this;
                        }
                        Add(expectations, "FreeWillPriorityExpectionsIgnored".TranslateSimple);
                        return this;
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
                            Add(expectations - ADJUSTMENT, "FreeWillPriorityExpectionsExceeded".TranslateSimple);
                            return this;
                        }
                        if (expectations < 0.4f)
                        {
                            Add(expectations - ADJUSTMENT, "FreeWillPriorityExpectionsMet".TranslateSimple);
                            return this;
                        }
                        if (expectations < 0.6f)
                        {
                            Add(expectations - ADJUSTMENT, "FreeWillPriorityExpectionsUnmet".TranslateSimple);
                            return this;
                        }
                        if (expectations < 0.8f)
                        {
                            Add(expectations - ADJUSTMENT, "FreeWillPriorityExpectionsLetDown".TranslateSimple);
                            return this;
                        }
                        Add(expectations - ADJUSTMENT, "FreeWillPriorityExpectionsIgnored".TranslateSimple);
                        return this;
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
                            Add(-expectations, "FreeWillPriorityExpectionsExceeded".TranslateSimple);
                            return this;
                        }
                        if (expectations < 0.4f)
                        {
                            Add(-expectations, "FreeWillPriorityExpectionsMet".TranslateSimple);
                            return this;
                        }
                        if (expectations < 0.6f)
                        {
                            Add(-expectations, "FreeWillPriorityExpectionsUnmet".TranslateSimple);
                            return this;
                        }
                        if (expectations < 0.8f)
                        {
                            Add(-expectations, "FreeWillPriorityExpectionsLetDown".TranslateSimple);
                            return this;
                        }
                        Add(-expectations, "FreeWillPriorityExpectionsIgnored".TranslateSimple);
                        return this;
                } // switch
            }
            catch (Exception e)
            {
                throw new Exception("could not consider beauty expectations: " + e.Message);
            }
        }

        private Priority ConsiderRelevantSkills(bool shouldAdd = false)
        {
            try
            {
                float badSkillCutoff = Mathf.Min(3f, mapComp.NumPawns);
                float goodSkillCutoff = badSkillCutoff + ((20f - badSkillCutoff) / 2f);
                float greatSkillCutoff = goodSkillCutoff + ((20f - goodSkillCutoff) / 2f);
                float excellentSkillCutoff = greatSkillCutoff + ((20f - greatSkillCutoff) / 2f);

                float averageSkill = pawn.skills.AverageOfRelevantSkillsFor(WorkTypeDef);
                string description()
                {
                    return string.Format("{0} {1:f0}", "FreeWillPrioritySkillLevel".TranslateSimple(), averageSkill);
                }

                if (shouldAdd)
                {
                    if (averageSkill >= excellentSkillCutoff)
                    {
                        Add(0.9f, description);
                        return this;
                    }
                    if (averageSkill >= greatSkillCutoff)
                    {
                        Add(0.7f, description);
                        return this;
                    }
                    if (averageSkill >= goodSkillCutoff)
                    {
                        Add(0.5f, description);
                        return this;
                    }
                    if (averageSkill >= badSkillCutoff)
                    {
                        Add(0.3f, description);
                        return this;
                    }
                    Add(0.1f, description);
                    return this;
                }

                // if not adding, just set the priority
                if (averageSkill >= excellentSkillCutoff)
                {
                    Set(0.9f, description);
                    return this;
                }
                if (averageSkill >= greatSkillCutoff)
                {
                    Set(0.7f, description);
                    return this;
                }
                if (averageSkill >= goodSkillCutoff)
                {
                    Set(0.5f, description);
                    return this;
                }
                if (averageSkill >= badSkillCutoff)
                {
                    Set(0.3f, description);
                    return this;
                }
                Set(0.1f, description);
                return this;
            }
            catch (Exception e)
            {
                throw new Exception("could not consider relevant skills: " + e.Message);
            }
        }

        private bool NotInHomeArea(Pawn pawn)
        {
            try
            {
                return !this.pawn.Map.areaManager.Home[pawn.Position];
            }
            catch (Exception e)
            {
                throw new Exception("could not determine if pawn is not in home area: " + e.Message);
            }
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
