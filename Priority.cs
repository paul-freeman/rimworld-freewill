using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;


namespace FreeWill
{    /// <summary>
     /// Calculates a pawn's work priority based on many in-game factors and
     /// converts between mod values and game values.
     /// </summary>
    public class Priority : IComparable
    {
        private const int DISABLED_CUTOFF = 100 / (Pawn_WorkSettings.LowestPriority + 1); // 20 if LowestPriority is 4
        private const int DISABLED_CUTOFF_ACTIVE_WORK_AREA = 100 - DISABLED_CUTOFF; // 80 if LowestPriority is 4
        private const float ONE_PRIORITY_WIDTH = DISABLED_CUTOFF_ACTIVE_WORK_AREA / (float)Pawn_WorkSettings.LowestPriority; // ~20 if LowestPriority is 4

        public readonly Pawn pawn;
        private readonly IPriorityDependencyProvider dependencyProvider;
        private IWorldStateProvider worldStateProvider;
        private IMapStateProvider mapStateProvider;
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
        private const string COOKING = "Cooking";
        private const string HUNTING = "Hunting";
        private const string CONSTRUCTION = "Construction";
        private const string PLANT_CUTTING = "PlantCutting";
        private const string SMITHING = "Smithing";
        private const string TAILORING = "Tailoring";
        private const string ART = "Art";
        private const string CRAFTING = "Crafting";
        private const string HAULING = "Hauling";
        private const string CLEANING = "Cleaning";
        private const string RESEARCHING = "Research";

        // supported modded work types
        private const string HAULING_URGENT = "HaulingUrgent";        /// <summary>
                                                                      /// Initializes a new priority instance for a pawn and work type.
                                                                      /// </summary>
                                                                      /// <param name="pawn">Pawn to evaluate.</param>
                                                                      /// <param name="workTypeDef">Work type being processed.</param>
        public Priority(Pawn pawn, WorkTypeDef workTypeDef)
            : this(pawn, workTypeDef, PriorityDependencyProviderFactory.Current)
        {
        }

        /// <summary>
        /// Initializes a new priority instance for a pawn and work type with custom dependency provider.
        /// This constructor is primarily used for testing and allows dependency injection.
        /// </summary>
        /// <param name="pawn">Pawn to evaluate.</param>
        /// <param name="workTypeDef">Work type being processed.</param>
        /// <param name="dependencyProvider">Provider for dependencies.</param>
        public Priority(Pawn pawn, WorkTypeDef workTypeDef, IPriorityDependencyProvider dependencyProvider)
        {
            this.pawn = pawn;
            WorkTypeDef = workTypeDef;
            this.dependencyProvider = dependencyProvider ?? throw new System.ArgumentNullException(nameof(dependencyProvider));
            AdjustmentStrings = new List<Func<string>> { };
        }        /// <summary>
                 /// Calculates the priority value using numerous heuristics.
                 /// </summary>
        public void Compute()
        {
            try
            {
                AdjustmentStrings = new List<Func<string>> { };

                // Validate pawn state before computation
                if (pawn?.Map == null)
                {
                    if (Prefs.DevMode)
                    {
                        Log.Warning($"Free Will: pawn {pawn?.Name?.ToStringShort ?? "null"} has no map, using default priority for {WorkTypeDef?.defName ?? "null"}");
                    }
                    Set(0.2f, "FreeWillPriorityDefault".TranslateSimple);
                    return;
                }

                // Initialize dependencies
                worldStateProvider = dependencyProvider.WorldStateProvider;
                mapStateProvider = dependencyProvider.GetMapStateProvider(pawn);

                if (mapStateProvider == null)
                {
                    if (Prefs.DevMode)
                    {
                        Log.Warning($"Free Will: no map state provider found for pawn {pawn.Name}, using default priority");
                    }
                    Set(0.2f, "FreeWillPriorityDefault".TranslateSimple);
                    return;
                }

                if (worldStateProvider == null)
                {
                    if (Prefs.DevMode)
                    {
                        Log.Warning($"Free Will: no world state provider found for pawn {pawn.Name}, using default priority");
                    }
                    Set(0.2f, "FreeWillPriorityDefault".TranslateSimple);
                    return;
                }

                // start priority at the global default and compute the priority
                // using the AI in this file
                Set(0.2f, "FreeWillPriorityGlobalDefault".TranslateSimple);
                _ = InnerCompute();
                return;
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not compute {WorkTypeDef?.defName ?? "unknown"} priority for pawn: {pawn?.Name?.ToStringShort ?? "unknown"}: {ex.Message}");
                }
                _ = AlwaysDo("FreeWillPriorityError".TranslateSimple);
                Set(0.4f, "FreeWillPriorityError".TranslateSimple);
                // Do NOT re-throw - handle gracefully with default priority
                return;
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

            // Additional safety checks
            if (mapStateProvider?.DisabledWorkTypes == null)
            {
                if (Prefs.DevMode)
                {
                    Log.Warning($"Free Will: mapStateProvider.DisabledWorkTypes is null for pawn {pawn?.Name?.ToStringShort ?? "unknown"}");
                }
                return this;
            }

            if (mapStateProvider.DisabledWorkTypes.Contains(WorkTypeDef))
            {
                return NeverDo("FreeWillPriorityPermanentlyDisabled".TranslateSimple);
            }

            try
            {
                // Use strategy pattern instead of large switch statement
                IWorkTypeStrategy strategy = dependencyProvider.StrategyProvider.GetStrategy(WorkTypeDef);
                if (strategy == null)
                {
                    if (Prefs.DevMode)
                    {
                        Log.Warning($"Free Will: no strategy found for work type {WorkTypeDef?.defName ?? "unknown"}");
                    }
                    return this;
                }
                return strategy.CalculatePriority(this);
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: error in strategy calculation for {WorkTypeDef?.defName ?? "unknown"}: {ex}");
                }
                return this;
            }
        }

