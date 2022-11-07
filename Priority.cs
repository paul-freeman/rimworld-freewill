using System;
using System.Text;
using System.Collections.Generic;
using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;


namespace FreeWill
{
    public class Priority : IComparable
    {
        const int disabledCutoff = 100 / (Pawn_WorkSettings.LowestPriority + 1); // 20 if LowestPriority is 4
        const int disabledCutoffActiveWorkArea = 100 - disabledCutoff; // 80 if LowestPriority is 4
        const float onePriorityWidth = (float)disabledCutoffActiveWorkArea / (float)Pawn_WorkSettings.LowestPriority; // ~20 if LowestPriority is 4
        private FreeWill_WorldComponent worldComp;

        private Pawn pawn;
        private FreeWill_MapComponent mapComp;
        private WorkTypeDef workTypeDef;
        private float value;
        private List<string> adjustmentStrings;
        private bool enabled;
        private bool disabled;

        // work types
        const string FIREFIGHTER = "Firefighter";
        const string PATIENT = "Patient";
        const string DOCTOR = "Doctor";
        const string PATIENT_BED_REST = "PatientBedRest";
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

        public Priority(Pawn pawn, WorkTypeDef workTypeDef)
        {
            try
            {
                this.pawn = pawn;
                this.workTypeDef = workTypeDef;
                this.adjustmentStrings = new List<string> { };

                mapComp = this.pawn.Map.GetComponent<FreeWill_MapComponent>();
                worldComp = Find.World.GetComponent<FreeWill_WorldComponent>();

                // pawn has no free will, so use the player set priority
                if (!worldComp.HasFreeWill(pawn))
                {
                    var p = pawn.workSettings.GetPriority(workTypeDef);
                    if (p == 0)
                    {
                        this.set(0.0f, "FreeWillPriorityNoFreeWill".TranslateSimple());
                        return;
                    }
                    this.set((100f - onePriorityWidth * (p - 1)) / 100f, "FreeWillPriorityNoFreeWill".TranslateSimple());
                    return;
                }

                // start priority at the global default and compute the priority
                // using the AI in this file
                this.set(0.2f, "FreeWillPriorityGlobalDefault".TranslateSimple()).compute();
                return;
            }
            catch (System.Exception err)
            {
                Log.ErrorOnce("could not set " + workTypeDef.defName + " priority for pawn: " + pawn.Name + ": " + err.Message, 15448413);
                this
                    .alwaysDo("FreeWillPriorityError".TranslateSimple())
                    .set(0.4f, "FreeWillPriorityError".TranslateSimple())
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
            return this.value.CompareTo(p.value);
        }

        private Priority compute()
        {
            this.enabled = false;
            this.disabled = false;
            if (pawn.WorkTypeIsDisabled(workTypeDef))
            {
                return this.neverDo("FreeWillPriorityPermanentlyDisabled".TranslateSimple());
            }
            switch (this.workTypeDef.defName)
            {
                case FIREFIGHTER:
                    return this
                        .set(0.0f, "FreeWillPriorityFirefightingDefault".TranslateSimple())
                        .alwaysDo("FreeWillPriorityPatientDefault".TranslateSimple())
                        .neverDoIf(this.pawn.Downed, "FreeWillPriorityPawnDowned".TranslateSimple())
                        .considerFire()
                        .considerBuildingImmunity()
                        .considerCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .considerDownedColonists()
                        .considerColonyPolicy()
                        ;

                case PATIENT:
                    return this
                        .set(0.0f, "FreeWillPriorityPatientDefault".TranslateSimple())
                        .alwaysDo("FreeWillPriorityPatientDefault".TranslateSimple())
                        .considerHealth()
                        .considerBuildingImmunity()
                        .considerCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .considerDownedColonists()
                        .considerOperation()
                        .considerColonyPolicy()
                        ;

                case DOCTOR:
                    return this
                        .considerRelevantSkills()
                        .considerCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .considerPassion()
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
                        .considerCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .considerDownedColonists()
                        .considerColonyPolicy()
                        ;

                case PATIENT_BED_REST:
                    return this
                        .set(0.0f, "FreeWillPriorityBedrestDefault".TranslateSimple())
                        .alwaysDo("FreeWillPriorityBedrestDefault".TranslateSimple())
                        .considerHealth()
                        .considerBuildingImmunity()
                        .considerLowFood(-0.2f)
                        .considerCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .considerBored()
                        .considerDownedColonists()
                        .considerOperation()
                        .considerColonyPolicy()
                        ;

                case BASIC_WORKER:
                    return this
                        .set(0.5f, "FreeWillPriorityBasicWorkDefault".TranslateSimple())
                        .considerThoughts()
                        .considerHealth()
                        .considerLowFood(-0.3f)
                        .considerBored()
                        .neverDoIf(this.pawn.Downed, "FreeWillPriorityPawnDowned".TranslateSimple())
                        .considerBuildingImmunity()
                        .considerCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .considerDownedColonists()
                        .considerColonyPolicy()
                        ;

                case WARDEN:
                    return this
                        .considerRelevantSkills()
                        .considerCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .considerPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerLowFood(-0.3f)
                        .considerColonistLeftUnburied()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .considerCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .considerDownedColonists()
                        .considerColonyPolicy()
                        ;

                case HANDLING:
                    return this
                        .considerRelevantSkills()
                        .considerMovementSpeed()
                        .considerCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .considerPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerAnimalsRoaming()
                        .considerColonistLeftUnburied()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .considerCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .considerDownedColonists()
                        .considerColonyPolicy()
                        ;

                case COOKING:
                    return this
                        .considerRelevantSkills()
                        .considerCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .considerPassion()
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
                        .considerCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .considerDownedColonists()
                        .considerColonyPolicy()
                        ;

                case HUNTING:
                    return this
                        .considerRelevantSkills()
                        .considerMovementSpeed()
                        .considerCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .considerPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerLowFood(0.3f)
                        .considerWeaponRange()
                        .considerColonistLeftUnburied()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerBored()
                        .considerHasHuntingWeapon()
                        .considerBrawlersNotHunting()
                        .considerFire()
                        .considerBuildingImmunity()
                        .considerCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .considerDownedColonists()
                        .considerColonyPolicy()
                        ;

                case CONSTRUCTION:
                    return this
                        .considerRelevantSkills()
                        .considerCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .considerPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerLowFood(-0.3f)
                        .considerColonistLeftUnburied()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .considerCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .considerDownedColonists()
                        .considerColonyPolicy()
                        ;

                case GROWING:
                    return this
                        .considerRelevantSkills()
                        .considerCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .considerPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerLowFood(0.3f)
                        .considerColonistLeftUnburied()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .considerCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .considerDownedColonists()
                        .considerColonyPolicy()
                        ;

                case MINING:
                    return this
                        .considerRelevantSkills()
                        .considerCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .considerPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerLowFood(-0.3f)
                        .considerColonistLeftUnburied()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .considerCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .considerDownedColonists()
                        .considerColonyPolicy()
                        ;

                case PLANT_CUTTING:
                    return this
                        .considerRelevantSkills()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .considerPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerGauranlenPruning()
                        .considerLowFood(0.3f)
                        .considerHealth()
                        .considerPlantsBlighted()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .considerCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .considerDownedColonists()
                        .considerColonyPolicy()
                        ;

                case SMITHING:
                    return this
                        .considerRelevantSkills()
                        .considerCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .considerBeautyExpectations()
                        .considerPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerLowFood(-0.3f)
                        .considerColonistLeftUnburied()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .considerCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .considerDownedColonists()
                        .considerColonyPolicy()
                        ;

                case TAILORING:
                    return this
                        .considerRelevantSkills()
                        .considerCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .considerBeautyExpectations()
                        .considerPassion()
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
                        .considerCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .considerDownedColonists()
                        .considerColonyPolicy()
                        ;

                case ART:
                    return this
                        .considerRelevantSkills()
                        .considerCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .considerBeautyExpectations()
                        .considerPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerLowFood(-0.3f)
                        .considerColonistLeftUnburied()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .considerCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .considerDownedColonists()
                        .considerColonyPolicy()
                        ;

                case CRAFTING:
                    return this
                        .considerRelevantSkills()
                        .considerCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .considerBeautyExpectations()
                        .considerPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerLowFood(-0.3f)
                        .considerColonistLeftUnburied()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .considerCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .considerDownedColonists()
                        .considerColonyPolicy()
                        ;

                case HAULING:
                    return this
                        .set(0.3f, "FreeWillPriorityHaulingDefault".TranslateSimple())
                        .considerBeautyExpectations()
                        .considerMovementSpeed()
                        .considerCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .considerPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerRefueling()
                        .considerLowFood(0.2f)
                        .considerColonistLeftUnburied()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerThingsDeteriorating()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .considerCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .considerDownedColonists()
                        .considerColonyPolicy()
                        ;

                case CLEANING:
                    return this
                        .set(0.3f, "FreeWillPriorityCleaningDefault".TranslateSimple())
                        .considerBeautyExpectations()
                        .considerIsAnyoneElseDoing()
                        .considerThoughts()
                        .considerOwnRoom()
                        .considerLowFood(-0.2f)
                        .considerFoodPoisoning()
                        .considerHealth()
                        .considerBored()
                        .neverDoIf(notInHomeArea(this.pawn), "FreeWillPriorityNotInHomeArea".TranslateSimple())
                        .considerBuildingImmunity()
                        .considerCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .considerDownedColonists()
                        .considerColonyPolicy()
                        ;

                case RESEARCHING:
                    return this
                        .considerRelevantSkills()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .considerBeautyExpectations()
                        .considerPassion()
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
                        .considerDownedColonists()
                        .considerColonyPolicy()
                        ;

                case HAULING_URGENT:
                    return this
                        .set(0.5f, "FreeWillPriorityUrgentHaulingDefault".TranslateSimple())
                        .considerBeautyExpectations()
                        .considerMovementSpeed()
                        .considerCarryingCapacity()
                        .considerIsAnyoneElseDoing()
                        .considerPassion()
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
                        .considerCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .considerDownedColonists()
                        .considerColonyPolicy()
                        ;

                default:
                    return this
                        .considerRelevantSkills()
                        .considerMovementSpeed()
                        .considerCarryingCapacity()
                        .considerBeautyExpectations()
                        .considerIsAnyoneElseDoing()
                        .considerBestAtDoing()
                        .considerPassion()
                        .considerThoughts()
                        .considerInspiration()
                        .considerLowFood(-0.3f)
                        .considerColonistLeftUnburied()
                        .considerHealth()
                        .considerAteRawFood()
                        .considerBored()
                        .considerFire()
                        .considerBuildingImmunity()
                        .considerCompletingTask()
                        .considerColonistsNeedingTreatment()
                        .considerDownedColonists()
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
            pawn.workSettings.SetPriority(workTypeDef, this.ToGamePriority());
        }

        public string GetTip()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(workTypeDef.description);
            if (!this.disabled)
            {
                int p = this.ToGamePriority();
                string str = string.Format("Priority{0}", p).TranslateSimple();
                string text = str.Colorize(WidgetsWork.ColorOfPriority(p));
                stringBuilder.AppendLine(text);
                stringBuilder.AppendLine("------------------------------");
            }
            foreach (string adj in this.adjustmentStrings)
            {
                stringBuilder.AppendLine(adj);
            }
            return stringBuilder.ToString();
        }

        public int ToGamePriority()
        {
            int valueInt = UnityEngine.Mathf.Clamp(UnityEngine.Mathf.RoundToInt(this.value * 100), 0, 100);
            if (valueInt <= disabledCutoff)
            {
                if (this.enabled)
                {
                    return Pawn_WorkSettings.LowestPriority;
                }
                return 0;
            }
            if (this.disabled)
            {
                return 0;
            }
            int invertedValueRange = disabledCutoffActiveWorkArea - (valueInt - disabledCutoff); // 0-80 if LowestPriority is 4
            int gamePriorityValue = UnityEngine.Mathf.FloorToInt((float)invertedValueRange / onePriorityWidth) + 1;
            if (gamePriorityValue > Pawn_WorkSettings.LowestPriority || gamePriorityValue < 1)
            {
                Log.Error("calculated an invalid game priority value of " + gamePriorityValue.ToString());
                gamePriorityValue = UnityEngine.Mathf.Clamp(gamePriorityValue, 1, Pawn_WorkSettings.LowestPriority);
            }

            return gamePriorityValue;
        }

        private Priority set(float x, string s)
        {
            this.value = UnityEngine.Mathf.Clamp01(x);
            if (Prefs.DevMode)
            {
                this.adjustmentStrings.Add("-- reset --");
                this.adjustmentStrings.Add(string.Format("{0} ({1})", this.value.ToStringPercent(), s));
            }
            else
            {
                this.adjustmentStrings = new List<string> { string.Format("{0} ({1})", this.value.ToStringPercent(), s) };
            }
            return this;
        }

        private Priority add(float x, string s)
        {
            if (disabled)
            {
                return this;
            }
            float newValue = UnityEngine.Mathf.Clamp01(value + x);
            if (newValue > value)
            {
                adjustmentStrings.Add(string.Format("+{0} ({1})", (newValue - value).ToStringPercent(), s));
                value = newValue;
            }
            else if (newValue < value)
            {
                adjustmentStrings.Add(string.Format("{0} ({1})", (newValue - value).ToStringPercent(), s));
                value = newValue;
            }
            else if (newValue == value && Prefs.DevMode)
            {
                adjustmentStrings.Add(string.Format("+{0} ({1})", (newValue - value).ToStringPercent(), s));
                value = newValue;
            }
            return this;
        }

        private Priority multiply(float x, string s)
        {
            if (disabled)
            {
                return this;
            }
            float newValue = UnityEngine.Mathf.Clamp01(value * x);
            return add(newValue - value, s);
        }

        private bool isDisabled()
        {
            return this.disabled;
        }

        private Priority alwaysDoIf(bool cond, string s)
        {
            if (!cond || this.enabled)
            {
                return this;
            }
            if (Prefs.DevMode || this.disabled || this.ToGamePriority() == 0)
            {
                string text = string.Format("{0} ({1})", "FreeWillPriorityEnabled".TranslateSimple(), s);
                this.adjustmentStrings.Add(text);
            }
            this.enabled = true;
            this.disabled = false;
            return this;
        }

        private Priority alwaysDo(string s)
        {
            return this.alwaysDoIf(true, s);
        }

        private Priority neverDoIf(bool cond, string s)
        {
            if (!cond || this.disabled)
            {
                return this;
            }
            if (Prefs.DevMode || this.enabled || this.ToGamePriority() >= 0)
            {
                string text = string.Format("{0} ({1})", "FreeWillPriorityDisabled".TranslateSimple(), s);
                this.adjustmentStrings.Add(text);
            }
            this.disabled = true;
            this.enabled = false;
            return this;
        }

        private Priority neverDo(string s)
        {
            return this.neverDoIf(true, s);
        }

        private Priority considerInspiration()
        {
            if (!this.pawn.mindState.inspirationHandler.Inspired)
            {
                return this;
            }
            Inspiration inspiration = this.pawn.mindState.inspirationHandler.CurState;
            if (this.workTypeDef.defName == HUNTING && inspiration.def.defName == "Frenzy_Shoot")
            {
                return add(0.4f, "FreeWillPriorityInspired".TranslateSimple());
            }
            foreach (WorkTypeDef workTypeDefB in inspiration?.def?.requiredNonDisabledWorkTypes ?? new List<WorkTypeDef>())
            {
                if (this.workTypeDef.defName == workTypeDefB.defName)
                {
                    return add(0.4f, "FreeWillPriorityInspired".TranslateSimple());
                }
            }
            foreach (WorkTypeDef workTypeDefB in inspiration?.def?.requiredAnyNonDisabledWorkType ?? new List<WorkTypeDef>())
            {
                if (this.workTypeDef.defName == workTypeDefB.defName)
                {
                    return add(0.4f, "FreeWillPriorityInspired".TranslateSimple());
                }
            }
            return this;
        }

        private Priority considerThoughts()
        {
            List<Thought> thoughts = new List<Thought>();
            pawn.needs.mood.thoughts.GetAllMoodThoughts(thoughts);
            foreach (Thought thought in thoughts)
            {
                if (thought.def.defName == "NeedFood")
                {
                    if (workTypeDef.defName == COOKING)
                    {
                        return add(-0.01f * thought.CurStage.baseMoodEffect, "FreeWillPriorityHungerLevel".TranslateSimple());
                    }
                    if (workTypeDef.defName == HUNTING || workTypeDef.defName == PLANT_CUTTING)
                    {
                        return add(-0.005f * thought.CurStage.baseMoodEffect, "FreeWillPriorityHungerLevel".TranslateSimple());
                    }
                    return add(0.005f * thought.CurStage.baseMoodEffect, "FreeWillPriorityHungerLevel".TranslateSimple());
                }
            }
            return this;
        }

        private Priority considerNeedingWarmClothes()
        {
            if (this.mapComp.NeedWarmClothes)
            {
                return add(0.2f, "FreeWillPriorityNeedWarmClothes".TranslateSimple());
            }
            return this;
        }

        private Priority considerAnimalsRoaming()
        {
            if (this.mapComp.AlertAnimalRoaming)
            {
                return add(0.4f, "FreeWillPriorityAnimalsRoaming".TranslateSimple());
            }
            return this;
        }

        private Priority considerColonistLeftUnburied()
        {
            if (this.mapComp.AlertColonistLeftUnburied && (this.workTypeDef.defName == HAULING || this.workTypeDef.defName == HAULING_URGENT))
            {
                return add(0.4f, "AlertColonistLeftUnburied".TranslateSimple());
            }
            return this;
        }

        private Priority considerBored()
        {
            const int boredomMemory = 2500; // 1 hour in game
            if (this.pawn.mindState.IsIdle)
            {
                (this.mapComp as FreeWill_MapComponent)?.UpdateLastBored(this.pawn);
                return this.alwaysDoIf(pawn.mindState.IsIdle, "FreeWillPriorityBored".TranslateSimple());
            }
            var lastBored = (this.mapComp as FreeWill_MapComponent)?.GetLastBored(this.pawn);
            var wasBored = lastBored != 0 && Find.TickManager.TicksGame - lastBored < boredomMemory;
            return this.alwaysDoIf(wasBored, "FreeWillPriorityWasBored".TranslateSimple());
        }

        private Priority considerHasHuntingWeapon()
        {
            try
            {
                if (!this.worldComp.settings.ConsiderHasHuntingWeapon)
                {
                    return this;
                }
                return neverDoIf(!WorkGiver_HunterHunt.HasHuntingWeapon(pawn), "FreeWillPriorityNoHuntingWeapon".TranslateSimple());
            }
            catch (System.Exception err)
            {
                Log.Error(pawn.Name + " could not consider has hunting weapon to adjust " + workTypeDef.defName);
                Log.Message(err.ToString());
                Log.Message("this consideration will be disabled in the mod settings to avoid future errors");
                worldComp.settings.ConsiderHasHuntingWeapon = false;
                return this;
            }
        }

        private Priority considerBrawlersNotHunting()
        {
            if (!worldComp.settings.ConsiderBrawlersNotHunting)
            {
                return this;
            }
            try
            {
                if (this.workTypeDef.defName != HUNTING)
                {
                    return this;
                }
                return neverDoIf(this.pawn.story.traits.HasTrait(DefDatabase<TraitDef>.GetNamed("Brawler")), "FreeWillPriorityBrawler".TranslateSimple());
            }
            catch (System.Exception err)
            {
                Log.Error(pawn.Name + " could not consider brawlers can hunt to adjust " + workTypeDef.defName);
                Log.Message(err.ToString());
                Log.Message("this consideration will be disabled in the mod settings to avoid future errors");
                worldComp.settings.ConsiderBrawlersNotHunting = false;
                return this;
            }
        }

        private Priority considerCompletingTask()
        {
            if (pawn.CurJob != null && pawn.CurJob.workGiverDef != null && pawn.CurJob.workGiverDef.workType == workTypeDef)
            {
                return this

                    // pawns should not stop doing the work they are currently
                    // doing
                    .alwaysDo("FreeWillPriorityCurrentlyDoing".TranslateSimple())

                    // pawns prefer the work they are current doing
                    .multiply(1.8f, "FreeWillPriorityCurrentlyDoing".TranslateSimple())

                    ;
            }
            return this;
        }

        private Priority considerMovementSpeed()
        {
            try
            {
                if (worldComp.settings.ConsiderMovementSpeed == 0.0f)
                {
                    return this;
                }
                return this.multiply(
                    (
                        worldComp.settings.ConsiderMovementSpeed
                            * 0.25f
                            * this.pawn.GetStatValue(StatDefOf.MoveSpeed, true)
                    ),
                    "FreeWillPriorityMovementSpeed".TranslateSimple()
                );
            }
            catch (System.Exception err)
            {
                Log.Message(pawn.Name + " could not consider movement speed to adjust " + workTypeDef.defName);
                Log.Message(err.ToString());
                Log.Message("this consideration will be disabled in the mod settings to avoid future errors");
                worldComp.settings.ConsiderMovementSpeed = 0.0f;
                return this;
            }
        }

        private Priority considerCarryingCapacity()
        {
            var _baseCarryingCapacity = 75.0f;
            if (workTypeDef.defName != HAULING && workTypeDef.defName != HAULING_URGENT)
            {
                return this;
            }
            float _carryingCapacity = this.pawn.GetStatValue(StatDefOf.CarryingCapacity, true);
            if (_carryingCapacity >= _baseCarryingCapacity)
            {
                return this;
            }
            return this.multiply(_carryingCapacity / _baseCarryingCapacity, "FreeWillPriorityCarryingCapacity".TranslateSimple());
        }

        private Priority considerPassion()
        {
            try
            {
                if (worldComp.settings.ConsiderPassions == 0f)
                {
                    return this;
                }
                var relevantSkills = workTypeDef.relevantSkills;
                const Passion Apathy = (Passion)3;
                const Passion Natural = (Passion)4;
                const Passion Critical = (Passion)5;
                float x;
                for (int i = 0; i < relevantSkills.Count; i++)
                {
                    switch (pawn.skills.GetSkill(relevantSkills[i]).passion)
                    {
                        case Passion.None:
                            continue;
                        case Passion.Major:
                            x = worldComp.settings.ConsiderPassions * pawn.needs.mood.CurLevel * 0.5f / relevantSkills.Count;
                            this
                                .alwaysDo("FreeWillPriorityMajorPassionFor".Translate(relevantSkills[i].skillLabel))
                                .add(x, "FreeWillPriorityMajorPassionFor".Translate(relevantSkills[i].skillLabel));
                            continue;
                        case Passion.Minor:
                            x = worldComp.settings.ConsiderPassions * pawn.needs.mood.CurLevel * 0.25f / relevantSkills.Count;
                            this
                                .alwaysDo("FreeWillPriorityMinorPassionFor".Translate(relevantSkills[i].skillLabel))
                                .add(x, "FreeWillPriorityMinorPassionFor".Translate(relevantSkills[i].skillLabel));
                            continue;
                        case Passion.Apathy:
                            x = worldComp.settings.ConsiderPassions * pawn.needs.mood.CurLevel * 0.15f / relevantSkills.Count;
                            this
                                .alwaysDo("FreeWillPriorityApathyPassionFor".Translate(relevantSkills[i].skillLabel))
                                .add(x, "FreeWillPriorityApathyPassionFor".Translate(relevantSkills[i].skillLabel));
                            continue;
                        case Passion.Natural:
                            x = worldComp.settings.ConsiderPassions * pawn.needs.mood.CurLevel * 0.4f / relevantSkills.Count;
                            this
                                .alwaysDo("FreeWillPriorityNaturalPassionFor".Translate(relevantSkills[i].skillLabel))
                                .add(x, "FreeWillPriorityNaturalPassionFor".Translate(relevantSkills[i].skillLabel));
                            continue;
                        case Passion.Critical:
                            x = worldComp.settings.ConsiderPassions * pawn.needs.mood.CurLevel * 0.75f / relevantSkills.Count;
                            this
                                .alwaysDo("FreeWillPriorityCriticalPassionFor".Translate(relevantSkills[i].skillLabel))
                                .add(x, "FreeWillPriorityCriticalPassionFor".Translate(relevantSkills[i].skillLabel));
                            continue;
                        default:
                            considerInterest(pawn, relevantSkills[i], relevantSkills.Count, workTypeDef);
                            continue;
                    }
                }
            }
            catch (System.Exception err)
            {
                Log.ErrorOnce("could not consider passions: " + "this consideration will be disabled in the mod settings to avoid future errors: " + err.ToString(), 228486541);
                worldComp.settings.ConsiderPassions = 0.0f;
            }
            return this;
        }

        private Priority considerInterest(Pawn pawn, SkillDef skillDef, int skillCount, WorkTypeDef workTypeDef)
        {
            if (!this.worldComp.HasInterestsFramework())
            {
                return this;
            }
            SkillRecord skillRecord = pawn.skills.GetSkill(skillDef);
            float x;
            string interest;
            try
            {
                interest = this.worldComp.interestsStrings[(int)skillRecord.passion];
            }
            catch (System.Exception)
            {
                Log.Message("could not find interest for index " + ((int)skillRecord.passion).ToString());
                return this;
            }
            switch (interest)
            {
                case "DMinorAversion":
                    x = (1.0f - pawn.needs.mood.CurLevel) * -0.25f / skillCount;
                    return add(x, "FreeWillPriorityMinorAversionTo".TranslateSimple() + " " + skillDef.skillLabel);
                case "DMajorAversion":
                    x = (1.0f - pawn.needs.mood.CurLevel) * -0.5f / skillCount;
                    return add(x, "FreeWillPriorityMajorAversionTo".TranslateSimple() + " " + skillDef.skillLabel);
                case "DCompulsion":
                    List<Thought> allThoughts = new List<Thought>();
                    pawn.needs.mood.thoughts.GetAllMoodThoughts(allThoughts);
                    foreach (var thought in allThoughts)
                    {
                        if (thought.def.defName == "CompulsionUnmet")
                        {
                            switch (thought.CurStage.label)
                            {
                                case "compulsive itch":
                                    x = 0.2f / skillCount;
                                    return add(x, "FreeWillPriorityCompulsiveItch".TranslateSimple() + " " + skillDef.skillLabel);
                                case "compulsive need":
                                    x = 0.4f / skillCount;
                                    return add(x, "FreeWillPriorityCompulsiveNeed".TranslateSimple() + " " + skillDef.skillLabel);
                                case "compulsive obsession":
                                    x = 0.6f / skillCount;
                                    return add(x, "FreeWillPriorityCompulsiveObsession".TranslateSimple() + " " + skillDef.skillLabel);
                                default:
                                    Log.Message("could not read compulsion label");
                                    return this;
                            }
                        }
                        if (thought.def.defName == "NeuroticCompulsionUnmet")
                        {
                            switch (thought.CurStage.label)
                            {
                                case "compulsive itch":
                                    x = 0.3f / skillCount;
                                    return add(x, "FreeWillPriorityCompulsiveItch".TranslateSimple() + " " + skillDef.skillLabel);
                                case "compulsive demand":
                                    x = 0.6f / skillCount;
                                    return add(x, "FreeWillPriorityCompulsiveDemand".TranslateSimple() + " " + skillDef.skillLabel);
                                case "compulsive withdrawal":
                                    x = 0.9f / skillCount;
                                    return add(x, "FreeWillPriorityCompulsiveWithdrawl".TranslateSimple() + " " + skillDef.skillLabel);
                                default:
                                    Log.Message("could not read compulsion label");
                                    return this;
                            }
                        }
                        if (thought.def.defName == "VeryNeuroticCompulsionUnmet")
                        {
                            switch (thought.CurStage.label)
                            {
                                case "compulsive yearning":
                                    x = 0.4f / skillCount;
                                    return add(x, "FreeWillPriorityCompulsiveYearning".TranslateSimple() + " " + skillDef.skillLabel);
                                case "compulsive tantrum":
                                    x = 0.8f / skillCount;
                                    return add(x, "FreeWillPriorityCompulsiveTantrum".TranslateSimple() + " " + skillDef.skillLabel);
                                case "compulsive hysteria":
                                    x = 1.2f / skillCount;
                                    return add(x, "FreeWillPriorityCompulsiveHysteria".TranslateSimple() + " " + skillDef.skillLabel);
                                default:
                                    Log.Message("could not read compulsion label");
                                    return this;
                            }
                        }
                    }
                    return this;
                case "DInvigorating":
                    x = 0.1f / skillCount;
                    return add(x, "FreeWillPriorityInvigorating".TranslateSimple() + " " + skillDef.skillLabel);
                case "DInspiring":
                    return this;
                case "DStagnant":
                    return this;
                case "DForgetful":
                    return this;
                case "DVocalHatred":
                    return this;
                case "DNaturalGenius":
                    return this;
                case "DBored":
                    if (pawn.mindState.IsIdle)
                    {
                        return this;
                    }
                    return neverDo("FreeWillPriorityBoredBy".TranslateSimple() + " " + skillDef.skillLabel);
                case "DAllergic":
                    List<Hediff> resultHediffs = new List<Hediff>();
                    pawn.health.hediffSet.GetHediffs<Hediff>(ref resultHediffs);
                    foreach (var hediff in resultHediffs)
                    {
                        if (hediff.def.defName == "DAllergicReaction")
                        {
                            switch (hediff.CurStage.label)
                            {
                                case "initial":
                                    x = -0.2f / skillCount;
                                    return add(x, "FreeWillPriorityReactionInitial".TranslateSimple() + " " + skillDef.skillLabel);
                                case "itching":
                                    x = -0.5f / skillCount;
                                    return add(x, "FreeWillPriorityReactionItching".TranslateSimple() + " " + skillDef.skillLabel);
                                case "sneezing":
                                    x = -0.8f / skillCount;
                                    return add(x, "FreeWillPriorityReactionSneezing".TranslateSimple() + " " + skillDef.skillLabel);
                                case "swelling":
                                    x = -1.1f / skillCount;
                                    return add(x, "FreeWillPriorityReactionSwelling".TranslateSimple() + " " + skillDef.skillLabel);
                                case "anaphylaxis":
                                    return neverDo("FreeWillPriorityReactionAnaphylaxis".TranslateSimple() + " " + skillDef.skillLabel);
                                default:
                                    break;
                            }
                        }
                        x = 0.1f / skillCount;
                        return add(x, "FreeWillPriorityNoReaction".TranslateSimple() + " " + skillDef.skillLabel);
                    }
                    return this;
                default:
                    Log.Message("did not recognize interest: " + skillRecord.passion.ToString());
                    return this;
            }
        }

        private Priority considerDownedColonists()
        {
            if (pawn.Downed)
            {
                if (workTypeDef.defName == PATIENT || workTypeDef.defName == PATIENT_BED_REST)
                {
                    return alwaysDo("FreeWillPriorityPawnDowned".TranslateSimple()).set(1.0f, "FreeWillPriorityPawnDowned".TranslateSimple());
                }
                return neverDo("FreeWillPriorityPawnDowned".TranslateSimple());
            }
            if (mapComp.PercentPawnsDowned <= 0.0f)
            {
                return this;
            }
            if (workTypeDef.defName == DOCTOR)
            {
                return add(mapComp.PercentPawnsDowned, "FreeWillPriorityOtherPawnsDowned".TranslateSimple());
            }
            if (workTypeDef.defName == SMITHING ||
                workTypeDef.defName == TAILORING ||
                workTypeDef.defName == ART ||
                workTypeDef.defName == CRAFTING ||
                workTypeDef.defName == RESEARCHING
                )
            {
                return neverDo("FreeWillPriorityOtherPawnsDowned".TranslateSimple());
            }
            return this;
        }

        private Priority considerColonyPolicy()
        {
            try
            {
                this.add(worldComp.settings.globalWorkAdjustments[this.workTypeDef.defName], "FreeWillPriorityColonyPolicy".TranslateSimple());
            }
            catch (System.Exception)
            {
                worldComp.settings.globalWorkAdjustments[this.workTypeDef.defName] = 0.0f;
            }
            return this;
        }

        private Priority considerRefueling()
        {
            if (mapComp.RefuelNeededNow)
            {
                return this.add(0.35f, "FreeWillPriorityRefueling".TranslateSimple());
            }
            if (mapComp.RefuelNeeded)
            {
                return this.add(0.20f, "FreeWillPriorityRefueling".TranslateSimple());
            }
            return this;
        }

        private Priority considerFire()
        {
            if (mapComp.HomeFire)
            {
                if (workTypeDef.defName != FIREFIGHTER)
                {
                    return add(-0.2f, "FreeWillPriorityFireInHomeArea".TranslateSimple());
                }
                return set(1.0f, "FreeWillPriorityFireInHomeArea".TranslateSimple());
            }
            if (mapComp.MapFires > 0 && workTypeDef.defName == FIREFIGHTER)
            {
                return add(Mathf.Clamp01(mapComp.MapFires * 0.01f), "FreeWillPriorityFireOnMap".TranslateSimple());
            }
            return this;
        }

        private Priority considerOperation()
        {
            if (HealthAIUtility.ShouldHaveSurgeryDoneNow(pawn))
            {
                return set(1.0f, "FreeWillPriorityOperation".TranslateSimple());
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
                if (workTypeDef.defName == PATIENT_BED_REST)
                {
                    return add(0.4f, "FreeWillPriorityBuildingImmunity".TranslateSimple());
                }
                if (workTypeDef.defName == PATIENT)
                {
                    return this;
                }
                return add(-0.2f, "FreeWillPriorityBuildingImmunity".TranslateSimple());
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
                return this.considerThisPawnNeedsTreatment();
            }
            else
            {
                // another pawn needs treatment
                return this.considerAnotherPawnNeedsTreatment();
            }
        }

        private Priority considerThisPawnNeedsTreatment()
        {

            if (workTypeDef.defName == PATIENT || workTypeDef.defName == PATIENT_BED_REST)
            {
                // patient and bed rest are activated and set to 100%
                return this
                    .alwaysDo("FreeWillPriorityNeedTreatment".TranslateSimple())
                    .set(1.0f, "FreeWillPriorityNeedTreatment".TranslateSimple())
                    ;
            }
            if (workTypeDef.defName == DOCTOR)
            {
                if (pawn.playerSettings.selfTend)
                {
                    // this pawn can self tend, so activate doctor skill and set
                    // to 100%
                    return this
                        .alwaysDo("FreeWillPriorityNeedTreatmentSelfTend".TranslateSimple())
                        .set(1.0f, "FreeWillPriorityNeedTreatmentSelfTend".TranslateSimple())
                        ;
                }
                // doctoring stays the same
                return this;
            }
            // don't do other work types
            return neverDo("FreeWillPriorityNeedTreatment".TranslateSimple());
        }

        private Priority considerAnotherPawnNeedsTreatment()
        {
            if (workTypeDef.defName == FIREFIGHTER ||
                workTypeDef.defName == PATIENT_BED_REST
                )
            {
                // don't adjust these work types
                return this;
            }

            // increase doctor priority for all pawns
            if (workTypeDef.defName == DOCTOR)
            {
                // increase the doctor priority by the percentage of pawns
                // needing treatment
                //
                // so if 25% of the colony is injured, doctoring for all
                // non-injured pawns will increase by 25%
                return add(mapComp.PercentPawnsNeedingTreatment, "FreeWillPriorityOthersNeedTreatment".TranslateSimple());
            }

            if (workTypeDef.defName == RESEARCHING)
            {
                // don't research when someone is dying please... it's rude
                return neverDo("FreeWillPriorityOthersNeedTreatment".TranslateSimple());
            }

            if (workTypeDef.defName == SMITHING ||
                workTypeDef.defName == TAILORING ||
                workTypeDef.defName == ART ||
                workTypeDef.defName == CRAFTING
                )
            {
                // crafting work types are low priority when someone is injured
                if (this.value > 0.3f)
                {
                    return add(-(this.value - 0.3f), "FreeWillPriorityOthersNeedTreatment".TranslateSimple());
                }
                return this;
            }

            // any other work type is capped at 0.6
            if (this.value > 0.6f)
            {
                return add(-(this.value - 0.6f), "FreeWillPriorityOthersNeedTreatment".TranslateSimple());
            }
            return this;
        }

        private Priority considerHealth()
        {
            if (this.workTypeDef.defName == PATIENT || this.workTypeDef.defName == PATIENT_BED_REST)
            {
                return add(1 - Mathf.Pow(this.pawn.health.summaryHealth.SummaryHealthPercent, 7.0f), "FreeWillPriorityHealth".TranslateSimple());
            }
            return multiply(this.pawn.health.summaryHealth.SummaryHealthPercent, "FreeWillPriorityHealth".TranslateSimple());
        }

        private Priority considerFoodPoisoning()
        {
            if (worldComp.settings.ConsiderFoodPoisoning == 0.0f)
            {
                return this;
            }
            try
            {
                if (this.workTypeDef.defName != CLEANING && this.workTypeDef.defName != COOKING)
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
                        if (this.workTypeDef.defName == CLEANING)
                        {
                            return add(adjustment, "FreeWillPriorityFilthyCookingArea".TranslateSimple());
                        }
                        if (this.workTypeDef.defName == COOKING)
                        {
                            return add(-adjustment, "FreeWillPriorityFilthyCookingArea".TranslateSimple());
                        }
                    }
                }
                return this;
            }
            catch (System.Exception err)
            {
                Log.Error(pawn.Name + " could not consider food poisoning to adjust " + workTypeDef.defName);
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
                if (this.workTypeDef.defName != CLEANING)
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
                return multiply(worldComp.settings.ConsiderOwnRoom * 2.0f, "FreeWillPriorityOwnRoom".TranslateSimple());
            }
            catch (System.Exception err)
            {
                Log.Message(pawn.Name + " could not consider being in own room to adjust " + workTypeDef.defName);
                Log.Message(err.ToString());
                Log.Message("this consideration will be disabled in the mod settings to avoid future errors");
                worldComp.settings.ConsiderOwnRoom = 0.0f;
                return this;
            }
        }

