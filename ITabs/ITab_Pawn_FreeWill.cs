using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Verse;
using RimWorld;

namespace FreeWill
{

    public class ITab_Pawn_FreeWill : ITab
    {
        private const float width = 300f;
        private const float height = 500f;
        private const float topPadding = 5f;

        private float scrollViewHeight;
        private Vector2 scrollPosition;
        private Color highlightColor;
        private FreeWill_WorldComponent worldComp;

        public ITab_Pawn_FreeWill()
        {
            size = new Vector2(width, height);
            labelKey = "FreeWillITab";
            scrollPosition = Vector2.zero;
            highlightColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            worldComp = Find.World?.GetComponent<FreeWill_WorldComponent>();
        }

        public override bool IsVisible
        {
            get
            {
                var pawn = getCurrentPawn();
                if (pawn == null)
                {
                    return false;
                }
                return pawn.IsColonistPlayerControlled;
            }
        }

        protected override void FillTab()
        {
            Pawn pawn = getCurrentPawn();
            if (pawn == null)
            {
                Log.Error("Free will tab found; no selected pawn to display.");
                return;
            }
            worldComp = worldComp ?? Find.World?.GetComponent<FreeWill_WorldComponent>();
            if (worldComp == null)
            {
                return;
            }
            Text.Font = GameFont.Small;
            Rect rect = new Rect(0f, topPadding, this.size.x, this.size.y - topPadding).ContractedBy(20f);
            Rect position = new Rect(rect.x, rect.y, rect.width, rect.height);

            try
            {
                GUI.BeginGroup(position);
                Text.Font = GameFont.Small;
                GUI.color = Color.white;
                Rect outRect = new Rect(0f, 0f, position.width, position.height);
                Rect viewRect = new Rect(0f, 0f, position.width - 16f, this.scrollViewHeight);
                try
                {
                    Widgets.BeginScrollView(outRect, ref this.scrollPosition, viewRect, true);
                    float num = 0f;

                    DrawPawnProfession(pawn, ref num, viewRect.width);
                    DrawPawnInterestText(pawn, ref num, viewRect.width);
                    DrawPawnPriorities(pawn, ref num, viewRect.width);
                    DrawPawnFreeWillCheckbox(pawn, ref num, viewRect.width);

                    if (Event.current.type == EventType.Layout)
                    {
                        if (num + 70f > 450f)
                        {
                            this.size.y = Mathf.Min(num + 70f, (float)(UI.screenHeight - 35) - 165f - 30f);
                        }
                        else
                        {
                            this.size.y = 450f;
                        }
                        this.scrollViewHeight = num + 20f;
                    }
                }
                catch
                {
                    Log.Error("could not draw fill tab scroll view");
                }
                finally
                {
                    Widgets.EndScrollView();

                }
            }
            catch
            {
                Log.Error("could not draw fill tab group");
            }
            finally
            {
                GUI.EndGroup();
                GUI.color = Color.white;
                Text.Anchor = TextAnchor.UpperLeft;
            }
        }


        private void DrawPawnProfession(Pawn pawn, ref float curY, float width)
        {
            try
            {
                Text.Font = GameFont.Medium;
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = new Color(0.9f, 0.9f, 0.9f);
                Rect rect = new Rect(0f, curY, width, 30f);
                Widgets.Label(rect, pawn.story.TitleCap);
            }
            catch
            {
                Log.Error("could not draw pawn profession");
            }
            curY += 30f;
        }

        private void DrawPawnInterestText(Pawn pawn, ref float curY, float width)
        {
            try
            {
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.UpperLeft;
                GUI.color = new Color(0.9f, 0.9f, 0.9f);
                Rect rect2 = new Rect(0f, curY, width, 25f);
                if (worldComp.HasFreeWill(pawn))
                {
                    Widgets.Label(rect2, "FreeWillWorkPreference".TranslateSimple());
                }
                else
                {
                    Widgets.Label(rect2, "FreeWillWorkAssignment".TranslateSimple());
                }
            }
            catch
            {
                Log.Error("could not draw pawn interest text");
            }
            curY += 25f;
        }

