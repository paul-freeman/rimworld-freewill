using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace FreeWill
{
    public class FreeWill_Mod : Mod
    {
        public FreeWill_Mod(ModContentPack content) : base(content)
        {
            var harmonyInstance = new Harmony("freemapa.freewill");
            Assembly assembly = Assembly.GetExecutingAssembly();
            harmonyInstance.PatchAll(assembly);
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
        /// <summary>
        /// Harmony prefix patch for the PawnColumnWorker_WorkPriority.DoCell method.
        /// Determines whether the original method should be executed, based on the Pawn's FreeWill status and other conditions.
        /// </summary>
        /// <param name="rect">The Rect object representing the cell in the work priority column.</param>
        /// <param name="pawn">The Pawn object for which the work priority cell is being rendered.</param>
        /// <param name="table">The PawnTable object containing the Pawn and associated work priority column.</param>
        /// <param name="__instance">The instance of the PawnColumnWorker_WorkPriority class being patched.</param>
        /// <returns>True if the original DoCell method should be executed, otherwise false.</returns>
        static bool Prefix(Rect rect, Pawn pawn, PawnTable table, PawnColumnWorker_WorkPriority __instance)
        {
            if (pawn.Dead
                || pawn.workSettings == null
                || !pawn.workSettings.EverWork
                || !FreeWillUtility.GetWorldComponent().HasFreeWill(pawn, pawn.GetUniqueLoadID()))
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
            if (!worldComp.HasFreeWill(___pawn, ___pawn.GetUniqueLoadID()))
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