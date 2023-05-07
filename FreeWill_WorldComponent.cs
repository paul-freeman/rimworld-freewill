using System.Collections.Generic;
using Verse;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using HarmonyLib;

namespace Rimworld_FreeWillMod
{
    public class FreeWill_WorldComponent : WorldComponent
    {
        private FreeWill_Mod mod = LoadedModManager.GetMod<FreeWill_Mod>();
        public FreeWill_ModSettings settings
        {
            get
            {
                return mod.GetSettings<FreeWill_ModSettings>();
            }
        }

        private Dictionary<string, bool> freePawns;
        private Dictionary<string, int> freeWillOverride;

        private PreceptDef freeWillProhibited;
        private PreceptDef freeWillDisapproved;
        private PreceptDef freeWillFlexible;
        private PreceptDef freeWillPreferred;
        private PreceptDef freeWillMandatory;

        private bool checkedForInterestsMod;
        public List<string> interestsStrings;

        public FreeWill_WorldComponent(World world) : base(world)
        {
            freePawns = new Dictionary<string, bool> { };
            freeWillOverride = new Dictionary<string, int> { };

            freeWillProhibited = DefDatabase<PreceptDef>.GetNamed("Free_Will_Prohibited");
            freeWillDisapproved = DefDatabase<PreceptDef>.GetNamed("Free_Will_Disapproved");
            freeWillFlexible = DefDatabase<PreceptDef>.GetNamed("Free_Will_Flexible");
            freeWillPreferred = DefDatabase<PreceptDef>.GetNamed("Free_Will_Preferred");
            freeWillMandatory = DefDatabase<PreceptDef>.GetNamed("Free_Will_Mandatory");

            checkedForInterestsMod = false;
            interestsStrings = new List<string> { };
        }

        public override void WorldComponentTick()
        {
            // add Free Will ideology if an ideo doesn't have it
            foreach (Ideo ideo in Find.IdeoManager.IdeosListForReading)
            {
                if (!ideo.HasPrecept(freeWillProhibited)
                        && !ideo.HasPrecept(freeWillDisapproved)
                        && !ideo.HasPrecept(freeWillFlexible)
                        && !ideo.HasPrecept(freeWillPreferred)
                        && !ideo.HasPrecept(freeWillMandatory))
                {
                    Log.Message("Adding free will precept, \"flexible\", to " + ideo.name + " ideology.");
                    ideo.AddPrecept(PreceptMaker.MakePrecept(freeWillFlexible), init: false);
                }
            }
            this.freePawns = this.freePawns ?? new Dictionary<string, bool> { };
            base.WorldComponentTick();
        }

        public override void FinalizeInit()
        {
            base.FinalizeInit();
            FreeWillUtility.UpdateWorldComponent(this);
        }

        public bool HasFreeWill(Pawn pawn)
        {
            if (pawn?.Ideo == null ||
                !pawn.IsColonistPlayerControlled ||
                pawn.IsSlaveOfColony ||
                pawn.Ideo.HasPrecept(freeWillProhibited)
            )
            {
                return false;
            }
            if (pawn.Ideo.HasPrecept(freeWillMandatory))
            {
                return true;
            }

            var pawnKey = pawn.GetUniqueLoadID();
            this.freePawns = this.freePawns ?? new Dictionary<string, bool> { };
            if (!this.freePawns.ContainsKey(pawnKey))
            {
                if (pawn.Ideo.HasPrecept(freeWillDisapproved) || pawn.Ideo.HasPrecept(freeWillProhibited))
                {
                    this.freePawns[pawnKey] = false;
                    return false;
                }
                else
                {
                    this.freePawns[pawnKey] = true;
                    return true;
                }
            }
            return this.freePawns[pawnKey];
        }

        public bool FreeWillCanChange(Pawn pawn)
        {
            try
            {
                if (pawn == null)
                {
                    return false;
                }
                if (pawn.Ideo == null)
                {
                    return true;
                }
                if (!pawn.IsColonistPlayerControlled || pawn.IsSlaveOfColony)
                {
                    return false;
                }
                var canChange = !pawn.Ideo.HasPrecept(freeWillMandatory) && !pawn.Ideo.HasPrecept(freeWillProhibited);
                if (!canChange)
                {
                    EnsureFreeWillStatusIsCorrect(pawn);
                }
                return canChange;
            }
            catch (System.NullReferenceException)
            {
                Log.ErrorOnce("Free Will: could not check if free will can be changed: null reference", 48620057);
                return false;
            }
            catch (System.Exception)
            {
                Log.ErrorOnce("Free Will: could not check if free will can be changed", 48620057);
                return false;
            }
        }

        public void EnsureFreeWillStatusIsCorrect(Pawn pawn)
        {
            try
            {
                if (pawn == null)
                {
                    return;
                }
                if (pawn.Ideo == null)
                {
                    return;
                }
                // ensure it is set correctly
                if (pawn.Ideo.HasPrecept(freeWillMandatory) && !HasFreeWill(pawn))
                {
                    var ok = TryGiveFreeWill(pawn);
                    if (!ok)
                    {
                        Log.ErrorOnce("Free Will: could not give free will to mandatory ideology", 48887931);
                        return;
                    }
                }
                else if (pawn.Ideo.HasPrecept(freeWillProhibited) && HasFreeWill(pawn))
                {
                    var ok = TryRemoveFreeWill(pawn);
                    if (!ok)
                    {
                        Log.ErrorOnce("Free Will: could not remove free will from prohibited ideology", 48887932);
                    }
                }
            }
            catch (System.NullReferenceException)
            {
                Log.ErrorOnce("Free Will: could not check free will status: null reference", 48887933);
            }
            catch (System.Exception)
            {
                Log.ErrorOnce("Free Will: could not check free will status", 48887934);
            }
        }

