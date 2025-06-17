using RimWorld;
using Verse;

namespace FreeWill.Core.Interfaces
{
    // Placeholder for Verse.Map
    public interface IMap
    {
        MapPawns mapPawns { get; }
        AreaManager areaManager { get; }
        ListerThings listerThings { get; }
        DesignationManager designationManager { get; }

        T GetComponent<T>() where T : MapComponent;
    }
}
