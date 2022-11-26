using System.Linq;
using UnityEngine;
using Verse;

namespace FreeWill
{
    [StaticConstructorOnStartup]
    public static class Resources
    {
        public static readonly Texture2D FreeWillOverlay;

        static Resources()
        {
            FreeWillOverlay = ContentFinder<Texture2D>.Get("UI/Icons/freewill-overlay");
        }
    }
}