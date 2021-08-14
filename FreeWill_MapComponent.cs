using System.Collections.Generic;
using System.Reflection;
using Verse;
using System.Linq;
using RimWorld;
using HarmonyLib;

namespace FreeWill
{
    public class FreeWill_MapComponent : MapComponent
    {
        public Dictionary<string, bool> pawnFree = new Dictionary<string, bool>();

        public FreeWill_MapComponent(Map map) : base(map)
        {
            this.pawnFree = new Dictionary<string, bool> { };
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref this.pawnFree, "PawnFree", LookMode.Value, LookMode.Value);
        }
    }
}