        private Priority considerIsAnyoneElseDoing()
        {
            float pawnSkill = this.pawn.skills.AverageOfRelevantSkillsFor(this.workTypeDef);
            foreach (Pawn other in this.pawn.Map.mapPawns.FreeColonistsSpawned)
            {
                if (other == this.pawn)
                {
                    continue;
                }
                if (!other.Awake() || other.Downed || other.Dead)
                {
                    continue;
                }
                if (other.workSettings.GetPriority(this.workTypeDef) != 0)
                {
                    return this; // someone else is doing
                }
            }
            return this.alwaysDo("FreeWillPriorityNoOneElseDoing".TranslateSimple());
        }

        private Priority considerBestAtDoing()
        {
            try
            {
                if (worldComp.settings.ConsiderBestAtDoing == 0.0f)
                {
                    return this;
                }
                var allPawns = this.pawn.Map.mapPawns.FreeColonistsSpawned;
                if (allPawns.Count() <= 1)
                {
                    return this;
                }
                bool isBest = true;
                float pawnSkill = this.pawn.skills.AverageOfRelevantSkillsFor(workTypeDef);
                float impactPerPawn = worldComp.settings.ConsiderBestAtDoing / (float)allPawns.Count();
                foreach (Pawn other in allPawns)
                {
                    if (other == null || other == this.pawn || !other.Awake() || other.Downed || other.Dead)
                    {
                        continue;
                    }
                    float skillDiff = other.skills.AverageOfRelevantSkillsFor(workTypeDef) - pawnSkill;
                    if (skillDiff > 0.0f)
                    {
                        // not the best
                        isBest = false;
                        var isDoing = other.CurJob != null && other.CurJob.workGiverDef != null && other.CurJob.workGiverDef.workType == workTypeDef;
                        var isMuchBetter = skillDiff >= 5.0f;
                        var isMuchMuchBetter = skillDiff >= 10.0f;
                        var isMuchMuchMuchBetter = skillDiff >= 15.0f;
                        if (isDoing)
                        {
                            if (isMuchMuchMuchBetter)
                            {
                                this.add(1.5f * -impactPerPawn, "FreeWillPrioritySomeoneMuchMuchMuchBetterIsDoing".Translate(other.LabelShortCap));
                            }
                            else if (isMuchMuchBetter)
                            {
                                this.add(1.5f * -impactPerPawn * 0.8f, "FreeWillPrioritySomeoneMuchMuchBetterIsDoing".Translate(other.LabelShortCap));
                            }
                            else if (isMuchBetter)
                            {
                                this.add(1.5f * -impactPerPawn * 0.6f, "FreeWillPrioritySomeoneMuchBetterIsDoing".Translate(other.LabelShortCap));
                            }
                            else
                            {
                                this.add(1.5f * -impactPerPawn * 0.4f, "FreeWillPrioritySomeoneBetterIsDoing".Translate(other.LabelShortCap));
                            }
                        }
                        else
                        {
                            if (isMuchMuchMuchBetter)
                            {
                                this.add(-impactPerPawn, "FreeWillPrioritySomeoneMuchMuchMuchBetterAtDoing".Translate(other.LabelShortCap));
                            }
                            else if (isMuchMuchBetter)
                            {
                                this.add(-impactPerPawn * 0.8f, "FreeWillPrioritySomeoneMuchMuchBetterAtDoing".Translate(other.LabelShortCap));
                            }
                            else if (isMuchBetter)
                            {
                                this.add(-impactPerPawn * 0.6f, "FreeWillPrioritySomeoneMuchBetterAtDoing".Translate(other.LabelShortCap));
                            }
                            else
                            {
                                this.add(-impactPerPawn * 0.4f, "FreeWillPrioritySomeoneBetterAtDoing".Translate(other.LabelShortCap));
                            }
                        }
                    }
                }
                if (isBest)
                {
                    return this.multiply(1.5f * worldComp.settings.ConsiderBestAtDoing, "FreeWillPriorityBestAtDoing".TranslateSimple());
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
            if (workTypeDef.defName != DOCTOR)
            {
                return this;
            }

            int n = mapComp.NumPawns;
            if (n == 0)
            {
                return this;
            }
            float numPetsNeedingTreatment = mapComp.NumPetsNeedingTreatment;
            return add(UnityEngine.Mathf.Clamp01(numPetsNeedingTreatment / ((float)n)) * 0.5f, "FreeWillPriorityPetsInjured".TranslateSimple());
        }

        private Priority considerInjuredPrisoners()
        {
            if (workTypeDef.defName != DOCTOR)
            {
                return this;
            }

            int n = mapComp.NumPawns;
            if (n == 0)
            {
                return this;
            }
            float numPrisonersNeedingTreatment = mapComp.NumPrisonersNeedingTreatment;
            return add(UnityEngine.Mathf.Clamp01(numPrisonersNeedingTreatment / ((float)n)) * 0.5f, "FreeWillPriorityPrisonersInjured".TranslateSimple());
        }

        private Priority considerLowFood(float adjustment)
        {
            try
            {
                if (worldComp.settings.ConsiderLowFood == 0.0f || !this.mapComp.AlertLowFood)
                {
                    return this;
                }

                // don't adjust hauling if nothing deteriorating (i.e. food in the field)
                if ((this.workTypeDef.defName == HAULING || this.workTypeDef.defName == HAULING_URGENT)
                    && !this.mapComp.ThingsDeteriorating)
                {
                    return this;
                }

                return this.add(adjustment * worldComp.settings.ConsiderLowFood, "FreeWillPriorityLowFood".TranslateSimple());
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
            if ((this.worldComp?.settings?.ConsiderWeaponRange ?? 0.0f) == 0.0f)
            {
                return this;
            }
            if (!WorkGiver_HunterHunt.HasHuntingWeapon(pawn))
            {
                return this;
            }
            const float boltActionRifleRange = 37.0f;
            float range = this.pawn.equipment.PrimaryEq.PrimaryVerb.verbProps.range;
            float relativeRange = range / boltActionRifleRange;
            return this.multiply(relativeRange * this.worldComp.settings.ConsiderWeaponRange, "FreeWillPriorityWeaponRange".TranslateSimple());
        }

        private Priority considerAteRawFood()
        {
            if (this.workTypeDef.defName != COOKING)
            {
                return this;
            }

            List<Thought> allThoughts = new List<Thought>();
            this.pawn.needs.mood.thoughts.GetAllMoodThoughts(allThoughts);
            for (int i = 0; i < allThoughts.Count; i++)
            {
                Thought thought = allThoughts[i];
                if (thought.def.defName == "AteRawFood")
                {
                    if (0.6f > value)
                    {
                        return this.set(0.6f, "FreeWillPriorityAteRawFood".TranslateSimple());
                    }
                }
            }
            return this;
        }

        private Priority considerThingsDeteriorating()
        {
            if (this.workTypeDef.defName == HAULING || this.workTypeDef.defName == HAULING_URGENT)
            {
                if (mapComp.ThingsDeteriorating)
                {
                    return this.multiply(2.0f, "FreeWillPriorityThingsDeteriorating".TranslateSimple());
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
                if (this.mapComp.PlantsBlighted)
                {
                    return this.add(0.4f * worldComp.settings.ConsiderPlantsBlighted, "FreeWillPriorityBlight".TranslateSimple());
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
                if (workTypeDef.defName != PLANT_CUTTING)
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
                            return this.multiply(2.0f * worldComp.settings.ConsiderGauranlenPruning, "FreeWillPriorityPruneGauranlenTree".TranslateSimple());
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
                BeautyUtility.FillBeautyRelevantCells(pawn.Position, pawn.Map);
                float expectations = worldComp.settings.ConsiderBeauty * expectationGrid[ExpectationsUtility.CurrentExpectationFor(this.pawn).defName][this.pawn.needs.beauty.CurCategory];
                switch (workTypeDef.defName)
                {
                    case HAULING:
                    case HAULING_URGENT:
                        // check for haulable
                        if (!areaHasHaulables(BeautyUtility.beautyRelevantCells))
                        {
                            // no hauling job
                            return this;
                        }
                        if (expectations < 0.2f)
                        {
                            return this.add(expectations, "FreeWillPriorityExpectionsExceeded".TranslateSimple());
                        }
                        if (expectations < 0.4f)
                        {
                            return this.add(expectations, "FreeWillPriorityExpectionsMet".TranslateSimple());
                        }
                        if (expectations < 0.6f)
                        {
                            return this.add(expectations, "FreeWillPriorityExpectionsUnmet".TranslateSimple());
                        }
                        if (expectations < 0.8f)
                        {
                            return this.add(expectations, "FreeWillPriorityExpectionsLetDown".TranslateSimple());
                        }
                        return this.add(expectations, "FreeWillPriorityExpectionsIgnored".TranslateSimple());
                    case CLEANING:
                        // check for cleanable
                        if (!areaHasFilth(BeautyUtility.beautyRelevantCells))
                        {
                            // no cleaning job
                            return this;
                        }
                        if (expectations < 0.2f)
                        {
                            return this.add(expectations, "FreeWillPriorityExpectionsExceeded".TranslateSimple());
                        }
                        if (expectations < 0.4f)
                        {
                            return this.add(expectations, "FreeWillPriorityExpectionsMet".TranslateSimple());
                        }
                        if (expectations < 0.6f)
                        {
                            return this.add(expectations, "FreeWillPriorityExpectionsUnmet".TranslateSimple());
                        }
                        if (expectations < 0.8f)
                        {
                            return this.add(expectations, "FreeWillPriorityExpectionsLetDown".TranslateSimple());
                        }
                        return this.add(expectations, "FreeWillPriorityExpectionsIgnored".TranslateSimple());
                    default:
                        // any other work type is decreased if either job is present
                        if (!areaHasHaulables(BeautyUtility.beautyRelevantCells) && !areaHasFilth(BeautyUtility.beautyRelevantCells))
                        {
                            // nothing to do
                            return this;
                        }
                        if (expectations < 0.2f)
                        {
                            return this.add(-expectations, "FreeWillPriorityExpectionsExceeded".TranslateSimple());
                        }
                        if (expectations < 0.4f)
                        {
                            return this.add(-expectations, "FreeWillPriorityExpectionsMet".TranslateSimple());
                        }
                        if (expectations < 0.6f)
                        {
                            return this.add(-expectations, "FreeWillPriorityExpectionsUnmet".TranslateSimple());
                        }
                        if (expectations < 0.8f)
                        {
                            return this.add(-expectations, "FreeWillPriorityExpectionsLetDown".TranslateSimple());
                        }
                        return this.add(-expectations, "FreeWillPriorityExpectionsIgnored".TranslateSimple());
                } // switch
            }
            catch (System.Exception err)
            {
                Log.ErrorOnce("could not consider beauty: " + "this consideration will be disabled in the mod settings to avoid future errors: " + err.ToString(), 228652891);
                worldComp.settings.ConsiderBeauty = 0.0f;
                return this;
            }
        }

        private bool areaHasHaulables(List<IntVec3> area)
        {
            var areaHasHaulingJobToDo = false;
            foreach (IntVec3 cell in BeautyUtility.beautyRelevantCells)
            {
                foreach (Thing t in cell.GetThingList(pawn.Map))
                {
                    if (t.IsForbidden(Faction.OfPlayer))
                    {
                        continue;
                    }
                    if (!t.def.alwaysHaulable)
                    {
                        if (!t.def.EverHaulable)
                        {
                            continue;
                        }
                        if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Haul) == null && !t.IsInAnyStorage())
                        {
                            continue;
                        }
                    }
                    if (t.IsInValidBestStorage())
                    {
                        continue;
                    }
                    if (t.IsForbidden(pawn))
                    {
                        continue;
                    }
                    if (!HaulAIUtility.PawnCanAutomaticallyHaulFast(pawn, t, forced: false))
                    {
                        continue;
                    }
                    if (pawn.carryTracker.MaxStackSpaceEver(t.def) <= 0)
                    {
                        continue;
                    }
                    areaHasHaulingJobToDo = true;
                    break;
                }
                if (areaHasHaulingJobToDo)
                {
                    break;
                }
            }
            return areaHasHaulingJobToDo;
        }

        private bool areaHasFilth(List<IntVec3> area)
        {
            var areaHasCleaningJobToDo = false;
            foreach (IntVec3 cell in area)
            {
                foreach (Thing thing in cell.GetThingList(pawn.Map))
                {
                    Filth filth = thing as Filth;
                    if (filth == null)
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

        private Priority considerRelevantSkills()
        {
            float _badSkillCutoff = Mathf.Min(3f, this.mapComp.NumPawns);
            float _goodSkillCutoff = _badSkillCutoff + (20f - _badSkillCutoff) / 2f;
            float _greatSkillCutoff = _goodSkillCutoff + (20f - _goodSkillCutoff) / 2f;
            float _excellentSkillCutoff = _greatSkillCutoff + (20f - _greatSkillCutoff) / 2f;

            float _avg = this.pawn.skills.AverageOfRelevantSkillsFor(this.workTypeDef);
            if (_avg >= _excellentSkillCutoff)
            {
                return this.set(0.9f, string.Format("{0} {1:f0}", "FreeWillPrioritySkillLevel".TranslateSimple(), _avg));
            }
            if (_avg >= _greatSkillCutoff)
            {
                return this.set(0.7f, string.Format("{0} {1:f0}", "FreeWillPrioritySkillLevel".TranslateSimple(), _avg));
            }
            if (_avg >= _goodSkillCutoff)
            {
                return this.set(0.5f, string.Format("{0} {1:f0}", "FreeWillPrioritySkillLevel".TranslateSimple(), _avg));
            }
            if (_avg >= _badSkillCutoff)
            {
                return this.set(0.3f, string.Format("{0} {1:f0}", "FreeWillPrioritySkillLevel".TranslateSimple(), _avg));
            }
            return this.set(0.1f, string.Format("{0} {1:f0}", "FreeWillPrioritySkillLevel".TranslateSimple(), _avg));
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
