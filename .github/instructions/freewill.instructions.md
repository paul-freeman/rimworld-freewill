---
applyTo: '**'
---
The following guidelines should be followed when working on the Free Will mod for RimWorld
 - all code should be written in C# and follow the .NET coding conventions (best practices trump the existing code style!)
 - the target framework is .NET v4.7.2 (this is required by RimWorld)
 - this project uses Windows line endings (CRLF)
 - our development is "local only", meaning we don't use any cloud services or CI/CD pipelines
 - the RimWorld DLLs should be available (see Directory.Build.props for the paths)
 - tests are executed using the build-and-test.bat script (you need to wait a bit for the output to appear)
 - you cannot use "&&" in terminal commands, since they run in PowerShell
 - always run `dotnet format --verify-no-changes --severity info | Select-String "diagnostic"` (wait a bit for it to finish) and fix any issues to ensure code style consistency
 - you will need to add all .cs files to the project file to ensure they are compiled and tested
 - please fix any whitespace linter issues that are reported (even if they are not related to your changes)