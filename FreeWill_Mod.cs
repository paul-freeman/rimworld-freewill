using System.Collections.Generic;
using System.Reflection;
using System;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

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

    [HarmonyPatch(typeof(PawnColumnWorker_WorkPriority), "DoCell")]
    class PawnColumnWorker_WorkPriority_Patch
    {
        static bool Prefix(Rect rect, Pawn pawn, PawnTable table, PawnColumnWorker_WorkPriority __instance)
        {
            if (!FreeWillUtility.GetWorldComponent().HasFreeWill(pawn))
            {
                return true;
            }
            if (pawn.Dead || pawn.workSettings == null || !pawn.workSettings.EverWork)
            {
                return true;
            }
            FreeWillUtility.DoCell(rect, pawn, table, __instance);
            return false;
        }
    }

    [HarmonyPatch(typeof(Pawn_JobTracker), "StartJob")]
    public class FreeWillOverride
    {
        static void Postfix(Pawn ___pawn, Job __0)
        {
            if (___pawn == null)
            {
                return;
            }
            if (!___pawn.IsColonistPlayerControlled)
            {
                return;
            }
            var worldComp = Find.World.GetComponent<FreeWill_WorldComponent>();
            if (!worldComp.HasFreeWill(___pawn))
            {
                return;
            }
            if (!__0.playerForced)
            {
                return;
            }
            worldComp.FreeWillOverride(___pawn);
        }
    }
}