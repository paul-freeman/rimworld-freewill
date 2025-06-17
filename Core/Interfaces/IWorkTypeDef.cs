using System.Collections.Generic;
using RimWorld;

namespace FreeWill.Core.Interfaces
{
    // Placeholder for RimWorld.WorkTypeDef
    public interface IWorkTypeDef
    {
        string defName { get; }
        string pawnLabel { get; }
        string description { get; }
        int index { get; }
        List<SkillDef> relevantSkills { get; }
        IEnumerable<IWorkTypeDef> requiredNonDisabledWorkTypes { get; }
        IEnumerable<IWorkTypeDef> requiredAnyNonDisabledWorkType { get; }
    }
}
