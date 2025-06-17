# Architecture Overview

This document provides a high level description of how the main pieces of the mod
work together.

## Components

### `FreeWill_WorldComponent`
A `WorldComponent` that stores global information about the free‑will status of
each pawn. It ensures every ideology has a Free Will precept and tracks whether
individual colonists have free will, including temporary overrides when the
player manually forces a job.

### `FreeWill_MapComponent`
A `MapComponent` used on each map. It gathers data about the colony (health
conditions, alerts, deteriorating items, etc.) and then iterates over every pawn
and `WorkTypeDef` to compute priorities. For each pawn it pulls the
`FreeWill_WorldComponent` to check if that pawn is free and then uses the
`Priority` class to calculate and apply the appropriate work priority in the
vanilla work settings.

### `Priority`
Represents the computed priority for a particular pawn and work type. It relies
on both components above to gather world and map state. The value it calculates
is converted into RimWorld's integer priority scale when `ApplyPriorityToGame`
is called by the map component.

## Harmony patches

`FreeWill_Mod` is the entry point of the mod. Its constructor creates a Harmony
instance and calls `PatchAll` on the executing assembly, applying all patch
classes.

Key patches include:

- `PawnColumnWorker_WorkPriority_Patch` – intercepts the work tab cell drawing to
  display the custom priority overlay when a pawn has free will.
- `FreeWillOverride` – listens for player‑forced jobs via `Pawn_JobTracker.StartJob`
  and records a temporary override so the pawn does not gain Free Will mood
  benefits.

## ITab and thought workers

`ITab_Pawn_FreeWill` adds a tab to every pawn inspector via the XML patch in
`Patches/FreeWillITabPatch.xml`. The tab shows the current priority analysis and
allows toggling a pawn's free‑will state.

The files under `ThoughtWorkers/` implement custom `ThoughtWorker_Precept`
classes referenced by `Defs/PreceptDefs/Precepts_FreeWill.xml`. They generate
mood thoughts based on whether a pawn is operating under free will or a strict
work schedule.
