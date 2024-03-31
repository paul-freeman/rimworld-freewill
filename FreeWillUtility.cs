using RimWorld;
using System;
using System.Text;
using UnityEngine;
using Verse;

namespace FreeWill
{
    public static class FreeWillUtility
    {
        private static FreeWill_WorldComponent worldComp;
        private static FreeWill_MapComponent mapComp;

        private static readonly int couldNotGetAdjustmentString = "FreewillCouldNotGetAdjustmentString".GetHashCode();
        private static readonly int couldNotGetTip = "FreewillCouldNotGetTip".GetHashCode();

        public static FreeWill_WorldComponent GetWorldComponent()
        {
            return worldComp ?? Find.World.GetComponent<FreeWill_WorldComponent>();
        }

        public static FreeWill_MapComponent GetMapComponent()
        {
            return mapComp ?? Find.CurrentMap.GetComponent<FreeWill_MapComponent>();
        }

        public static void UpdateWorldComponent(FreeWill_WorldComponent newWorldComp)
        {
            worldComp = newWorldComp ?? Find.World.GetComponent<FreeWill_WorldComponent>();
        }

        public static void UpdateMapComponent(FreeWill_MapComponent newMapComp)
        {
            mapComp = newMapComp ?? Find.CurrentMap.GetComponent<FreeWill_MapComponent>();
        }

        public static void DoCell(Rect rect, Pawn pawn, PawnTable _, PawnColumnWorker_WorkPriority __instance)
        {
            Text.Font = GameFont.Medium;
            float x = rect.x + ((rect.width - 25f) / 2f);
            float y = rect.y + 2.5f;
            bool incapable = IsIncapableOfWholeWorkType(pawn, __instance.def.workType);
            Priority priority = GetMapComponent()?.GetPriority(pawn, __instance.def.workType);
            if (priority == null)
            {
                return;
            }
            if (!priority.Disabled)
            {
                DrawWorkBoxFor(x, y, pawn, priority, incapable);
            }
            Rect rect2 = new Rect(x, y, 25f, 25f);
            if (Mouse.IsOver(rect2))
            {
                TooltipHandler.TipRegion(rect2, () => GetTip(priority), pawn.thingIDNumber ^ priority.WorkTypeDef.GetHashCode());
            }
            Text.Font = GameFont.Small;
        }

        public static void DrawWorkBoxFor(float x, float y, Pawn pawn, Priority priority, bool __)
        {
            Rect rect = new Rect(x, y, 25f, 25f);
            GUI.color = Color.white;

            float uniqueModifier = (pawn.thingIDNumber + x + y) * 18577.0f;
            float timeBasedFlicker = Mathf.Sin((Find.TickManager.TicksGame + uniqueModifier) * 500.0f) * 0.1f;
            float flickeredValue = priority.Value + timeBasedFlicker;
            if (flickeredValue > 1.0f)
            {
                flickeredValue = 1.0f - (flickeredValue - 1.0f);
            }
            flickeredValue = Mathf.Clamp01(flickeredValue);

            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, flickeredValue);
            GUI.DrawTexture(rect, Resources.FreeWillOverlay);
        }

        public static bool IsIncapableOfWholeWorkType(Pawn p, WorkTypeDef work)
        {
            for (int i = 0; i < work.workGiversByPriority.Count; i++)
            {
                bool flag = true;
                for (int j = 0; j < work.workGiversByPriority[i].requiredCapacities.Count; j++)
                {
                    PawnCapacityDef capacity = work.workGiversByPriority[i].requiredCapacities[j];
                    if (!p.health.capacities.CapableOf(capacity))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    return false;
                }
            }
            return true;
        }

        public static string GetTip(Priority priority)
        {
            try
            {
                StringBuilder stringBuilder = new StringBuilder();
                TaggedString workTypeTitle = priority.WorkTypeDef.pawnLabel.CapitalizeFirst().AsTipTitle();
                stringBuilder = stringBuilder.AppendLineTagged(workTypeTitle)
                    .AppendLineTagged(priority.WorkTypeDef.description.Colorize(ColoredText.SubtleGrayColor)).AppendLine()
                    .AppendLineTagged(("FreeWillWorkPreference".Translate().CapitalizeFirst() + ": ").AsTipTitle() + priority.Value.ToStringPercent());
                foreach (Func<string> ProduceAdjustmentString in priority.AdjustmentStrings)
                {
                    try
                    {
                        string adjustmentString = ProduceAdjustmentString();
                        stringBuilder = stringBuilder.AppendLine(adjustmentString);
                    }
                    catch (Exception e)
                    {
                        Log.ErrorOnce("Free Will: could not get adjustment string: " + e.Message, couldNotGetAdjustmentString);
                        stringBuilder = Prefs.DevMode ? stringBuilder.AppendLine("error: " + e.Message) : stringBuilder.AppendLine("error");
                    }
                }
                stringBuilder = stringBuilder.AppendLine();
                if (!priority.Disabled)
                {
                    int p = priority.ToGamePriority();
                    string priorityDescriptionStr = string.Format("Priority{0}", p).TranslateSimple();
                    string priorityLevelStr = p + " - " + priorityDescriptionStr;
                    TaggedString colorizedPriorityLevelStr = priorityLevelStr.Colorize(WidgetsWork.ColorOfPriority(p));
                    TaggedString priorityTitle = ("Priority".Translate().CapitalizeFirst() + ": ").AsTipTitle();
                    stringBuilder = stringBuilder.AppendLineTagged(priorityTitle + colorizedPriorityLevelStr);
                }
                return stringBuilder.ToString();
            }
            catch (Exception e)
            {
                Log.ErrorOnce("Free Will: could not get tip: " + e.Message, couldNotGetTip);
                return "could not get tip";
            }
        }
    }
}