        /// <summary>
        /// Writes the computed priority back to the pawn's work settings.
        /// </summary>
        public void ApplyPriorityToGame()
        {
            HandleExceptionWrapper(() =>
            {
                if (pawn?.workSettings == null || WorkTypeDef == null)
                {
                    if (Prefs.DevMode)
                    {
                        Log.Warning($"Free Will: cannot apply priority - pawn.workSettings or WorkTypeDef is null for {pawn?.Name?.ToStringShort ?? "unknown"}");
                    }
                    return;
                }

                if (!Current.Game.playSettings.useWorkPriorities)
                {
                    Current.Game.playSettings.useWorkPriorities = true;
                }

                int priority = ToGamePriority();
                if (pawn.workSettings.GetPriority(WorkTypeDef) != priority)
                {
                    pawn.workSettings.SetPriority(WorkTypeDef, priority);
                }
            }, "apply priority to game");
        }

        /// <summary>
        /// Converts the current value into a RimWorld work priority integer.
        /// </summary>
        /// <returns>The equivalent game priority value.</returns>
        public int ToGamePriority()
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
                Log.Error("Free Will: calculated an invalid game priority value of " + gamePriorityValue.ToString());
                gamePriorityValue = Mathf.Clamp(gamePriorityValue, 1, Pawn_WorkSettings.LowestPriority);
            }

            return gamePriorityValue;
        }

        /// <summary>
        /// Updates the priority value using a RimWorld work priority integer.
        /// </summary>
        /// <param name="gamePriorityValue">Game priority value.</param>
        public void FromGamePriority(int gamePriorityValue)
        {
            AdjustmentStrings = new List<Func<string>> { };

            if (gamePriorityValue == 0)
            {
                Set(0.0f, "FreeWillPriorityFromGame".TranslateSimple);
                return;
            }

            float invertedValueRange = (gamePriorityValue - 0.5f) * ONE_PRIORITY_WIDTH;
            float valueInt = DISABLED_CUTOFF_ACTIVE_WORK_AREA - invertedValueRange + DISABLED_CUTOFF;
            float finalValue = Mathf.Clamp(valueInt, 0, 100) / 100;
            Set(finalValue, "FreeWillPriorityFromGame".TranslateSimple);
        }

        /// <summary>
        /// Wraps an operation with standardized exception handling.
        /// Logs errors in dev mode and returns this instance for graceful continuation.
        /// </summary>
        /// <param name="operation">The operation to execute</param>
        /// <param name="operationName">The name of the operation for error logging</param>
        /// <returns>The result of the operation, or this instance if an exception occurs</returns>
        private Priority HandleExceptionWrapper(Func<Priority> operation, string operationName)
        {
            try
            {
                return operation();
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not {operationName}: {ex.Message}");
                }
                return this;
            }
        }

        /// <summary>
        /// Wraps a void operation with standardized exception handling.
        /// Logs errors in dev mode and continues gracefully.
        /// </summary>
        /// <param name="operation">The operation to execute</param>
        /// <param name="operationName">The name of the operation for error logging</param>
        private void HandleExceptionWrapper(Action operation, string operationName)
        {
            try
            {
                operation();
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not {operationName}: {ex.Message}");
                }
                // Continue gracefully - don't re-throw
            }
        }

        public void Set(float x, Func<string> description)
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

        public void Add(float x, Func<string> description)
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

        public Priority Multiply(float x, Func<string> description)
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

        public Priority AlwaysDoIf(bool cond, Func<string> description)
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

        public Priority AlwaysDo(Func<string> description)
        {
            return AlwaysDoIf(true, description);
        }

        public Priority NeverDoIf(bool cond, Func<string> description)
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

        public Priority NeverDo(Func<string> description)
        {
            return NeverDoIf(true, description);
        }

        public Priority ConsiderInspiration()
        {
            return HandleExceptionWrapper(() =>
            {
                if (pawn?.mindState?.inspirationHandler?.Inspired != true)
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
            }, "consider inspiration");
        }
        public Priority ConsiderThoughts()
        {
            return HandleExceptionWrapper(() =>
            {
                if (pawn?.Map == null || mapStateProvider == null)
                {
                    return this;
                }

                foreach (Thought thought in mapStateProvider.AllThoughts)
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
            }, "consider thoughts");
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

        public Priority ConsiderNeedingWarmClothes()
        {
            return HandleExceptionWrapper(() =>
            {
                if (!EnsureMapStateProvider())
                {
                    return this;
                }
                if (mapStateProvider.AlertNeedWarmClothes)
                {
                    Add(0.2f, "FreeWillPriorityNeedWarmClothes".TranslateSimple);
                    return this;
                }
                return this;
            }, "consider needing warm clothes");
        }

        public Priority ConsiderAnimalsRoaming()
        {
            try
            {
                if (!EnsureMapStateProvider())
                {
                    return this;
                }
                if (mapStateProvider.AlertAnimalRoaming)
                {
                    Add(0.4f, "FreeWillPriorityAnimalsRoaming".TranslateSimple);
                    return this;
                }
                return this;
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider animals roaming: {ex.Message}");
                }
                return this;
            }
        }

        public Priority ConsiderSuppressionNeed()
        {
            try
            {
                if (!EnsureMapStateProvider())
                {
                    return this;
                }
                if (mapStateProvider.SuppressionNeed != 0.0f)
                {
                    Add(mapStateProvider.SuppressionNeed, "FreeWillPrioritySuppressionNeed".TranslateSimple);
                    return this;
                }
                return this;
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider suppression need: {ex.Message}");
                }
                return this;
            }
        }

        public Priority ConsiderColonistLeftUnburied()
        {
            try
            {
                if (!EnsureMapStateProvider())
                {
                    return this;
                }
                if (mapStateProvider.AlertColonistLeftUnburied && (WorkTypeDef.defName == HAULING || WorkTypeDef.defName == HAULING_URGENT))
                {
                    Add(0.4f, "FreeWillPriorityColonistLeftUnburied".TranslateSimple);
                    return this;
                }
                return this;
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider colonist left unburied: {ex.Message}");
                }
                return this;
            }
        }

        public Priority ConsiderBored()
        {
            try
            {
                const int boredomMemory = 2500; // 1 hour in game
                if (pawn.mindState.IsIdle)
                {
                    mapStateProvider?.UpdateLastBored(pawn);
                    return AlwaysDoIf(pawn.mindState.IsIdle, "FreeWillPriorityBored".TranslateSimple);
                }
                if (!EnsureMapStateProvider())
                {
                    return this;
                }
                int? lastBored = mapStateProvider.GetLastBored(pawn);
                bool wasBored = lastBored != 0 && Find.TickManager.TicksGame - lastBored < boredomMemory;
                return AlwaysDoIf(wasBored, "FreeWillPriorityWasBored".TranslateSimple);
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider bored: {ex.Message}");
                }
                return this;
            }
        }

        public Priority ConsiderHasHuntingWeapon()
        {
            return HandleExceptionWrapper(() =>
            {
                return (WorldCompSettings?.ConsiderHasHuntingWeapon ?? false)
                    ? NeverDoIf(!WorkGiver_HunterHunt.HasHuntingWeapon(pawn), "FreeWillPriorityNoHuntingWeapon".TranslateSimple)
                    : this;
            }, "consider has hunting weapon");
        }

        public Priority ConsiderBrawlersNotHunting()
        {
            return HandleExceptionWrapper(() =>
            {
                return (WorldCompSettings?.ConsiderBrawlersNotHunting ?? false) && WorkTypeDef.defName == HUNTING
                    ? NeverDoIf(pawn.story.traits.HasTrait(DefDatabase<TraitDef>.GetNamed("Brawler")), "FreeWillPriorityBrawler".TranslateSimple)
                    : this;
            }, "consider brawlers not hunting");
        }

        public Priority ConsiderCompletingTask()
        {
            try
            {
                // pawns prefer the work they are current doing
                return pawn.CurJob?.workGiverDef?.workType == WorkTypeDef
                    ? AlwaysDo("FreeWillPriorityCurrentlyDoing".TranslateSimple)
                        .Multiply(1.8f, "FreeWillPriorityCurrentlyDoing".TranslateSimple)
                    : this;
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider completing task: {ex.Message}");
                }
                return this;
            }
        }

        public Priority ConsiderMovementSpeed()
        {
            try
            {
                float setting = WorldCompSettings?.ConsiderMovementSpeed ?? 0.0f;
                return setting != 0.0f
                    ? Multiply(setting * (pawn.GetStatValue(StatDefOf.MoveSpeed, true) / 4.6f), "FreeWillPriorityMovementSpeed".TranslateSimple)
                    : this;
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider movement speed: {ex.Message}");
                }
                return this;
            }
        }

        public Priority ConsiderCarryingCapacity()
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
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider carrying capacity: {ex.Message}");
                }
                return this;
            }
        }

        public Priority ConsiderPassion()
        {
            try
            {
                if (WorldCompSettings?.ConsiderPassions == 0f)
                {
                    return this;
                }
                List<SkillDef> relevantSkills = WorkTypeDef.relevantSkills;
                float x;
                for (int i = 0; i < relevantSkills.Count; i++)
                {
                    int index = i;
                    Passion passion = pawn.skills.GetSkill(relevantSkills[i]).passion;
                    switch (passion)
                    {
                        case Passion.None:
                            continue;
                        case Passion.Minor:
                            x = WorldCompSettings.ConsiderPassions * pawn.needs.mood.CurLevel * 0.25f / relevantSkills.Count;

                            _ = AlwaysDo(() => "FreeWillPriorityMinorPassionFor".Translate(relevantSkills[index].skillLabel));
                            Add(x, () => "FreeWillPriorityMinorPassionFor".Translate(relevantSkills[index].skillLabel));
                            continue;
                        case Passion.Major:
                            x = WorldCompSettings.ConsiderPassions * pawn.needs.mood.CurLevel * 0.5f / relevantSkills.Count;

                            _ = AlwaysDo(() => "FreeWillPriorityMajorPassionFor".Translate(relevantSkills[index].skillLabel));
                            Add(x, () => "FreeWillPriorityMajorPassionFor".Translate(relevantSkills[index].skillLabel));
                            continue;
                        default:
                            ConsiderVanillaSkillsExpanded(passion, relevantSkills[index], relevantSkills.Count);
                            continue;
                    }
                }
                return this;
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider passion: {ex.Message}");
                }
                return this;
            }
        }
        public void ConsiderVanillaSkillsExpanded(Passion passion, SkillDef relevantSkill, int relevantSkillsCount)
        {
            try
            {
                if (WorldCompSettings == null)
                {
                    return;
                }
                switch ((int)passion)
                {
                    case 3: // Vanilla Skills Expanded: Apathy
                        if (passion.GetLabel() != "Apathy")
                        {
                            // There is a third passion from another mod, which we don't want to consider.
                            return;
                        }
                        Set(0.0f, () => "FreeWillPriorityApathy".Translate(relevantSkill.skillLabel));
                        break;
                    case 4: // Vanilla Skills Expanded: Natural
                        if (passion.GetLabel() != "Natural")
                        {
                            // There is a fourth passion from another mod, which we don't want to consider.
                            return;
                        }
                        _ = AlwaysDo(() => "FreeWillPriorityNatural".Translate(relevantSkill.skillLabel));
                        break;
                    case 5: // Vanilla Skills Expanded: Critical
                        if (passion.GetLabel() != "Critical")
                        {
                            // There is a fifth passion from another mod, which we don't want to consider.
                            return;
                        }
                        float x = WorldCompSettings.ConsiderPassions / relevantSkillsCount;
                        _ = AlwaysDo(() => "FreeWillPriorityCritical".Translate(relevantSkill.skillLabel));
                        Add(x, () => "FreeWillPriorityCritical".Translate(relevantSkill.skillLabel));
                        break;
                    default:
                        Log.WarningOnce($"Free Will: could not consider {passion} ({passion.GetLabel()}) passion for {WorkTypeDef.defName}", 1518670634);
                        return;
                }
            }
            catch (Exception e)
            {
                Log.ErrorOnce($"Free Will: could not consider vanilla skills expanded: {e}", 1550506890);
            }
        }

        public Priority ConsiderFinishedMechGestators()
        {
            try
            {
                if (pawn?.Map?.listerThings == null)
                {
                    return this;
                }
                List<Thing> mechGestators = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.MechGestator);
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
                    // In Rimworld 1.5, the state was changed from
                    // FormingCycleState to FormingState. So we cast to int to
                    // avoid a compiler error.
                    if ((int)productionMech.State != 3) // FormingState.Formed
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
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider finished mech gestators: {ex.Message}");
                }
                return this;
            }
        }
        public Priority ConsiderDownedColonists()
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
                if (!EnsureMapStateProvider())
                {
                    return this;
                }

                if (mapStateProvider.PercentPawnsDowned <= 0.0f)
                {
                    return this;
                }
                if (WorkTypeDef.defName == DOCTOR)
                {
                    Add(mapStateProvider.PercentPawnsDowned, "FreeWillPriorityOtherPawnsDowned".TranslateSimple);
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
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider downed colonists: {ex.Message}");
                }
                return this;
            }
        }

        public Priority ConsiderColonyPolicy()
        {
            try
            {
                if (worldStateProvider == null)
                {
                    return this;
                }
                if (!worldStateProvider.Settings.globalWorkAdjustments.ContainsKey(WorkTypeDef.defName))
                {
                    worldStateProvider.Settings.globalWorkAdjustments.Add(WorkTypeDef.defName, 0.0f);
                }
                float adj = worldStateProvider.Settings.globalWorkAdjustments[WorkTypeDef.defName];
                Add(adj, "FreeWillPriorityColonyPolicy".TranslateSimple);
                return this;
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider colony policy: {ex.Message}");
                }
                return this;
            }
        }

        public Priority ConsiderRefueling()
        {
            try
            {
                if (!EnsureMapStateProvider())
                {
                    return this;
                }
                if (mapStateProvider.RefuelNeededNow)
                {
                    Add(0.35f, "FreeWillPriorityRefueling".TranslateSimple);
                    return this;
                }
                if (mapStateProvider.RefuelNeeded)
                {
                    Add(0.20f, "FreeWillPriorityRefueling".TranslateSimple);
                    return this;
                }
                return this;
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider refueling: {ex.Message}");
                }
                return this;
            }
        }
        public Priority ConsiderFire()
        {
            try
            {
                if (!EnsureMapStateProvider())
                {
                    return this;
                }

                if (WorkTypeDef.defName != FIREFIGHTER)
                {
                    if (mapStateProvider.HomeFire)
                    {
                        Add(-0.2f, "FreeWillPriorityFireInHomeArea".TranslateSimple);
                        return this;
                    }
                    if (mapStateProvider.MapFires > 0 && WorkTypeDef.defName == FIREFIGHTER)
                    {
                        Add(Mathf.Clamp01(mapStateProvider.MapFires * 0.01f), "FreeWillPriorityFireOnMap".TranslateSimple);
                        return this;
                    }
                    return this;
                }
                if (mapStateProvider.HomeFire)
                {
                    Set(1.0f, "FreeWillPriorityFireInHomeArea".TranslateSimple);
                    return this;
                }
                if (mapStateProvider.MapFires > 0 && WorkTypeDef.defName == FIREFIGHTER)
                {
                    Add(Mathf.Clamp01(mapStateProvider.MapFires * 0.01f), "FreeWillPriorityFireOnMap".TranslateSimple);
                    return this;
                }
                return this;
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider fire: {ex.Message}");
                }
                return this;
            }
        }

        public Priority ConsiderOperation()
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
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider operation: {ex.Message}");
                }
                return this;
            }
        }

        public Priority ConsiderBuildingImmunity()
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
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider building immunity: {ex.Message}");
                }
                return this;
            }
        }

        public Priority ConsiderColonistsNeedingTreatment()
        {
            try
            {
                if (!EnsureMapStateProvider())
                {
                    return this;
                }

                if (mapStateProvider.PercentPawnsNeedingTreatment <= 0.0f)
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
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider colonists needing treatment: {ex.Message}");
                }
                return this;
            }
        }

        public Priority ConsiderThisPawnNeedsTreatment()
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
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider this pawn needs treatment: {ex.Message}");
                }
                return this;
            }
        }

        public Priority ConsiderAnotherPawnNeedsTreatment()
        {
            try
            {
                if (!EnsureMapStateProvider())
                {
                    return this;
                }
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
                    Add(mapStateProvider.PercentPawnsNeedingTreatment, "FreeWillPriorityOthersNeedTreatment".TranslateSimple);
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
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider another pawn needs treatment: {ex.Message}");
                }
                return this;
            }
        }

        public Priority ConsiderHealth()
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
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider health: {ex.Message}");
                }
                return this;
            }
        }

        public void ConsiderHavingFoodPoisoning()
        {
            if (WorkTypeDef.defName == PATIENT_BED_REST)
            {
                if (pawn?.health?.hediffSet?.HasHediff(HediffDefOf.FoodPoisoning, true) ?? false)
                {
                    float movementSpeed = pawn.GetStatValue(StatDefOf.MoveSpeed, true);
                    Add(Mathf.Clamp01(1 - (movementSpeed / 4.6f)), "FreeWillPriorityFoodPoisoning".TranslateSimple);
                }
            }
        }

        public void ConsiderAnimalPen()
        {
            if (WorkTypeDef.defName == CONSTRUCTION)
            {
                if (mapStateProvider.AlertAnimalPenNeeded)
                {
                    Add(0.3f, "FreeWillPriorityAnimalPenNeeded".TranslateSimple);
                }
                if (mapStateProvider.AlertAnimalPenNotEnclosed)
                {
                    Add(0.3f, "FreeWillPriorityAnimalPenNotEnclosed".TranslateSimple);
                }
            }
        }

        public Priority ConsiderFoodPoisoningRisk()
        {
            try
            {
                if (worldStateProvider?.Settings?.ConsiderFoodPoisoning == 0.0f)
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
                        adjustment = worldStateProvider.Settings.ConsiderFoodPoisoning * 20.0f * pawn.GetRoom().GetStat(RoomStatDefOf.FoodPoisonChance);
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
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider food poisoning risk: {ex.Message}");
                }
                return this;
            }
        }

        public Priority ConsiderOwnRoom()
        {
            try
            {
                if (worldStateProvider?.Settings?.ConsiderOwnRoom == 0.0f)
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
                return !isPawnsRoom ? this : Multiply(worldStateProvider.Settings.ConsiderOwnRoom * 2.0f, "FreeWillPriorityOwnRoom".TranslateSimple);
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider own room: {ex.Message}");
                }
                return this;
            }
        }

        public Priority ConsiderRepairingMech()
        {
            try
            {
                if (!EnsureMapStateProvider())
                {
                    return this;
                }
                if (mapStateProvider.AlertMechDamaged)
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
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider repairing mech: {ex.Message}");
                }
                return this;
            }
        }
        public Priority ConsiderIsAnyoneElseDoing()
        {
            try
            {
                if (pawn?.Map?.mapPawns == null)
                {
                    return this;
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
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider is anyone else doing: {ex.Message}");
                }
                return this;
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

                int priority = other.workSettings.GetPriority(WorkTypeDef);
                bool isOtherPawnDoing = priority != 0;
                if (isOtherPawnDoing)
                {
                    return true;
                }

                bool isMechDoing = other.RaceProps.mechEnabledWorkTypes.Contains(WorkTypeDef);

                return isMechDoing;
            }
            catch (Exception e)
            {
                if (Prefs.DevMode)
                {
                    AdjustmentStrings.Add(() => "SomeoneElseDoingError");
                }
                Log.ErrorOnce($"Free Will: could not determine if someone else is doing: {e}", 1203438361);
                return false;
            }
        }

        public Priority ConsiderBestAtDoing()
        {
            try
            {
                return ConsiderBestAtDoingCore();
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider best at doing: {ex.Message}");
                }
                return this;
            }
        }
        public Priority ConsiderBestAtDoingCore()
        {
            if (worldStateProvider?.Settings?.ConsiderBestAtDoing == 0.0f)
            {
                return this;
            }

            List<Pawn> allPawns = GetEligiblePawns();
            if (allPawns.Count <= 1)
            {
                return this;
            }

            SkillComparisonData comparisonData = CalculatePawnSkillComparison(allPawns);
            ApplySkillBasedAdjustments(comparisonData);

            return comparisonData.IsBestAtDoing
                ? Multiply(1.5f * worldStateProvider.Settings.ConsiderBestAtDoing, "FreeWillPriorityBestAtDoing".TranslateSimple)
                : this;
        }
        private List<Pawn> GetEligiblePawns()
        {
            return pawn?.Map?.mapPawns == null ? new List<Pawn>() : pawn.Map.mapPawns.PawnsInFaction(Faction.OfPlayer);
        }
        private SkillComparisonData CalculatePawnSkillComparison(List<Pawn> allPawns)
        {
            SkillComparisonData data = new SkillComparisonData
            {
                IsBestAtDoing = true,
                PawnSkill = pawn.skills.AverageOfRelevantSkillsFor(WorkTypeDef),
                ImpactPerPawn = worldStateProvider.Settings.ConsiderBestAtDoing / allPawns.Count
            };

            foreach (Pawn other in allPawns)
            {
                OtherPawnSkillInfo otherSkillInfo = GetOtherPawnSkillInfo(other);
                if (otherSkillInfo == null)
                {
                    continue;
                }

                float skillDiff = otherSkillInfo.Skill - data.PawnSkill;
                if (skillDiff <= 0.0f)
                {
                    continue;
                }

                data.IsBestAtDoing = false;
                data.SkillComparisons.Add(new PawnSkillComparison
                {
                    Other = other,
                    SkillDifference = skillDiff,
                    IsCurrentlyDoing = IsPawnCurrentlyDoingWork(other)
                });
            }

            return data;
        }
        private OtherPawnSkillInfo GetOtherPawnSkillInfo(Pawn other)
        {
            try
            {
                if (other == null || other == pawn)
                {
                    return null;
                }
                if (!other.IsColonistPlayerControlled && !other.IsColonyMechPlayerControlled)
                {
                    return null;
                }
                if (!other.Awake() || other.Downed || other.Dead || other.IsCharging())
                {
                    return null;
                }
                if (other.IsColonyMechPlayerControlled && !other.RaceProps.mechEnabledWorkTypes.Contains(WorkTypeDef))
                {
                    return null;
                }

                float skill = other.IsColonistPlayerControlled
                    ? other.skills.AverageOfRelevantSkillsFor(WorkTypeDef)
                    : other.RaceProps.mechFixedSkillLevel;

                return new OtherPawnSkillInfo { Skill = skill };
            }
            catch (Exception e)
            {
                Log.ErrorOnce($"Free Will: could not compute skill difference: {e}: (logged only once)", 856149440);
                return null;
            }
        }

        private bool IsPawnCurrentlyDoingWork(Pawn other)
        {
            return other.CurJob?.workGiverDef?.workType == WorkTypeDef;
        }

        private void ApplySkillBasedAdjustments(SkillComparisonData data)
        {
            foreach (PawnSkillComparison comparison in data.SkillComparisons)
            {
                SkillLevel skillLevel = GetSkillLevel(comparison.SkillDifference);
                float impactMultiplier = GetImpactMultiplier(skillLevel);
                float adjustment = comparison.IsCurrentlyDoing ? -1.5f * data.ImpactPerPawn * impactMultiplier : -data.ImpactPerPawn * impactMultiplier;
                string messageKey = GetMessageKey(skillLevel, comparison.IsCurrentlyDoing);

                Add(adjustment, () => messageKey.Translate(comparison.Other.LabelShortCap));
            }
        }

        private SkillLevel GetSkillLevel(float skillDifference)
        {
            return skillDifference >= 15.0f
                ? SkillLevel.MuchMuchMuchBetter
                : skillDifference >= 10.0f ? SkillLevel.MuchMuchBetter : skillDifference >= 5.0f ? SkillLevel.MuchBetter : SkillLevel.Better;
        }

        private float GetImpactMultiplier(SkillLevel skillLevel)
        {
            switch (skillLevel)
            {
                case SkillLevel.MuchMuchMuchBetter:
                    return 1.0f;
                case SkillLevel.MuchMuchBetter:
                    return 0.8f;
                case SkillLevel.MuchBetter:
                    return 0.6f;
                case SkillLevel.Better:
                    return 0.4f;
                default:
                    return 0.4f;
            }
        }

        private string GetMessageKey(SkillLevel skillLevel, bool isCurrentlyDoing)
        {
            string suffix = isCurrentlyDoing ? "IsDoing" : "AtDoing";
            switch (skillLevel)
            {
                case SkillLevel.MuchMuchMuchBetter:
                    return $"FreeWillPrioritySomeoneMuchMuchMuchBetter{suffix}";
                case SkillLevel.MuchMuchBetter:
                    return $"FreeWillPrioritySomeoneMuchMuchBetter{suffix}";
                case SkillLevel.MuchBetter:
                    return $"FreeWillPrioritySomeoneMuchBetter{suffix}";
                case SkillLevel.Better:
                    return $"FreeWillPrioritySomeoneBetter{suffix}";
                default:
                    return $"FreeWillPrioritySomeoneBetter{suffix}";
            }
        }

        public Priority ConsiderInjuredPets()
        {
            try
            {
                if (!EnsureMapStateProvider())
                {
                    return this;
                }
                if (WorkTypeDef.defName != DOCTOR)
                {
                    return this;
                }
                int n = mapStateProvider.NumPawns;
                if (n == 0)
                {
                    return this;
                }
                float numPetsNeedingTreatment = mapStateProvider.NumPetsNeedingTreatment;
                Add(Mathf.Clamp01(numPetsNeedingTreatment / n) * 0.5f, "FreeWillPriorityPetsInjured".TranslateSimple);
                return this;
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider injured pets: {ex.Message}");
                }
                return this;
            }
        }

        public Priority ConsiderMechHaulers()
        {
            try
            {
                if (!EnsureMapStateProvider())
                {
                    return this;
                }
                float percentPawnsMechHaulers = mapStateProvider.PercentPawnsMechHaulers;
                Add(-percentPawnsMechHaulers, "FreeWillPriorityMechHaulers".TranslateSimple);
                return this;
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider mech haulers: {ex.Message}");
                }
                return this;
            }
        }

        public Priority ConsiderInjuredPrisoners()
        {
            try
            {
                if (!EnsureMapStateProvider())
                {
                    return this;
                }
                if (WorkTypeDef.defName != DOCTOR)
                {
                    return this;
                }
                int n = mapStateProvider.NumPawns;
                if (n == 0)
                {
                    return this;
                }
                float numPrisonersNeedingTreatment = mapStateProvider.NumPrisonersNeedingTreatment;
                Add(Mathf.Clamp01(numPrisonersNeedingTreatment / n) * 0.5f, "FreeWillPriorityPrisonersInjured".TranslateSimple);
                return this;
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider injured prisoners: {ex.Message}");
                }
                return this;
            }
        }

        public Priority ConsiderLowFood(float adjustment)
        {
            try
            {
                if (worldStateProvider?.Settings?.ConsiderLowFood == 0.0f || !mapStateProvider.AlertLowFood)
                {
                    return this;
                }

                // don't adjust hauling if nothing deteriorating (i.e. food in the field)
                if ((WorkTypeDef.defName != HAULING && WorkTypeDef.defName != HAULING_URGENT) || mapStateProvider.ThingsDeteriorating != null)
                {
                    Add(adjustment * worldStateProvider.Settings.ConsiderLowFood, "FreeWillPriorityLowFood".TranslateSimple);
                    return this;
                }
                return this;
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider low food: {ex.Message}");
                }
                return this;
            }
        }

        public Priority ConsiderWeaponRange()
        {
            try
            {
                if ((worldStateProvider?.Settings?.ConsiderWeaponRange ?? 0.0f) == 0.0f)
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
                return Multiply(relativeRange * worldStateProvider.Settings.ConsiderWeaponRange, "FreeWillPriorityWeaponRange".TranslateSimple);
            }
            catch (Exception e)
            {
                if (Prefs.DevMode)
                {
                    AdjustmentStrings.Add(() => "WeaponRangeError");
                }
                Log.ErrorOnce($"Free Will: could not consider weapon range: {e}", 219276975);
                return this;
            }
        }

        public Priority ConsiderAteRawFood()
        {
            try
            {
                if (!EnsureMapStateProvider())
                {
                    return this;
                }
                if (WorkTypeDef.defName != COOKING)
                {
                    return this;
                }

                foreach (Thought thought in mapStateProvider.AllThoughts)
                {
                    if (thought.def.defName == "AteRawFood" && 0.6f > Value)
                    {
                        Set(0.6f, "FreeWillPriorityAteRawFood".TranslateSimple);
                        return this;
                    }
                }
                return this;
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider ate raw food: {ex.Message}");
                }
                return this;
            }
        }

        public Priority ConsiderThingsDeteriorating()
        {
            try
            {
                if (!EnsureMapStateProvider())
                {
                    return this;
                }
                if (WorkTypeDef.defName == HAULING || WorkTypeDef.defName == HAULING_URGENT)
                {
                    if (mapStateProvider.ThingsDeteriorating != null)
                    {
                        if (Prefs.DevMode)
                        {
                            string name = mapStateProvider.ThingsDeteriorating.def.defName;
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
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider things deteriorating: {ex.Message}");
                }
                return this;
            }
        }

        public Priority ConsiderPlantsBlighted()
        {
            try
            {
                if (worldStateProvider?.Settings?.ConsiderPlantsBlighted == 0.0f)
                {
                    // no point checking if it is disabled
                    return this;
                }
                if (!EnsureMapStateProvider())
                {
                    return this;
                }
                if (mapStateProvider.PlantsBlighted)
                {
                    Add(0.4f * worldStateProvider.Settings.ConsiderPlantsBlighted, "FreeWillPriorityBlight".TranslateSimple);
                    return this;
                }
                return this;
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider plants blighted: {ex.Message}");
                }
                return this;
            }
        }

        public Priority ConsiderGauranlenPruning()
        {
            try
            {
                if (worldStateProvider == null)
                {
                    return this;
                }
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
                            : Multiply(2.0f * worldStateProvider.Settings.ConsiderGauranlenPruning, "FreeWillPriorityPruneGauranlenTree".TranslateSimple);
                        {
                        }
                    }
                }
                return this;
            }
            catch (Exception ex)
            {
                if (Prefs.DevMode)
                {
                    Log.Error($"Free Will: could not consider gauranlen pruning: {ex.Message}");
                }
                return this;
            }
        }

        public Priority ConsiderBeautyExpectations()
        {
            try
            {
                if ((worldStateProvider?.Settings?.ConsiderBeauty ?? 0.0f) == 0.0f)
                {
                    return this;
                }
                if (!EnsureMapStateProvider())
                {
                    return this;
                }
                float expectations = worldStateProvider.Settings.ConsiderBeauty * expectationGrid[ExpectationsUtility.CurrentExpectationFor(pawn).defName][pawn.needs.beauty.CurCategory];
                switch (WorkTypeDef.defName)
                {
                    case HAULING:
                    case HAULING_URGENT:
                        // check for haulable
                        if (!mapStateProvider.AreaHasHaulables)
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
                        if (!mapStateProvider.AreaHasFilth)
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
                        if (!mapStateProvider.AreaHasFilth && !mapStateProvider.AreaHasHaulables)
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
                Log.ErrorOnce($"Free Will: could not consider beauty expectations: {e}: (logged only once)", 1177516601);
            }

            return this;
        }

        public Priority ConsiderRelevantSkills(bool shouldAdd = false)
        {
            try
            {
                if (!EnsureMapStateProvider())
                {
                    return this;
                }
                float badSkillCutoff = Mathf.Min(3f, mapStateProvider.NumPawns);
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
            catch
            {
                if (Prefs.DevMode)
                {
                    Log.Error("Free Will: could not consider relevant skills");
                }
                return this;
            }
        }
        public bool NotInHomeArea(Pawn pawn)
        {
            try
            {
                return this.pawn?.Map?.areaManager?.Home != null && !this.pawn.Map.areaManager.Home[pawn.Position];
            }
            catch
            {
                if (Prefs.DevMode)
                {
                    Log.Error("Free Will: could not determine if pawn is not in home area");
                }
                throw;
            }
        }

        /// <summary>
        /// Ensures that map state provider is available.
        /// </summary>
        /// <returns>True if available, false otherwise.</returns>
        private bool EnsureMapStateProvider()
        {
            return mapStateProvider != null;
        }

        /// <summary>
        /// Ensures that world state provider is available.
        /// </summary>
        /// <returns>True if available, false otherwise.</returns>
        private bool EnsureWorldStateProvider()
        {
            return worldStateProvider != null;
        }

        /// <summary>
        /// Legacy compatibility method for EnsureMapComp calls.
        /// </summary>
        /// <returns>True if map state provider is available, false otherwise.</returns>
        private bool EnsureMapComp()
        {
            return EnsureMapStateProvider();
        }

        /// <summary>
        /// Legacy compatibility property for worldComp access.
        /// </summary>
        private FreeWill_ModSettings WorldCompSettings => worldStateProvider?.Settings;

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

    public enum SkillLevel
    {
        Better,
        MuchBetter,
        MuchMuchBetter,
        MuchMuchMuchBetter
    }

    public class SkillComparisonData
    {
        public bool IsBestAtDoing { get; set; }
        public float PawnSkill { get; set; }
        public float ImpactPerPawn { get; set; }
        public List<PawnSkillComparison> SkillComparisons { get; set; } = new List<PawnSkillComparison>();
    }

    public class PawnSkillComparison
    {
        public Pawn Other { get; set; }
        public float SkillDifference { get; set; }
        public bool IsCurrentlyDoing { get; set; }
    }

    public class OtherPawnSkillInfo
    {
        public float Skill { get; set; }
    }
}
