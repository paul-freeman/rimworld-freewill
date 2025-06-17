using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using FreeWill.Core.Interfaces;

namespace FreeWill
{
    /// <summary>
    /// World component that tracks which pawns have free will and integrates
    /// ideology related behaviour.
    /// </summary>
    public class FreeWill_WorldComponent : WorldComponent, IWorldComponent
    {
        private readonly FreeWill_Mod mod = LoadedModManager.GetMod<FreeWill_Mod>();
        public FreeWill_ModSettings Settings => mod.GetSettings<FreeWill_ModSettings>();

        private Dictionary<string, bool> freePawns;
        private Dictionary<string, int> freeWillOverride;

        private readonly PreceptDef freeWillProhibited;
        private readonly PreceptDef freeWillDisapproved;
        private readonly PreceptDef freeWillFlexible;
        private readonly PreceptDef freeWillPreferred;
        private readonly PreceptDef freeWillMandatory;

        private bool checkedForInterestsMod;
        public List<string> interestsStrings;

        /// <summary>
        /// Initializes a new instance of the <see cref="FreeWill_WorldComponent"/> class.
        /// </summary>
        /// <param name="world">World instance the component belongs to.</param>
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

        /// <summary>
        /// Executes world-level checks each game tick.
        /// </summary>
        public override void WorldComponentTick()
        {
            try
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
                        Log.Message($"Free Will: adding free will precept, \"flexible\", to {ideo.name} ideology.");
                        ideo.AddPrecept(PreceptMaker.MakePrecept(freeWillFlexible), init: false);
                    }
                }
                freePawns = freePawns ?? new Dictionary<string, bool> { };
            }
            catch
            {
                if (Prefs.DevMode)
                {
                    Log.Error("Free Will: could not perform free will world tick");
                }
                throw;
            }
            base.WorldComponentTick();
        }

        /// <summary>
        /// Finalizes initialization after loading a save or starting a new game.
        /// </summary>
        /// <param name="fromLoad">Indicates whether the component was loaded from a save.</param>
        public override void FinalizeInit(bool fromLoad)
        {
            base.FinalizeInit(fromLoad);
            FreeWillUtility.UpdateWorldComponent(this);
        }

        /// <summary>
        /// Determines whether a pawn currently has free will enabled.
        /// </summary>
        /// <param name="pawn">Pawn to check.</param>
        /// <param name="pawnKey">Unique identifier for the pawn.</param>
        /// <returns>True if the pawn has free will.</returns>
        public bool HasFreeWill(Pawn pawn, string pawnKey)
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

            freePawns = freePawns ?? new Dictionary<string, bool> { };
            if (!freePawns.ContainsKey(pawnKey))
            {
                if (pawn.Ideo.HasPrecept(freeWillDisapproved) || pawn.Ideo.HasPrecept(freeWillProhibited))
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

        bool IWorldComponent.HasFreeWill(IPawn pawn, string pawnKey)
        {
            return HasFreeWill((Pawn)pawn, pawnKey);
        }

        /// <summary>
        /// Checks whether the pawn's free will status can be changed by the player.
        /// </summary>
        /// <param name="pawn">Pawn to evaluate.</param>
        /// <param name="pawnKey">Unique identifier for the pawn.</param>
        /// <returns>True if the status can be modified.</returns>
        public bool FreeWillCanChange(Pawn pawn, string pawnKey)
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
                bool canChange = !pawn.Ideo.HasPrecept(freeWillMandatory) && !pawn.Ideo.HasPrecept(freeWillProhibited);
                if (!canChange)
                {
                    EnsureFreeWillStatusIsCorrect(pawn, pawnKey);
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

        bool IWorldComponent.FreeWillCanChange(IPawn pawn, string pawnKey)
        {
            return FreeWillCanChange((Pawn)pawn, pawnKey);
        }

        /// <summary>
        /// Ensures that a pawn's free will status matches their ideology rules.
        /// </summary>
        /// <param name="pawn">Pawn to evaluate.</param>
        /// <param name="pawnKey">Unique identifier for the pawn.</param>
        public void EnsureFreeWillStatusIsCorrect(Pawn pawn, string pawnKey)
        {
            try
            {
                if (pawn?.Ideo == null)
                {
                    return;
                }
                // ensure it is set correctly
                if (pawn.Ideo.HasPrecept(freeWillMandatory) && !HasFreeWill(pawn, pawnKey))
                {
                    bool ok = TryGiveFreeWill(pawn);
                    if (!ok)
                    {
                        Log.ErrorOnce("Free Will: could not give free will to mandatory ideology", 48887931);
                        return;
                    }
                }
                else if (pawn.Ideo.HasPrecept(freeWillProhibited) && HasFreeWill(pawn, pawnKey))
                {
                    bool ok = TryRemoveFreeWill(pawn);
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

        void IWorldComponent.EnsureFreeWillStatusIsCorrect(IPawn pawn, string pawnKey)
        {
            EnsureFreeWillStatusIsCorrect((Pawn)pawn, pawnKey);
        }

        /// <summary>
        /// Attempts to grant free will to a pawn.
        /// </summary>
        /// <param name="pawn">Pawn to modify.</param>
        /// <returns>True if free will was granted.</returns>
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
            freePawns = freePawns ?? new Dictionary<string, bool> { };
            freePawns[pawn.GetUniqueLoadID()] = true;
            return true;
        }

        bool IWorldComponent.TryGiveFreeWill(IPawn pawn)
        {
            return TryGiveFreeWill((Pawn)pawn);
        }

        /// <summary>
        /// Attempts to remove free will from a pawn.
        /// </summary>
        /// <param name="pawn">Pawn to modify.</param>
        /// <returns>True if free will was removed.</returns>
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
            freePawns = freePawns ?? new Dictionary<string, bool> { };
            freePawns[pawn.GetUniqueLoadID()] = false;
            return true;
        }

        bool IWorldComponent.TryRemoveFreeWill(IPawn pawn)
        {
            return TryRemoveFreeWill((Pawn)pawn);
        }

        /// <summary>
        /// Marks that the player manually changed this pawn's work priorities.
        /// </summary>
        /// <param name="pawn">Pawn that was overridden.</param>
        public void FreeWillOverride(Pawn pawn)
        {
            try
            {
                if (pawn == null)
                {
                    return;
                }
                string pawnKey = pawn.GetUniqueLoadID();
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

        void IWorldComponent.FreeWillOverride(IPawn pawn)
        {
            FreeWillOverride((Pawn)pawn);
        }

        /// <summary>
        /// Gets the number of ticks since the player's last override for this pawn.
        /// </summary>
        /// <param name="pawn">Pawn to query.</param>
        /// <returns>Ticks since the last manual override.</returns>
        public int FreeWillTicks(Pawn pawn)
        {
            try
            {
                if (pawn == null)
                {
                    return 0;
                }
                string pawnKey = pawn.GetUniqueLoadID();
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

        int IWorldComponent.FreeWillTicks(IPawn pawn)
        {
            return FreeWillTicks((Pawn)pawn);
        }

        /// <summary>
        /// Detects if the Interests Framework mod is installed and caches its data.
        /// </summary>
        /// <returns>True if the Interests Framework is available.</returns>
        public bool HasInterestsFramework()
        {
            if (checkedForInterestsMod)
            {
                return interestsStrings != null;
            }

            checkedForInterestsMod = true;
            if (LoadedModManager.RunningModsListForReading.Any(x => x.Name == "[D] Interests Framework"))
            {
                System.Type interestsBaseT = AccessTools.TypeByName("DInterests.InterestBase");
                if (interestsBaseT == null)
                {
                    Log.ErrorOnce("Free Will: did not find interestsBase", 118574562);
                    return false;
                }

                object interestList = AccessTools.Field(interestsBaseT, "interestList").GetValue(interestsBaseT);
                if (interestList == null)
                {
                    Log.ErrorOnce("Free Will: did not find interest list", 118574563);
                    return false;
                }

                System.Type interestListT = AccessTools.TypeByName("DInterests.InterestList");
                if (interestListT == null)
                {
                    Log.ErrorOnce("Free Will: could not find interest list type", 118574564);
                    return false;
                }

                System.Reflection.MethodInfo countMethod = AccessTools.Method(interestListT.BaseType, "get_Count", null);
                if (countMethod == null)
                {
                    Log.ErrorOnce("Free Will: could not find count method", 118574565);
                    return false;
                }

                object count = countMethod.Invoke(interestList, null);
                if (count == null)
                {
                    Log.ErrorOnce("Free Will: could not get count", 118574566);
                    return false;
                }

                System.Type interestDefT = AccessTools.TypeByName("DInterests.InterestDef");
                if (interestDefT == null)
                {
                    Log.ErrorOnce("Free Will: could not find interest def type", 118574567);
                    return false;
                }

                System.Reflection.MethodInfo getItem = AccessTools.Method(interestListT.BaseType, "get_Item");
                if (getItem == null)
                {
                    Log.ErrorOnce("Free Will: coud not find get item method", 118574568);
                    return false;
                }

                System.Reflection.FieldInfo defNameField = AccessTools.Field(interestDefT, "defName");
                if (defNameField == null)
                {
                    Log.ErrorOnce("Free Will: could not get defName field", 118574569);
                    return false;
                }

                interestsStrings = new List<string> { };
                for (int i = 0; i < (int)count; i++)
                {
                    object interestDef = getItem.Invoke(interestList, new object[] { i });
                    if (interestDef == null)
                    {
                        Log.ErrorOnce("Free Will: could not find interest def", 118574570);
                        return false;
                    }
                    object defName = defNameField.GetValue(interestDef);
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

        /// <summary>
        /// Saves and loads the component's persistent data.
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref freePawns, "FreeWillFreePawns", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref freeWillOverride, "FreeWillFreeWillOverride", LookMode.Value, LookMode.Value);
        }
    }
}
