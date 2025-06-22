using Verse;

namespace FreeWill
{
    /// <summary>
    /// Strategy for research work type priority calculation.
    /// Research involves advancing technology and unlocking new capabilities for the colony.
    /// Priority significantly decreases during food shortages as research is intellectual luxury.
    /// Considers intellectual skills and inspiration for breakthrough research discoveries.
    /// Research is long-term investment that becomes less critical during survival crises.
    /// </summary>
    public class ResearchingStrategy : BaseWorkTypeStrategy
    {
        public override WorkTypeDef WorkType => GetWorkTypeDef("Research");

        public override Priority CalculatePriority(Priority priority)
        {
            return priority
                .ConsiderRelevantSkills()
                .ConsiderIsAnyoneElseDoing()
                .ConsiderBestAtDoing()
                .ConsiderBeautyExpectations()
                .ConsiderPassion()
                .ConsiderThoughts()
                .ConsiderInspiration() // Inspiration can lead to research breakthroughs
                .ConsiderLowFood(-0.4f) // Research has largest penalty when food is scarce (intellectual luxury)
                .ConsiderColonistLeftUnburied()
                .ConsiderHealth()
                .ConsiderAteRawFood()
                .ConsiderBored()
                .ConsiderFire()
                .ConsiderBuildingImmunity()
                .ConsiderColonistsNeedingTreatment()
                .ConsiderDownedColonists()
                .ConsiderColonyPolicy();
        }
    }
}