---
applyTo: '**'
---
The following guidelines should be followed when working on the Free Will mod for RimWorld
 - all code should be written in C# and follow the .NET coding conventions
 - the target framework is .NET v4.7.2 (this is required by RimWorld)
 - our development is "local only", meaning we don't use any cloud services or CI/CD pipelines
 - the RimWorld DLLs should be available (see Directory.Build.props for the paths)
 - tests are executed using the build-and-test.bat script