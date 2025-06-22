# FreeWill Strategy System Documentation

This document provides a comprehensive guide to understanding the FreeWill mod's strategy system, which determines how colonists prioritize different types of work based on their skills, colony conditions, and various environmental factors.

## Overview

The strategy system replaces RimWorld's simple work priority numbers with dynamic, context-aware calculations. Each work type has its own strategy that considers multiple factors to determine the most appropriate priority for each colonist in real-time.

### Key Benefits
- **Dynamic Priorities**: Work priorities adjust automatically based on colony needs
- **Context Awareness**: Emergency situations (fires, medical crises, food shortages) automatically influence priorities
- **Skill Consideration**: Colonists naturally gravitate toward work they're good at
- **Passion Integration**: Passionate colonists get priority boosts for work they enjoy
- **Smart Resource Management**: Priorities adjust during resource shortages or abundance

## How Strategies Work

Each work type strategy follows this general pattern:

1. **Base Priority**: Set an initial priority value (typically 0.3-0.7)
2. **Skill Considerations**: Adjust based on relevant skills and passion
3. **Environmental Factors**: Modify based on colony conditions (food, emergencies, etc.)
4. **Health & Mood**: Account for colonist physical and mental state
5. **Final Adjustments**: Apply colony policy and situational modifiers

### Priority Calculation Chain

Each strategy uses a fluent API pattern where methods can be chained together:

```csharp
return priority
    .ConsiderRelevantSkills()    // Higher for skilled colonists
    .ConsiderPassion()           // Bonus for passionate work
    .ConsiderLowFood(0.3f)       // Increase/decrease during food shortage
    .ConsiderFire()              // Emergency boost during fires
    .ConsiderHealth()            // Reduce if injured/sick
    .ConsiderColonyPolicy();     // Apply final policy adjustments
```

## Work Type Strategies

### Emergency & Medical Strategies

#### FirefighterStrategy
- **Purpose**: Handle fire emergencies with absolute priority
- **Key Factors**: Fire always takes precedence over other work
- **Priority Adjustments**: Maximum priority during fire emergencies

#### DoctorStrategy  
- **Purpose**: Provide medical treatment to injured colonists and animals
- **Key Factors**: Medical skill, number of patients needing treatment
- **Priority Adjustments**: 
  - Increases when colonists need medical attention
  - Higher priority for skilled doctors
  - Considers medical supplies availability

#### PatientStrategy
- **Purpose**: Handle colonists who need to rest and recover
- **Key Factors**: Health status, injury severity
- **Priority Adjustments**: Only applies to injured/sick colonists

#### PatientBedRestStrategy
- **Purpose**: Manage bed rest for recovery
- **Key Factors**: Bed availability, recovery needs
- **Priority Adjustments**: Higher during medical crises

### Food Production Strategies

#### CookingStrategy
- **Purpose**: Prepare meals for colonist nutrition and morale
- **Key Factors**: Cooking skill, food shortage status, meal quality expectations
- **Priority Adjustments**:
  - **Food Shortage (+0.3)**: Cooking becomes critical when food is scarce
  - **Cooking Skill**: Higher priority for skilled cooks
  - **Beauty Expectations**: Quality meals matter more for demanding colonists

#### HuntingStrategy
- **Purpose**: Hunt wild animals for meat and leather
- **Key Factors**: Shooting skill, weapon availability, food needs
- **Priority Adjustments**:
  - **Food Shortage (+0.3)**: Hunting provides essential meat when food is scarce
  - **Weapon Requirements**: Must have appropriate hunting weapon
  - **Brawler Penalty**: Brawlers are less effective hunters
  - **Movement Speed**: Important for tracking animals

#### GrowingStrategy
- **Purpose**: Plant, tend, and harvest crops for long-term food security
- **Key Factors**: Plants skill, growing season, food shortage
- **Priority Adjustments**:
  - **Food Shortage (+0.3)**: Growing becomes critical for survival
  - **Seasonal Considerations**: Higher priority during growing seasons
  - **Skill Bonus**: Experienced growers get priority

### Production & Crafting Strategies

#### ConstructionStrategy
- **Purpose**: Build structures, walls, furniture, and infrastructure
- **Key Factors**: Construction skill, building needs, animal pen requirements
- **Priority Adjustments**:
  - **Food Shortage (-0.3)**: Construction becomes less important during survival crises
  - **Animal Pen Needs**: Special priority for animal-related construction

