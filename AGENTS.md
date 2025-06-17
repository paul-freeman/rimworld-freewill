### Background

This repo contains the code to create a mod for the game RimWorld. The mod is
called FreeWill and it automates the "work tab" in the game, which is the
interface for the player to set the priority for each colonist (or "pawn") to do
certain work types.

### Project details

The artifact of the build step is a DLL that is patched into the game when the
game starts. The binaries of the game are not available during development and
are only provided as part of the final build process that produces the patch.

### Current work

Currently, we are working on the following tasks:
 - abstracting away the game dependencies so we can test the code locally and in CI
 - adding local testing to the project that does not rely on the external game
   DLLs
 - migrating to the code to .NET best practices

### Requirements

 - We *must* use .NET Framework `v4.7.2`. Other frameworks will not work with
   RimWorld.
 - We run a "minimal" .NET project that targets VSCode (and not Visual Studio).
   We use `dotnet` directly to build the DLL.
### Contribution guidelines

- Pull requests should use "conventional commit" style messages.
- Update documentation as part of the PR when changes require it.
- We use "release-please" so the `CHANGELOG.md` is generated automatically and
  should not be updated manually in pull requests.
