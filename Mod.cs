using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace FreeWill
{
    public class Mod : Verse.Mod
    {
        private Vector2 pos;
        private float height;

        public Mod(ModContentPack content) : base(content)
        {
            Harmony harmony = new Harmony("freemapa.freewill");
            Assembly assembly = Assembly.GetExecutingAssembly();
            harmony.PatchAll(assembly);
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);

            var view = new Rect(15.0f, 0, inRect.width - 30.0f, inRect.height);
            var ls = new Listing_Standard();

            view.height = height;
            Widgets.BeginScrollView(inRect, ref pos, view);
            GUI.BeginGroup(view);
            view.height = 9999.0f;
            ls.Begin(new Rect(10, 10, view.width - 40, view.height - 10));

            ls.TextEntry("This is the Free Will mod settings.");

            // save height of ls rectangle
            height = ls.GetRect(0).yMax + 20.0f;

            ls.End();
            GUI.EndGroup();
            Widgets.EndScrollView();
        }

        public override string SettingsCategory()
        {
            return "FreeWillSettingsCategory".TranslateSimple();
        }
    }
}