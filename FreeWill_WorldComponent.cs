using System.Collections.Generic;
using System.Reflection;
using Verse;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using HarmonyLib;

namespace FreeWill
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

        private Dictionary<string, bool> freePawns = new Dictionary<string, bool>();

        private PreceptDef freeWillProhibited;
        private PreceptDef freeWillDisapproved;
        private PreceptDef freeWillPreferred;
        private PreceptDef freeWillMandatory;

        private bool checkedForInterestsMod;
        public List<string> interestsStrings;

        public FreeWill_WorldComponent(World world) : base(world)
        {
            freePawns = new Dictionary<string, bool> { };

            freeWillProhibited = DefDatabase<PreceptDef>.GetNamed("Free_Will_Prohibited");
            freeWillDisapproved = DefDatabase<PreceptDef>.GetNamed("Free_Will_Disapproved");
            freeWillPreferred = DefDatabase<PreceptDef>.GetNamed("Free_Will_Preferred");
            freeWillMandatory = DefDatabase<PreceptDef>.GetNamed("Free_Will_Mandatory");

            checkedForInterestsMod = false;
            interestsStrings = new List<string> { };
        }

        public bool HasFreeWill(Pawn pawn)
        {
            var pawnKey = pawn.GetUniqueLoadID();
            if (!freePawns.ContainsKey(pawnKey))
            {
                if (pawn.Ideo.HasPrecept(freeWillProhibited) || pawn.Ideo.HasPrecept(freeWillDisapproved))
                {
                    freePawns[pawnKey] = false;
                    return false;
                }
                else
                {
                    freePawns[pawnKey] = true;
                    return true;
                }
            }
            return freePawns[pawnKey];
        }

        public bool FreeWillCanChange(Pawn pawn)
        {
            var canChange = pawn.Ideo.HasPrecept(freeWillPreferred) || pawn.Ideo.HasPrecept(freeWillDisapproved);
            if (!canChange)
            {
                CheckFreeWillStatus(pawn);
            }
            return canChange;
        }

        public void CheckFreeWillStatus(Pawn pawn)
        {
            // ensure it is set correctly
            if (pawn.Ideo.HasPrecept(freeWillMandatory) && !HasFreeWill(pawn))
            {
                var ok = TryGiveFreeWill(pawn);
                if (!ok)
                {
                    Log.Error("could not give free will to mandatory ideology");
                }
            }
            else if (pawn.Ideo.HasPrecept(freeWillProhibited) && HasFreeWill(pawn))
            {
                var ok = TryRemoveFreeWill(pawn);
                if (!ok)
                {
                    Log.Error("could not give free will to mandatory ideology");
                }
            }
        }

        public bool TryGiveFreeWill(Pawn pawn)
        {
            var pawnKey = pawn.GetUniqueLoadID();
            if (pawn.Ideo.HasPrecept(freeWillProhibited))
            {
                return false;
            }
            freePawns[pawnKey] = true;
            return true;
        }

        public bool TryRemoveFreeWill(Pawn pawn)
        {
            var pawnKey = pawn.GetUniqueLoadID();
            if (pawn.Ideo.HasPrecept(freeWillMandatory))
            {
                return false;
            }
            freePawns[pawnKey] = false;
            return true;
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
                Log.Message("found \"[D] Interests Framework\"");
                var interestsBaseT = AccessTools.TypeByName("DInterests.InterestBase");
                if (interestsBaseT == null)
                {
                    Log.Error("did not find interestsBase");
                    return false;
                }

                var interestList = AccessTools.Field(interestsBaseT, "interestList").GetValue(interestsBaseT);
                if (interestList == null)
                {
                    Log.Error("did not find interest list");
                    return false;
                }

                var interestListT = AccessTools.TypeByName("DInterests.InterestList");
                if (interestListT == null)
                {
                    Log.Error("could not find interest list type");
                    return false;
                }

                var countMethod = AccessTools.Method(interestListT.BaseType, "get_Count", null);
                if (countMethod == null)
                {
                    Log.Error("could not find count method");
                    return false;
                }

                var count = countMethod.Invoke(interestList, null);
                if (count == null)
                {
                    Log.Error("could not get count");
                    return false;
                }

                var interestDefT = AccessTools.TypeByName("DInterests.InterestDef");
                if (interestDefT == null)
                {
                    Log.Error("could not find interest def type");
                    return false;
                }

                var getItem = AccessTools.Method(interestListT.BaseType, "get_Item");
                if (getItem == null)
                {
                    Log.Error("coud not find get item method");
                    return false;
                }

                var defNameField = AccessTools.Field(interestDefT, "defName");
                if (defNameField == null)
                {
                    Log.Error("could not get defName field");
                    return false;
                }

                interestsStrings = new List<string> { };
                for (int i = 0; i < (int)count; i++)
                {
                    var interestDef = getItem.Invoke(interestList, new object[] { i });
                    if (interestDef == null)
                    {
                        Log.Error("could not find interest def");
                        return false;
                    }
                    var defName = defNameField.GetValue(interestDef);
                    if (defName == null)
                    {
                        Log.Error("could not get defname");
                        return false;
                    }
                    Log.Message("supporting interest " + (string)defName);
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
        }
    }
}