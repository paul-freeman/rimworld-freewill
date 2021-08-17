using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FreeWill
{
    public class FreeWill_ModSettings : ModSettings
    {
        // mod default settings
        const bool ConsiderBrawlersNotHuntingDefault = true;
        const bool ConsiderHasHuntingWeaponDefault = true;
        const float ConsiderMovementSpeedDefault = 1.0f;
        const float ConsiderFoodPoisoningDefault = 1.0f;
        const float ConsiderOwnRoomDefault = 1.0f;
        const float ConsiderPlantsBlightedDefault = 1.0f;
        const float ConsiderGauranlenPruningDefault = 1.0f;

        public bool ConsiderBrawlersNotHunting = ConsiderBrawlersNotHuntingDefault;
        public bool ConsiderHasHuntingWeapon = ConsiderHasHuntingWeaponDefault;
        public float ConsiderMovementSpeed = ConsiderMovementSpeedDefault;
        public float ConsiderFoodPoisoning = ConsiderFoodPoisoningDefault;
        public float ConsiderOwnRoom = ConsiderOwnRoomDefault;
        public float ConsiderPlantsBlighted = ConsiderPlantsBlightedDefault;
        public float ConsiderGauranlenPruning = ConsiderGauranlenPruningDefault;

        public Dictionary<string, float> globalWorkAdjustments;

        private Vector2 pos;
        private float height;

        public FreeWill_ModSettings()
        {
            globalWorkAdjustments = new Dictionary<string, float>();
            pos = Vector2.zero;
            height = 500.0f;
        }

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
            var view = new Rect(15.0f, 0, inRect.width - 30.0f, inRect.height);
            var ls = new Listing_Standard();
            var workTypes = DefDatabase<WorkTypeDef>.AllDefsListForReading;
            var v = 0.0f;
            var s1 = "";
            var s2 = "";
            var s3 = "";

            view.height = height;
            Widgets.BeginScrollView(inRect, ref pos, view);
            GUI.BeginGroup(view);
            view.height = 9999.0f;
            ls.Begin(new Rect(10, 10, view.width - 40, view.height - 10));
            ls.Gap(30.0f);

            s1 = "FreeWillConsiderMovementSpeed".TranslateSimple();
            s2 = String.Format("{0}x", ConsiderMovementSpeed);
            s3 = "FreeWillConsiderMovementSpeedLong".TranslateSimple();
            ls.LabelDouble(s1, s2, tip: s3);
            ConsiderMovementSpeed = Mathf.RoundToInt(ls.Slider(ConsiderMovementSpeed, 0.0f, 10.0f) * 10.0f) / 10.0f;
            if (ls.ButtonText("FreeWillDefaultSliderButtonLabel".TranslateSimple()))
            {
                ConsiderMovementSpeed = ConsiderMovementSpeedDefault;
            }
            ls.GapLine(30.0f);

            s1 = "FreeWillConsiderFoodPoisoning".TranslateSimple();
            s2 = String.Format("{0}x", ConsiderFoodPoisoning);
            s3 = "FreeWillConsiderFoodPoisoningLong".TranslateSimple();
            ls.LabelDouble(s1, s2, tip: s3);
            ConsiderFoodPoisoning = Mathf.RoundToInt(ls.Slider(ConsiderFoodPoisoning, 0.0f, 10.0f) * 10.0f) / 10.0f;
            if (ls.ButtonText("FreeWillDefaultSliderButtonLabel".TranslateSimple()))
            {
                ConsiderFoodPoisoning = ConsiderFoodPoisoningDefault;
            }
            ls.GapLine(30.0f);

            s1 = "FreeWillConsiderOwnRoom".TranslateSimple();
            s2 = String.Format("{0}x", ConsiderOwnRoom);
            s3 = "FreeWillConsiderOwnRoomLong".TranslateSimple();
            ls.LabelDouble(s1, s2, tip: s3);
            ConsiderOwnRoom = Mathf.RoundToInt(ls.Slider(ConsiderOwnRoom, 0.0f, 10.0f) * 10.0f) / 10.0f;
            if (ls.ButtonText("FreeWillDefaultSliderButtonLabel".TranslateSimple()))
            {
                ConsiderOwnRoom = ConsiderOwnRoomDefault;
            }
            ls.GapLine(30.0f);

            s1 = "FreeWillConsiderPlantsBlighted".TranslateSimple();
            s2 = String.Format("{0}x", ConsiderPlantsBlighted);
            s3 = "FreeWillConsiderPlantsBlightedLong".TranslateSimple();
            ls.LabelDouble(s1, s2, tip: s3);
            ConsiderPlantsBlighted = Mathf.RoundToInt(ls.Slider(ConsiderPlantsBlighted, 0.0f, 10.0f) * 10.0f) / 10.0f;
            if (ls.ButtonText("FreeWillDefaultSliderButtonLabel".TranslateSimple()))
            {
                ConsiderPlantsBlighted = ConsiderPlantsBlightedDefault;
            }
            ls.GapLine(30.0f);

            s1 = "FreeWillConsiderGauranlenPruning".TranslateSimple();
            s2 = String.Format("{0}x", ConsiderGauranlenPruning);
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
                globalWorkAdjustments.TryGetValue(workTypeDef.defName, out v);
                s1 = String.Format("{0} {1}", "FreeWillWorkTypeAdjustment".TranslateSimple(), workTypeDef.labelShort);
                s2 = String.Format("{0}%", Mathf.RoundToInt(v * 100.0f));
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

        public override void ExposeData()
        {
            Scribe_Values.Look(ref ConsiderMovementSpeed, "freeWillConsiderMovementSpeed", ConsiderMovementSpeedDefault, true);
            Scribe_Values.Look(ref ConsiderFoodPoisoning, "freeWillConsiderFoodPoisoning", ConsiderFoodPoisoningDefault, true);
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