#### MiningStrategy
- **Purpose**: Extract valuable resources from stone and mineral deposits
- **Key Factors**: Mining skill, resource needs
- **Priority Adjustments**:
  - **Food Shortage (-0.3)**: Mining becomes less critical during survival situations
  - **Resource Demands**: Higher priority when specific materials are needed

#### SmithingStrategy
- **Purpose**: Create metal weapons, tools, and mechanical components
- **Key Factors**: Crafting skill, mechanitor needs, weapon demands
- **Priority Adjustments**:
  - **Food Shortage (-0.3)**: Smithing becomes less important during survival crises
  - **Mech Gestators**: Special priority for mechanitor-related tasks
  - **Beauty Expectations**: Quality items matter for demanding colonists

### Creative & Luxury Strategies

#### ArtStrategy
- **Purpose**: Create sculptures and decorative items for colony beauty
- **Key Factors**: Artistic skill, beauty expectations, inspiration
- **Priority Adjustments**:
  - **Beauty Expectations**: Higher priority when colonists demand beautiful surroundings
  - **Inspiration Bonus**: Artists with inspiration get significant priority boost
  - **Food Shortage (-0.3)**: Art becomes luxury during survival situations

#### TailoringStrategy
- **Purpose**: Create clothing, armor, and textile items
- **Key Factors**: Crafting skill, clothing needs, weather conditions
- **Priority Adjustments**:
  - **Warm Clothes Need**: Emergency priority when colonists need cold protection
  - **Food Shortage (-0.3)**: Clothing production becomes less critical during food crises

#### CraftingStrategy
- **Purpose**: Create tools, furniture, and manufactured items
- **Key Factors**: Crafting skill, quality expectations
- **Priority Adjustments**:
  - **Beauty Expectations**: Quality crafted items improve colony aesthetics
  - **Food Shortage (-0.3)**: Crafting becomes less important during survival situations

### Maintenance & Support Strategies

#### HaulingStrategy
- **Purpose**: Transport items, resources, and materials around the colony
- **Key Factors**: Carrying capacity, movement speed, deteriorating items
- **Priority Adjustments**:
  - **Food Shortage (+0.2)**: Hauling food supplies becomes more important
  - **Deteriorating Items**: Emergency priority for items about to spoil
  - **Carrying Capacity**: More efficient haulers get priority
  - **Mech Haulers**: Considers if mechanical haulers are available

#### HaulingUrgentStrategy *(Modded Work Type)*
- **Purpose**: Handle time-critical transportation needs
- **Key Factors**: Same as regular hauling but with higher urgency
- **Priority Adjustments**:
  - **Food Shortage (+0.3)**: Higher increase than regular hauling during food crises
  - **Emergency Priority**: Critical for urgent supply movements

#### CleaningStrategy
- **Purpose**: Maintain hygiene and cleanliness to prevent disease
- **Key Factors**: Beauty expectations, food poisoning risk, home area
- **Priority Adjustments**:
  - **Beauty Expectations**: More important when colonists expect clean surroundings
  - **Food Shortage (-0.2)**: Cleaning becomes less critical during survival situations
  - **Food Poisoning Risk**: Higher priority in food preparation areas
  - **Home Area Only**: Only clean within designated home area for efficiency

#### ResearchingStrategy
- **Purpose**: Advance technology and unlock new capabilities
- **Key Factors**: Intellectual skill, research inspiration, long-term planning
- **Priority Adjustments**:
  - **Food Shortage (-0.4)**: Research has the largest penalty during food crises (intellectual luxury)
  - **Inspiration Bonus**: Research inspiration can lead to breakthroughs
  - **Long-term Investment**: Lower priority during immediate survival needs

### Specialized Care Strategies

#### ChildcareStrategy
- **Purpose**: Take care of children including feeding, playing, and supervision
- **Key Factors**: Social skills, childcare experience
- **Priority Adjustments**:
  - **Social Skills**: Higher priority for socially skilled colonists
  - **Child Needs**: Adjusts based on children's specific requirements

#### WardenStrategy
- **Purpose**: Manage prisoners including feeding, recruitment, and prison maintenance
- **Key Factors**: Social skills, prisoner needs
- **Priority Adjustments**:
  - **Food Shortage (-0.3)**: Prisoner care becomes less important when colonists are hungry
  - **Suppression Needs**: Priority adjusts based on prisoner unrest

