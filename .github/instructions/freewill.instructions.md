---
applyTo: '**'
---
The following guidelines should be followed when working on the Free Will mod for RimWorld
 - all code should be written in C# and follow the .NET coding conventions (best practices trump the existing code style!)
 - the target framework is .NET v4.7.2 (this is required by RimWorld)
 - our development is "local only", meaning we don't use any cloud services or CI/CD pipelines
 - the RimWorld DLLs should be available (see Directory.Build.props for the paths)
 - tests are executed using the build-and-test.bat script (you need to wait a bit for the output to appear)
 - always run `dotnet format --verify-no-changes --severity info | Select-String "diagnostic"` (wait a bit for it to finish) and fix any issues to ensure code style consistency
 - it seems that your code patches frequently lack a trailing newline, which breaks the formatting, so please ensure that you always add a trailing newline to the end of your code patches
 