        public bool TryGiveFreeWill(Pawn pawn)
        {
            if (pawn?.Ideo == null)
            {
                return false;
            }
            if (pawn.Ideo.HasPrecept(freeWillProhibited))
            {
                return false;
            }
            this.freePawns = this.freePawns ?? new Dictionary<string, bool> { };
            this.freePawns[pawn.GetUniqueLoadID()] = true;
            return true;
        }

        public bool TryRemoveFreeWill(Pawn pawn)
        {
            if (pawn?.Ideo == null)
            {
                return false;
            }
            if (!pawn.IsSlaveOfColony && pawn.Ideo.HasPrecept(freeWillMandatory))
            {
                return false;
            }
            this.freePawns = this.freePawns ?? new Dictionary<string, bool> { };
            freePawns[pawn.GetUniqueLoadID()] = false;
            return true;
        }

        public void FreeWillOverride(Pawn pawn)
        {
            try
            {
                if (pawn == null)
                {
                    return;
                }
                var pawnKey = pawn.GetUniqueLoadID();
                if (freeWillOverride == null)
                {
                    freeWillOverride = new Dictionary<string, int> { };
                }
                freeWillOverride[pawnKey] = Find.TickManager.TicksGame;
            }
            catch (System.Exception)
            {
                Log.ErrorOnce("Free Will: error during FreeWillOverride", 1454146);
            }
        }

        public int FreeWillTicks(Pawn pawn)
        {
            try
            {
                if (pawn == null)
                {
                    return 0;
                }
                var pawnKey = pawn.GetUniqueLoadID();
                if (freeWillOverride == null)
                {
                    freeWillOverride = new Dictionary<string, int> { };
                }
                if (!freeWillOverride.ContainsKey(pawnKey))
                {
                    freeWillOverride[pawnKey] = Find.TickManager.TicksGame;
                    return 0;
                }
                return Find.TickManager.TicksGame - freeWillOverride[pawnKey];
            }
            catch (System.Exception err)
            {
                Log.ErrorOnce("Free Will: error during FreeWillTicks: " + err.ToString(), 1454147);
                return 0;
            }
        }

        public bool HasInterestsFramework()
        {
            if (checkedForInterestsMod)
            {
                return (interestsStrings != null);
            }

            checkedForInterestsMod = true;
            if (LoadedModManager.RunningModsListForReading.Any(x => x.Name == "[D] Interests Framework"))
            {
                var interestsBaseT = AccessTools.TypeByName("DInterests.InterestBase");
                if (interestsBaseT == null)
                {
                    Log.ErrorOnce("Free Will: did not find interestsBase", 118574562);
                    return false;
                }

                var interestList = AccessTools.Field(interestsBaseT, "interestList").GetValue(interestsBaseT);
                if (interestList == null)
                {
                    Log.ErrorOnce("Free Will: did not find interest list", 118574563);
                    return false;
                }

                var interestListT = AccessTools.TypeByName("DInterests.InterestList");
                if (interestListT == null)
                {
                    Log.ErrorOnce("Free Will: could not find interest list type", 118574564);
                    return false;
                }

                var countMethod = AccessTools.Method(interestListT.BaseType, "get_Count", null);
                if (countMethod == null)
                {
                    Log.ErrorOnce("Free Will: could not find count method", 118574565);
                    return false;
                }

                var count = countMethod.Invoke(interestList, null);
                if (count == null)
                {
                    Log.ErrorOnce("Free Will: could not get count", 118574566);
                    return false;
                }

                var interestDefT = AccessTools.TypeByName("DInterests.InterestDef");
                if (interestDefT == null)
                {
                    Log.ErrorOnce("could not find interest def type", 118574567);
                    return false;
                }

                var getItem = AccessTools.Method(interestListT.BaseType, "get_Item");
                if (getItem == null)
                {
                    Log.ErrorOnce("Free Will: coud not find get item method", 118574568);
                    return false;
                }

                var defNameField = AccessTools.Field(interestDefT, "defName");
                if (defNameField == null)
                {
                    Log.ErrorOnce("Free Will: could not get defName field", 118574569);
                    return false;
                }

                interestsStrings = new List<string> { };
                for (int i = 0; i < (int)count; i++)
                {
                    var interestDef = getItem.Invoke(interestList, new object[] { i });
                    if (interestDef == null)
                    {
                        Log.ErrorOnce("Free Will: could not find interest def", 118574570);
                        return false;
                    }
                    var defName = defNameField.GetValue(interestDef);
                    if (defName == null)
                    {
                        Log.ErrorOnce("Free Will: could not get defname", 118574571);
                        return false;
                    }
                    interestsStrings.Add((string)defName);
                }
                return true;
            }
            return false;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref this.freePawns, "FreeWillFreePawns", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref this.freeWillOverride, "FreeWillFreeWillOverride", LookMode.Value, LookMode.Value);
        }
    }
}