#### HandlingStrategy
- **Purpose**: Train, tame, and manage animal behavior
- **Key Factors**: Animal skill, roaming animals, movement speed
- **Priority Adjustments**:
  - **Animals Roaming**: Higher priority when animals need immediate management
  - **Movement Speed**: Important for catching and handling animals effectively

## Priority Factors Explained

### Skill-Based Factors

- **ConsiderRelevantSkills()**: Higher priority for colonists with relevant skills
- **ConsiderBestAtDoing()**: Ensures the most skilled colonist gets highest priority
- **ConsiderPassion()**: Passionate colonists get significant priority boosts

### Environmental Factors

- **ConsiderLowFood(modifier)**: Adjusts priorities during food shortages
  - Positive values increase priority (food production work)
  - Negative values decrease priority (luxury work)
  - Typical range: -0.4 to +0.3
- **ConsiderFire()**: Emergency priority boost during fire situations
- **ConsiderThingsDeteriorating()**: Priority for hauling items about to spoil

### Health & Status Factors

- **ConsiderHealth()**: Reduces priority for injured or sick colonists
- **ConsiderInspiration()**: Bonus priority when colonist has relevant inspiration
- **ConsiderBored()**: Slight priority adjustments based on recreation needs
- **ConsiderBuildingImmunity()**: Considers colonists fighting diseases

### Colony Factors

- **ConsiderColonistsNeedingTreatment()**: Medical work gets priority when colonists are injured
- **ConsiderDownedColonists()**: Emergency adjustments when colonists are unconscious
- **ConsiderBeautyExpectations()**: Aesthetic work becomes more important for demanding colonists

## Understanding Priority Numbers

- **0.0**: Disabled/Never do this work
- **0.1-0.3**: Low priority, will do if nothing else available
- **0.4-0.6**: Normal priority, regular work assignment
- **0.7-0.9**: High priority, preferred work
- **1.0+**: Emergency priority, drop everything else

### Special Priority States

- **AlwaysDo**: Colonist will prioritize this work above all others
- **NeverDo**: Colonist will never perform this work
- **Disabled**: Work type is completely unavailable to this colonist

## Examples of Priority Calculations

### Example 1: Skilled Cook During Food Shortage
- Base Cooking Priority: 0.4
- Skill Bonus (Level 15): +0.3
- Passion Bonus (Major): +0.2
- Food Shortage Bonus: +0.3
- **Final Priority**: 1.2 (Emergency level)

### Example 2: Artist During Survival Crisis  
- Base Art Priority: 0.5
- Skill Bonus (Level 10): +0.2
- Beauty Expectations: +0.1
- Food Shortage Penalty: -0.3
- **Final Priority**: 0.5 (Reduced to normal level)

### Example 3: Doctor with Injured Colonists
- Base Doctor Priority: 0.6
- Medical Skill Bonus: +0.2
- Injured Colonists Bonus: +0.3
- **Final Priority**: 1.1 (Emergency level)

## Troubleshooting Strategy Issues

### Colonist Won't Do Expected Work
1. Check if work type is disabled in restrictions
2. Verify colonist has required skills/tools
3. Look for competing higher-priority work
4. Check for special conditions (home area restrictions, etc.)

### Wrong Colonist Doing Specialized Work
1. Verify skill levels - higher skilled colonists get priority
2. Check for passion differences
3. Look at current colony conditions affecting priorities
4. Consider if other colonists are busy with higher-priority tasks

### Priorities Don't Change During Emergencies
1. Ensure FreeWill mod is properly loaded and active
2. Check mod settings for strategy toggles
3. Verify emergency conditions are properly detected
4. Look for conflicting mods that might override priorities

## Modifying Strategy Behavior

Each strategy can be customized by modifying the priority calculation chain. Common modifications include:

### Adjusting Base Priorities
Change the initial `priority.Set()` value to make work types generally higher or lower priority.

### Modifying Factor Weights
Adjust the numerical values passed to consideration methods (e.g., change `ConsiderLowFood(0.3f)` to `ConsiderLowFood(0.5f)` for stronger food shortage effects).

### Adding New Considerations
Add new `.Consider*()` method calls to the chain to account for additional factors.

### Removing Considerations
Remove method calls from the chain to ignore certain factors in priority calculations.

---

*This documentation reflects the current state of the FreeWill strategy system. For technical implementation details, see the individual strategy class files and the Priority.cs calculation methods.* 