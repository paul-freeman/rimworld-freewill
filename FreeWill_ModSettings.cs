using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FreeWill
{
    /// <summary>
    /// Holds user configurable settings for the FreeWill mod and draws the mod settings UI.
    /// </summary>
    public class FreeWill_ModSettings : ModSettings
    {
        // mod default settings
        private const bool ConsiderBrawlersNotHuntingDefault = true;
        private const bool ConsiderHasHuntingWeaponDefault = true;
        private const float ConsiderMovementSpeedDefault = 1.0f;
        private const float ConsiderPassionsDefault = 1.0f;
        private const float ConsiderBeautyDefault = 1.0f;
        private const float ConsiderBestAtDoingDefault = 0.0f;
        private const float ConsiderFoodPoisoningDefault = 1.0f;
        private const float ConsiderLowFoodDefault = 1.0f;
        private const float ConsiderWeaponRangeDefault = 1.0f;
        private const float ConsiderOwnRoomDefault = 1.0f;
        private const float ConsiderPlantsBlightedDefault = 1.0f;
        private const float ConsiderGauranlenPruningDefault = 1.0f;

        public bool ConsiderBrawlersNotHunting = ConsiderBrawlersNotHuntingDefault;
        public bool ConsiderHasHuntingWeapon = ConsiderHasHuntingWeaponDefault;
        public float ConsiderMovementSpeed = ConsiderMovementSpeedDefault;
        public float ConsiderPassions = ConsiderPassionsDefault;
        public float ConsiderBeauty = ConsiderBeautyDefault;
        public float ConsiderBestAtDoing = ConsiderBestAtDoingDefault;
        public float ConsiderFoodPoisoning = ConsiderFoodPoisoningDefault;
        public float ConsiderLowFood = ConsiderLowFoodDefault;
        public float ConsiderWeaponRange = ConsiderWeaponRangeDefault;
        public float ConsiderOwnRoom = ConsiderOwnRoomDefault;
        public float ConsiderPlantsBlighted = ConsiderPlantsBlightedDefault;
        public float ConsiderGauranlenPruning = ConsiderGauranlenPruningDefault;

        public Dictionary<string, float> globalWorkAdjustments;

        private Vector2 pos;
        private float height;

        /// <summary>
        /// Initializes the settings with default values.
        /// </summary>
        public FreeWill_ModSettings()
        {
            globalWorkAdjustments = new Dictionary<string, float>();
            pos = Vector2.zero;
            height = 500.0f;
        }

        /// <summary>
        /// Draws the settings UI for the mod.
        /// </summary>
        /// <param name="inRect">Area to draw within.</param>
        public void DoSettingsWindowContents(Rect inRect)
        {
            if (globalWorkAdjustments == null)
            {
                globalWorkAdjustments = new Dictionary<string, float>();
            }
            if (pos == null)
            {
                pos = Vector2.zero;
            }
            Rect view = new Rect(15.0f, 0, inRect.width - 30.0f, inRect.height);
            Listing_Standard ls = new Listing_Standard();
            List<WorkTypeDef> workTypes = DefDatabase<WorkTypeDef>.AllDefsListForReading;
            view.height = height;
            Widgets.BeginScrollView(inRect, ref pos, view);
            GUI.BeginGroup(view);
            view.height = 9999.0f;
            ls.Begin(new Rect(10, 10, view.width - 40, view.height - 10));
            ls.Gap(30.0f);
            string s1 = "FreeWillConsiderMovementSpeed".TranslateSimple();
            string s2 = string.Format("{0}x", ConsiderMovementSpeed);
            string s3 = "FreeWillConsiderMovementSpeedLong".TranslateSimple();
            ls.LabelDouble(s1, s2, tip: s3);
            ConsiderMovementSpeed = Mathf.RoundToInt(ls.Slider(ConsiderMovementSpeed, 0.0f, 10.0f) * 10.0f) / 10.0f;
            if (ls.ButtonText("FreeWillDefaultSliderButtonLabel".TranslateSimple()))
            {
                ConsiderMovementSpeed = ConsiderMovementSpeedDefault;
            }
            ls.GapLine(30.0f);

            s1 = "FreeWillConsiderPassions".TranslateSimple();
            s2 = string.Format("{0}x", ConsiderPassions);
            s3 = "FreeWillConsiderPassionsLong".TranslateSimple();
            ls.LabelDouble(s1, s2, tip: s3);
            ConsiderPassions = Mathf.RoundToInt(ls.Slider(ConsiderPassions, 0.0f, 10.0f) * 10.0f) / 10.0f;
            if (ls.ButtonText("FreeWillDefaultSliderButtonLabel".TranslateSimple()))
            {
                ConsiderPassions = ConsiderPassionsDefault;
            }
            ls.GapLine(30.0f);

            s1 = "FreeWillConsiderBeauty".TranslateSimple();
            s2 = string.Format("{0}x", ConsiderBeauty);
            s3 = "FreeWillConsiderBeautyLong".TranslateSimple();
            ls.LabelDouble(s1, s2, tip: s3);
            ConsiderBeauty = Mathf.RoundToInt(ls.Slider(ConsiderBeauty, 0.0f, 10.0f) * 10.0f) / 10.0f;
            if (ls.ButtonText("FreeWillDefaultSliderButtonLabel".TranslateSimple()))
            {
                ConsiderBeauty = ConsiderBeautyDefault;
            }
            ls.GapLine(30.0f);

            s1 = "FreeWillConsiderBestAtDoing".TranslateSimple();
            s2 = string.Format("{0}x", ConsiderBestAtDoing);
            s3 = "FreeWillConsiderBestAtDoingLong".TranslateSimple();
            ls.LabelDouble(s1, s2, tip: s3);
            ConsiderBestAtDoing = Mathf.RoundToInt(ls.Slider(ConsiderBestAtDoing, 0.0f, 10.0f) * 10.0f) / 10.0f;
            if (ls.ButtonText("FreeWillDefaultSliderButtonLabel".TranslateSimple()))
            {
                ConsiderBestAtDoing = ConsiderBestAtDoingDefault;
            }
            ls.GapLine(30.0f);

            s1 = "FreeWillConsiderLowFood".TranslateSimple();
            s2 = string.Format("{0}x", ConsiderLowFood);
            s3 = "FreeWillConsiderLowFoodLong".TranslateSimple();
            ls.LabelDouble(s1, s2, tip: s3);
            ConsiderLowFood = Mathf.RoundToInt(ls.Slider(ConsiderLowFood, 0.0f, 10.0f) * 10.0f) / 10.0f;
            if (ls.ButtonText("FreeWillDefaultSliderButtonLabel".TranslateSimple()))
            {
                ConsiderLowFood = ConsiderLowFoodDefault;
            }
            ls.GapLine(30.0f);

            s1 = "FreeWillConsiderWeaponRange".TranslateSimple();
            s2 = string.Format("{0}x", ConsiderWeaponRange);
            s3 = "FreeWillConsiderWeaponRangeLong".TranslateSimple();
            ls.LabelDouble(s1, s2, tip: s3);
            ConsiderWeaponRange = Mathf.RoundToInt(ls.Slider(ConsiderWeaponRange, 0.0f, 10.0f) * 10.0f) / 10.0f;
            if (ls.ButtonText("FreeWillDefaultSliderButtonLabel".TranslateSimple()))
            {
                ConsiderWeaponRange = ConsiderWeaponRangeDefault;
            }
            ls.GapLine(30.0f);

            s1 = "FreeWillConsiderFoodPoisoning".TranslateSimple();
            s2 = string.Format("{0}x", ConsiderFoodPoisoning);
            s3 = "FreeWillConsiderFoodPoisoningLong".TranslateSimple();
            ls.LabelDouble(s1, s2, tip: s3);
            ConsiderFoodPoisoning = Mathf.RoundToInt(ls.Slider(ConsiderFoodPoisoning, 0.0f, 10.0f) * 10.0f) / 10.0f;
            if (ls.ButtonText("FreeWillDefaultSliderButtonLabel".TranslateSimple()))
            {
                ConsiderFoodPoisoning = ConsiderFoodPoisoningDefault;
            }
            ls.GapLine(30.0f);

            s1 = "FreeWillConsiderOwnRoom".TranslateSimple();
            s2 = string.Format("{0}x", ConsiderOwnRoom);
            s3 = "FreeWillConsiderOwnRoomLong".TranslateSimple();
            ls.LabelDouble(s1, s2, tip: s3);
            ConsiderOwnRoom = Mathf.RoundToInt(ls.Slider(ConsiderOwnRoom, 0.0f, 10.0f) * 10.0f) / 10.0f;
            if (ls.ButtonText("FreeWillDefaultSliderButtonLabel".TranslateSimple()))
            {
                ConsiderOwnRoom = ConsiderOwnRoomDefault;
            }
            ls.GapLine(30.0f);

            s1 = "FreeWillConsiderPlantsBlighted".TranslateSimple();
            s2 = string.Format("{0}x", ConsiderPlantsBlighted);
            s3 = "FreeWillConsiderPlantsBlightedLong".TranslateSimple();
            ls.LabelDouble(s1, s2, tip: s3);
            ConsiderPlantsBlighted = Mathf.RoundToInt(ls.Slider(ConsiderPlantsBlighted, 0.0f, 10.0f) * 10.0f) / 10.0f;
            if (ls.ButtonText("FreeWillDefaultSliderButtonLabel".TranslateSimple()))
            {
                ConsiderPlantsBlighted = ConsiderPlantsBlightedDefault;
            }
            ls.GapLine(30.0f);

            s1 = "FreeWillConsiderGauranlenPruning".TranslateSimple();
            s2 = string.Format("{0}x", ConsiderGauranlenPruning);
            s3 = "FreeWillConsiderGauranlenPruningLong".TranslateSimple();
            ls.LabelDouble(s1, s2, tip: s3);
            ConsiderGauranlenPruning = Mathf.RoundToInt(ls.Slider(ConsiderGauranlenPruning, 0.0f, 10.0f) * 10.0f) / 10.0f;
            if (ls.ButtonText("FreeWillDefaultSliderButtonLabel".TranslateSimple()))
            {
                ConsiderGauranlenPruning = ConsiderGauranlenPruningDefault;
            }
            ls.GapLine(30.0f);

            ls.CheckboxLabeled("FreeWillConsiderHasHuntingWeapon".TranslateSimple(), ref ConsiderHasHuntingWeapon, "FreeWillConsiderHasHuntingWeaponLong".TranslateSimple());
            ls.Gap(20.0f);
            ls.CheckboxLabeled("FreeWillConsiderBrawlersNotHunting".TranslateSimple(), ref ConsiderBrawlersNotHunting, "FreeWillConsiderBrawlersNotHuntingLong".TranslateSimple());

            // draw sliders for each work type
            ls.GapLine(60.0f);
            foreach (WorkTypeDef workTypeDef in workTypes)
            {
                _ = globalWorkAdjustments.TryGetValue(workTypeDef.defName, out float v);
                s1 = string.Format("{0} {1}", "FreeWillWorkTypeAdjustment".TranslateSimple(), workTypeDef.labelShort);
                s2 = string.Format("{0}%", Mathf.RoundToInt(v * 100.0f));
                ls.LabelDouble(s1, s2, tip: workTypeDef.description);
                globalWorkAdjustments.SetOrAdd(
                    workTypeDef.defName,
                    Mathf.RoundToInt(ls.Slider(v, -1.0f, 1.0f) * 100.0f) / 100.0f
                );
                ls.Gap();
            }

            // slider reset button
            if (ls.ButtonTextLabeled("FreeWillResetGlobalSlidersLabel".TranslateSimple(), "FreeWillResetGlobalSlidersButtonLabel".TranslateSimple()))
            {
                foreach (WorkTypeDef workTypeDef in workTypes)
                {
                    globalWorkAdjustments.SetOrAdd(workTypeDef.defName, 0f);
                }
            }

            ls.GapLine(40.0f);
            height = ls.GetRect(0).yMax + 20.0f;
            ls.End();
            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        /// <summary>
        /// Saves and loads the mod settings.
        /// </summary>
        public override void ExposeData()
        {
            Scribe_Values.Look(ref ConsiderMovementSpeed, "freeWillConsiderMovementSpeed", ConsiderMovementSpeedDefault, true);
            Scribe_Values.Look(ref ConsiderPassions, "freeWillConsiderPassions", ConsiderPassionsDefault, true);
            Scribe_Values.Look(ref ConsiderBeauty, "freeWillConsiderBeauty", ConsiderBeautyDefault, true);
            Scribe_Values.Look(ref ConsiderBestAtDoing, "freeWillConsiderBestAtDoing", ConsiderBestAtDoingDefault, true);
            Scribe_Values.Look(ref ConsiderFoodPoisoning, "freeWillConsiderFoodPoisoning", ConsiderFoodPoisoningDefault, true);
            Scribe_Values.Look(ref ConsiderLowFood, "freeWillConsiderLowFood", ConsiderLowFoodDefault, true);
            Scribe_Values.Look(ref ConsiderWeaponRange, "freeWillConsiderWeaponRange", ConsiderWeaponRangeDefault, true);
            Scribe_Values.Look(ref ConsiderOwnRoom, "freeWillConsiderOwnRoom", ConsiderOwnRoomDefault, true);
            Scribe_Values.Look(ref ConsiderBrawlersNotHunting, "freeWillBrawlersNotHunting", ConsiderBrawlersNotHuntingDefault, true);
            Scribe_Values.Look(ref ConsiderHasHuntingWeapon, "freeWillHuntingWeapon", ConsiderHasHuntingWeaponDefault, true);
            Scribe_Values.Look(ref ConsiderPlantsBlighted, "freeWillPlantsBlighted", ConsiderPlantsBlightedDefault, true);
            Scribe_Values.Look(ref ConsiderGauranlenPruning, "freeWillGauranlenPruning", ConsiderGauranlenPruningDefault, true);
            if (globalWorkAdjustments == null)
            {
                globalWorkAdjustments = new Dictionary<string, float>();
            }
            Scribe_Collections.Look(ref globalWorkAdjustments, "freeWillWorkTypeAdjustments", LookMode.Value, LookMode.Value);
            base.ExposeData();
        }
    }
}
