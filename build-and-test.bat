@echo off
REM Build and Test Script for FreeWill Mod
REM This script builds the project in Testing configuration and runs the tests

echo Building FreeWill.Tests in Testing configuration...

REM Build the test project in Testing configuration
dotnet build --configuration Testing FreeWill.Tests/FreeWill.Tests.csproj

if %ERRORLEVEL% EQU 0 (
    echo Build successful! Running tests...
    
    REM Check if the test executable exists and run it
    if exist "FreeWill.Tests/bin/Testing/FreeWill.Tests.exe" (
        echo Executing test suite...
        "FreeWill.Tests/bin/Testing/FreeWill.Tests.exe"
        
        if %ERRORLEVEL% EQU 0 (
            echo All tests completed successfully!
        ) else (
            echo Some tests failed. Exit code: %ERRORLEVEL%
        )
    ) else (
        echo Test executable not found at: FreeWill.Tests/bin/Testing/FreeWill.Tests.exe
        echo Please ensure the build completed successfully.
    )
) else (
    echo Build failed with exit code: %ERRORLEVEL%
)

pause
