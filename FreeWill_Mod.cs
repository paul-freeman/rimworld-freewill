using System.Collections.Generic;
using System.Reflection;
using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace FreeWill
{
    public class FreeWill_Mod : Mod
    {
        public FreeWill_Mod(ModContentPack content) : base(content)
        {
            Harmony harmony = new Harmony("freemapa.freewill");
            Assembly assembly = Assembly.GetExecutingAssembly();
            harmony.PatchAll(assembly);
        }

        public override string SettingsCategory()
        {
            return "FreeWillSettingsCategory".TranslateSimple();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            this.GetSettings<FreeWill_ModSettings>().DoSettingsWindowContents(inRect);
        }
    }

    [HarmonyPatch(typeof(PawnTable), MethodType.Constructor, new Type[] { typeof(PawnTableDef), typeof(Func<IEnumerable<Pawn>>), typeof(int), typeof(int) })]
    public class RemoveFreePawns
    {
        static FreeWill_WorldComponent worldComp;
        static void Postfix(PawnTableDef def, ref Func<IEnumerable<Pawn>> ___pawnsGetter)
        {
            if (def != PawnTableDefOf.Work)
            {
                return;
            }
            if (worldComp == null)
            {
                worldComp = Find.World.GetComponent<FreeWill_WorldComponent>();
            }
            Func<IEnumerable<Pawn>> oldPawns = ___pawnsGetter;
            try
            {
                ___pawnsGetter = () =>
                {
                    try
                    {
                        List<Pawn> newPawns = new List<Pawn>();
                        foreach (Pawn pawn in oldPawns())
                        {
                            if (worldComp.HasFreeWill(pawn))
                            {
                                continue;
                            }
                            newPawns.Add(pawn);
                        }
                        return newPawns;
                    }
                    catch
                    {
                        Log.ErrorOnce("FreeWill mod could not remove free pawns from work tab - likely due to a mod conflict", 75674514);
                        return oldPawns();
                    }
                };
                Log.Message("FreeWill mod sucessfully patched the work tab");
            }
            catch
            {
                Log.ErrorOnce("FreeWill mod failed to patch the work tab", 654762154);
                ___pawnsGetter = oldPawns;
            }
        }
    }
}