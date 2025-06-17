# Free Will

Colonists have free will! Adds a precept that influences how willing colonists are to do what they're told.

## Prerequisites

- RimWorld must be installed. By default the build expects the game in `C:\Program Files (x86)\Steam\steamapps\common\RimWorld`. Set the `RIMWORLD_DIR` environment variable if your copy lives elsewhere.
- A .NET SDK capable of targeting **.NET Framework 4.7.2** is required.

## Building the DLL

Use the `dotnet` CLI in the repository root. The build process reads `RIMWORLD_DIR` and copies the result into `Mods/FreeWill` inside the game directory.

```bash
RIMWORLD_DIR="C:\\Games\\RimWorld" dotnet build -c Stable
```

Use `-c Unstable` for a debug build.

## Key files

- `FreeWill_Mod.cs` &ndash; entry point for the mod and Harmony patches.
- `FreeWill_MapComponent.cs` &ndash; `MapComponent` that tracks colony state and updates work priorities.

See [docs/architecture.md](docs/architecture.md) for an overview of how these pieces work together.

For a detailed overview of features see [description.txt](description.txt).
This file is also the description shown on the Steam Workshop. Update it for each
release and follow the steps in [docs/workshop.md](docs/workshop.md) to keep the
page in sync.