        private void DrawPawnPriorities(Pawn pawn, ref float curY, float width)
        {
            try
            {
                if (!pawn.Dead)
                {
                    foreach (KeyValuePair<WorkTypeDef, Priority> pair in (
                            from x in pawn.Map.GetComponent<FreeWill_MapComponent>().GetPriorities(pawn)
                            orderby x.Key.naturalPriority descending
                            select x
                            ))
                    {
                        DrawPawnWorkPriority(pawn, ref curY, width, pair.Key, pair.Value);
                    }
                }
            }
            catch
            {
                Log.Error("could not draw pawn priorities");
            }
        }

        private void DrawPawnFreeWillCheckbox(Pawn pawn, ref float curY, float width)
        {
            try
            {
                if (pawn == null)
                {
                    return;
                }
                worldComp = worldComp ?? Find.World?.GetComponent<FreeWill_WorldComponent>();
                if (worldComp == null)
                {
                    return;
                }
                bool isFree = worldComp.HasFreeWill(pawn);
                bool flag = isFree;
                Rect rect = new Rect(0f, curY, width, 24f);
                Text.Font = GameFont.Small;
                GUI.color = Color.white;

                var canChange = worldComp.FreeWillCanChange(pawn);
                Widgets.CheckboxLabeled(rect, "FreeWillITabCheckbox".TranslateSimple(), ref isFree, !canChange, null, null, false);
                if (Mouse.IsOver(rect))
                {
                    string tip;
                    if (pawn.IsSlaveOfColony)
                    {
                        // pawn is slave
                        tip = "FreeWillITabWorkScheduleSlave".Translate(pawn.NameShortColored);
                    }
                    else if (canChange)
                    {
                        tip = "FreeWillITabWorkScheduleCanChange".Translate(pawn.NameShortColored);
                    }
                    else
                    {
                        if (isFree)
                        {
                            // free will mandatory - work schedule prohibited
                            tip = "FreeWillITabWorkScheduleProhibited".Translate(pawn.NameShortColored, pawn.Ideo.MemberNamePlural.CapitalizeFirst()).CapitalizeFirst();
                        }
                        else
                        {
                            // free will prohibited - work schedule mandatory
                            tip = "FreeWillITabWorkScheduleMandatory".Translate(pawn.NameShortColored, pawn.Ideo.MemberNamePlural.CapitalizeFirst()).CapitalizeFirst();
                        }
                    }

                    TooltipHandler.TipRegion(rect, tip);
                }
                if (flag != isFree)
                {
                    bool ok = (isFree) ? worldComp.TryGiveFreeWill(pawn) : worldComp.TryRemoveFreeWill(pawn);
                    if (!ok)
                    {
                        Log.Error($"could not change free will for {pawn.Name.ToStringShort}");
                    }
                }
            }
            catch
            {
                Log.Error("could not draw pawn free will checkbox");
            }
            curY += 28f;
        }

        private void DrawPawnWorkPriority(Pawn pawn, ref float curY, float width, WorkTypeDef workTypeDef, Priority priority)
        {
            try
            {
                if (pawn.Dead || pawn.workSettings == null || !pawn.workSettings.EverWork)
                {
                    return;
                }

                int p = priority.ToGamePriority();
                GUI.color = Color.white;
                Text.Font = GameFont.Small;
                string t = priority.GetTip();
                Func<string> textGetter = delegate ()
                {
                    return t;
                };
                Rect rect = new Rect(10f, curY, width - 10f, 20f);
                if (Mouse.IsOver(rect))
                {
                    GUI.color = highlightColor;
                    GUI.DrawTexture(rect, TexUI.HighlightTex);
                    TooltipHandler.TipRegion(rect, new TipSignal(textGetter, pawn.thingIDNumber ^ (int)workTypeDef.index));
                }
                GUI.color = WidgetsWork.ColorOfPriority(p);
                Text.Anchor = TextAnchor.MiddleLeft;
                Widgets.Label(rect, workTypeDef.pawnLabel);

                if (p == 0 && pawn.GetDisabledWorkTypes(true).Contains(workTypeDef))
                {
                    GUI.color = new Color(1f, 0f, 0f, 0.5f);
                    Widgets.DrawLineHorizontal(0f, rect.center.y, rect.width);
                }
            }
            catch
            {
                Log.Message("could not draw pawn work priority");
            }
            curY += 20f;
        }


        private Pawn getCurrentPawn()
        {
            if (base.SelPawn != null)
            {
                return base.SelPawn;
            }
            Corpse corpse = base.SelThing as Corpse;
            if (corpse != null)
            {
                return corpse.InnerPawn;
            }
            return null;
        }
    }
}
