# Mod Settings

This page lists the options available in `FreeWill_ModSettings`. Each setting corresponds to a string key in `Languages/English/Keyed/Settings.xml` so translators can locate the correct entry.

## Consideration sliders

These settings control how strongly different factors influence automatic work priorities. All values range from `0` to `10` and have a default of `1` unless noted.

| Field | Translation key | Default | Effect |
|-------|-----------------|---------|--------|
| `ConsiderMovementSpeed` | `FreeWillConsiderMovementSpeed` / `FreeWillConsiderMovementSpeedLong` | `1.0` | Weight applied when pawns choose jobs that suit their movement speed. |
| `ConsiderPassions` | `FreeWillConsiderPassions` / `FreeWillConsiderPassionsLong` | `1.0` | Encourages pawns to do work they are passionate about. |
| `ConsiderBeauty` | `FreeWillConsiderBeauty` / `FreeWillConsiderBeautyLong` | `1.0` | Higher values make pawns prefer beautiful areas. |
| `ConsiderBestAtDoing` | `FreeWillConsiderBestAtDoing` / `FreeWillConsiderBestAtDoingLong` | `0.0` | Bonus when a pawn is the best in the colony at a task. |
| `ConsiderFoodPoisoning` | `FreeWillConsiderFoodPoisoning` / `FreeWillConsiderFoodPoisoningLong` | `1.0` | Pawns react to cooking areas with a poisoning risk. |
| `ConsiderLowFood` | `FreeWillConsiderLowFood` / `FreeWillConsiderLowFoodLong` | `1.0` | Increases hunting, farming and hauling when food is scarce. |
| `ConsiderWeaponRange` | `FreeWillConsiderWeaponRange` / `FreeWillConsiderWeaponRangeLong` | `1.0` | Discourages hunting with short ranged weapons. |
| `ConsiderOwnRoom` | `FreeWillConsiderOwnRoom` / `FreeWillConsiderOwnRoomLong` | `1.0` | Encourages cleaning when in a pawn's own room. |
| `ConsiderPlantsBlighted` | `FreeWillConsiderPlantsBlighted` / `FreeWillConsiderPlantsBlightedLong` | `1.0` | Makes cutting blighted plants a higher priority. |
| `ConsiderGauranlenPruning` | `FreeWillConsiderGauranlenPruning` / `FreeWillConsiderGauranlenPruningLong` | `1.0` | Preference for keeping a linked Gauranlen tree pruned. |

## Toggle options

| Field | Translation key | Default | Effect |
|-------|-----------------|---------|--------|
| `ConsiderBrawlersNotHunting` | `FreeWillConsiderBrawlersNotHunting` / `FreeWillConsiderBrawlersNotHuntingLong` | `true` | Brawlers avoid hunting jobs. |
| `ConsiderHasHuntingWeapon` | `FreeWillConsiderHasHuntingWeapon` / `FreeWillConsiderHasHuntingWeaponLong` | `true` | Hunting is avoided when a pawn lacks a suitable weapon. |

## Global work type adjustments

The `globalWorkAdjustments` dictionary stores per work type multipliers in the mod settings window. The controls use the keys `FreeWillWorkTypeAdjustment`, `FreeWillWorkTypeAdjustments`, `FreeWillResetGlobalSlidersLabel` and `FreeWillResetGlobalSlidersButtonLabel`.

Each entry ranges from `-1` to `1` and is applied after all other calculations to fine tune specific work types.
