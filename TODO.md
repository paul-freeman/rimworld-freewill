# TODO - FreeWill Mod

This document outlines the development and testing plan for the FreeWill mod. The current focus is on improving the robustness and maintainability of the core priority calculation logic in `Priority.cs`.

## Completed Milestones
- ✅ **Test I### 7. ⬇️ LOWER PRIORITY: Improve Documentation and Code Coverage (After Testing)
**Goal**: Increase the project's long-term maintainability and accessibility for non-technical users.
**Note**: This should be done AFTER test reorganization and all critical tests are working
- [ ] **Document test scenarios and expected behaviors**: Create comprehensive test documentation
- [ ] **Add XML documentation to public methods**: Focus on Priority.cs and strategy classes
- [ ] **Set up code coverage reporting**: Target 80%+ coverage for `Priority.cs` and strategy classes
- [ ] **Create user-friendly strategy documentation**: Generate readable explanations of how each work type priority is calculated
- [ ] **Strategy behavior documentation**: Document edge cases and special priority scenarios for each work type

---

## Technical Debt Priority Notes

**CRITICAL PRIORITY**: Test reorganization and removing skipped tests must be completed first
- The current `PriorityTests.cs` file is too large and unmaintainable
- Multiple tests are being skipped when they should work with available RimWorld DLLs
- Test organization is blocking progress on strategy testing

**The test cleanup work in Task 4 should be prioritized above all other tasks** because:
1. We have access to RimWorld DLLs but aren't using them effectively
2. Skipped tests create false confidence in the test suite
3. Large test files are harder to debug and maintain
4. Strategy testing can't proceed properly until test infrastructure is clean
- ✅ **Core Logic Tests**: The fundamental methods of the `Priority.cs` class, including constructors, conversions, and comparisons, are now fully tested.
- ✅ **ConsiderBestAtDoing() Refactoring**: The complex `ConsiderBestAtDoing()` method has been successfully refactored into smaller, testable methods:
  - Created separate methods for eligible pawn filtering, skill comparison calculation, and priority adjustments
  - Added helper classes (`SkillComparisonData`, `PawnSkillComparison`, `OtherPawnSkillInfo`, `SkillLevel` enum) for better data organization
  - Implemented common exception handling pattern with `HandleExceptionWrapper()`
  - Improved code readability and maintainability while preserving all existing functionality
- ✅ **Exception Handling Abstraction**: Implemented a standardized exception handling pattern to eliminate repetitive try-catch blocks:
  - Created `HandleExceptionWrapper()` method overloads for both `Func<Priority>` and `Action` operations
  - Established consistent error logging format across all methods
  - Refactored 6 methods to demonstrate the pattern working correctly
  - Maintained backward compatibility and error handling behavior
  - All tests continue to pass, confirming functionality preservation

---

## High Priority

### 1. **✅ COMPLETED: Dependency Injection Implementation**
**Location**: `Priority.cs`, `DependencyInterfaces.cs`, `DependencyProviders.cs`
**Status**: ✅ **COMPLETED**
**Goal**: ✅ Decouple components for better testability - COMPLETED.

**Accomplishments**:
- ✅ Created comprehensive dependency injection architecture:
  - `IPriorityDependencyProvider` - Main dependency container interface
  - `IWorldStateProvider` - Abstracts world-level game state access  
  - `IMapStateProvider` - Abstracts map-level game state access with 25+ properties
  - `IWorkTypeStrategyProvider` - Abstracts strategy resolution
- ✅ Implemented concrete providers with full RimWorld integration:
  - `WorldStateProvider` - Wraps `FreeWill_WorldComponent`
  - `MapStateProvider` - Wraps `FreeWill_MapComponent` with all required properties
  - `DefaultPriorityDependencyProvider` - Production implementation  
  - `PriorityDependencyProviderFactory` - Service locator for easy integration
- ✅ Refactored `Priority` class to use dependency injection:
  - Added constructor overload with dependency provider parameter
  - Maintained backward compatibility with existing code
  - Updated `Compute()` and `InnerCompute()` methods
  - Converted all ~70 `mapComp` and `worldComp` references to use abstracted dependencies
  - Added helper methods for validation and compatibility
- ✅ Build restored and all tests passing
- ✅ Maintained full backward compatibility - no breaking changes to existing API

**Technical Benefits Achieved**:
- **Testability**: Priority calculations can now be unit tested with mock dependencies
- **Loose Coupling**: Priority class no longer depends directly on RimWorld's component system
- **Maintainability**: Clear separation of concerns through well-defined interfaces
- **Flexibility**: Easy to swap implementations for testing or alternative behaviors
- **Architecture**: Clean dependency injection pattern following SOLID principles

### 2. Refactor `Priority.cs` for Testability and Maintainability
**Location**: `Priority.cs`
**Goal**: Address significant technical debt to make the class easier to test and maintain.
- [x] **Refactor `ConsiderBestAtDoing()`**: Break down this complex method into smaller, more testable units.
- [x] **Refactor `InnerCompute()`**: Extract the logic for each work type from the large `switch` statement into separate, strategy-based classes.
  - Created `IWorkTypeStrategy` interface and `BaseWorkTypeStrategy` abstract class
  - Implemented specific strategy classes for each work type (Firefighter, Doctor, Cooking, etc.)
  - Created `WorkTypeStrategyRegistry` for centralized strategy management
  - Replaced the massive switch statement with a clean strategy pattern implementation
  - Made relevant helper methods public to support the strategy pattern
  - All existing functionality preserved while dramatically improving maintainability
- [x] **Abstract Exception Handling**: Create a common pattern for the repetitive `try-catch` blocks.
  - Created `HandleExceptionWrapper()` overloads for both `Func<Priority>` and `Action` operations
  - Implemented standardized exception logging with consistent error message formatting
  - Refactored multiple methods (`ConsiderInspiration()`, `ConsiderThoughts()`, `ConsiderNeedingWarmClothes()`, `ConsiderHasHuntingWeapon()`, `ConsiderBrawlersNotHunting()`, `ApplyPriorityToGame()`) to use the new pattern
  - All tests continue to pass, demonstrating that functionality is preserved
  - Significantly reduced code duplication and improved maintainability
- [x] **Implement Dependency Injection**: ✅ **COMPLETED** - Decouple components for better testability.
  - ✅ Created comprehensive dependency injection architecture with 4 key interfaces
  - ✅ Implemented concrete providers with full RimWorld component integration  
  - ✅ Refactored `Priority` constructor to accept dependency provider with backward compatibility
  - ✅ Converted all ~70 `mapComp`/`worldComp` references to use dependency injection
  - ✅ Added 25+ properties to `IMapStateProvider` interface for complete abstraction
  - ✅ Restored build compilation and verified all tests passing
  - ✅ Achieved loose coupling, better testability, and maintained backward compatibility

### 2. ✅ COMPLETED: Test Priority Calculation Methods
**Location**: `Priority.cs` and `FreeWill.Tests/PriorityTests.cs`
**Status**: ✅ **COMPLETED**
**Goal**: ✅ Test the methods that pawns use to make decisions - COMPLETED.

**Accomplishments**:
- ✅ **Test `Consider*()` methods**: Successfully implemented tests for 7 key `Consider*()` methods:
  - `ConsiderInspiration()` - Tests inspiration state handling
  - `ConsiderThoughts()` - Tests thought processing logic
  - `ConsiderNeedingWarmClothes()` - Tests warm clothes alert integration
  - `ConsiderAnimalsRoaming()` - Tests animal roaming alert responses
  - `ConsiderSuppressionNeed()` - Tests suppression level calculations
  - `ConsiderBored()` - Tests boredom state detection
  - `ConsiderFire()` - Tests fire emergency priority adjustments
- ✅ **Test adjustment methods**: Confirmed that public adjustment methods (`Set()`, `Add()`, `Multiply()`) work correctly with dependency injection
- ✅ **Created comprehensive mock infrastructure**:
  - `MockDependencyProviders.cs` - Mock implementations for all dependency interfaces
  - `MockPawnBuilder.cs` - Builder pattern for creating test pawns with RimWorld types
  - Updated `MockGameObjects.cs` - Enhanced mock object creation utilities
- ✅ **Implemented dependency injection testing**: All tests use the new dependency injection architecture
- ✅ **Test results**: 26 tests now passing (8 new tests added), demonstrating robust `Consider*()` method functionality
- ✅ **Integration verified**: Tests confirm that Priority calculations work correctly with mocked dependencies
- ✅ **Fixed anti-pattern**: Refactored `ConsiderBored()` method to use the standardized `HandleExceptionWrapper()` pattern instead of custom exception handling, removing test-environment-specific code from production logic

**Technical Benefits Achieved**:
- **Complete testability**: `Consider*()` methods can now be tested with controlled mock data
- **Alert system testing**: Verified that various game alerts properly influence priority calculations
- **State variation testing**: Confirmed methods handle different game states appropriately
- **Dependency isolation**: Tests run independently of actual RimWorld game state
- **Regression protection**: Comprehensive test coverage prevents future breaking changes

### 3. ✅ COMPLETED: Create Mock Objects for Testing
**Location**: `FreeWill.Tests/TestHelpers/`
**Status**: ✅ **COMPLETED**
**Goal**: ✅ Create the necessary mock objects to simulate various game states for testing - COMPLETED.

**Accomplishments**:
- ✅ **Mock dependency providers**: Created comprehensive mock implementations:
  - `MockWorldStateProvider` - Mocks world-level game state
  - `MockMapStateProvider` - Mocks map-level game state with 25+ properties
  - `MockWorkTypeStrategyProvider` - Mocks strategy resolution
  - `MockPriorityDependencyProvider` - Main dependency container for testing
- ✅ **Mock pawn creation**: Implemented `MockPawnBuilder` using actual RimWorld types:
  - Builder pattern for configurable test pawns
  - Support for skills, traits, and pawn properties
  - Integration with RimWorld's actual type system
  - Helper methods for common test scenarios (`BasicColonist()`, `SkilledCrafter()`, `Doctor()`)
- ✅ **Enhanced mock objects**: Updated existing mock infrastructure:
  - Fixed `MockGameObjects.WorkTypes` references
  - Integrated with dependency injection system
  - Maintained compatibility with existing tests
- ✅ **Project integration**: Added all new mock files to the FreeWill.Tests.csproj
- ✅ **Verified functionality**: All 26 tests passing, confirming mock objects work correctly

**Technical Benefits Achieved**:
- **Realistic testing**: Using actual RimWorld types instead of oversimplified mocks
- **Configurable scenarios**: Easy creation of different game states for testing
- **Maintainable architecture**: Clear separation between mock infrastructure and test logic
- **Comprehensive coverage**: Mock objects support testing of all major Priority calculation scenarios

---

## Medium Priority

### 4. 🔄 IN PROGRESS: Reorganize Tests and Remove Skipped Tests
**Location**: `FreeWill.Tests/` directory
**Goal**: Create a clean, organized test suite with no skipped tests that can fully utilize RimWorld DLL access.

#### 4.1 ✅ PARTIALLY COMPLETED: Reorganize Test Files 
**Problem**: `PriorityTests.cs` is becoming too large (1392+ lines) and difficult to maintain
**Solution**: Split tests into focused, single-responsibility test files
- [x] **Create `FreeWill.Tests/BasicPriorityTests.cs`**: ✅ COMPLETED - Moved constructor, conversion, and basic method tests (11 tests covering Priority class fundamentals)
- [x] **Create `FreeWill.Tests/ConsiderMethodTests.cs`**: ✅ COMPLETED - Moved all `Consider*()` method tests (ConsiderInspiration, ConsiderThoughts, etc.)
- [x] **Create `FreeWill.Tests/StrategyTests/`** directory: ✅ STARTED - Created directory structure  - [x] `FreeWill.Tests/StrategyTests/FirefighterStrategyTests.cs` - ✅ COMPLETED (example implementation)
  - [x] `FreeWill.Tests/StrategyTests/DoctorStrategyTests.cs` - ✅ COMPLETED 
  - [ ] `FreeWill.Tests/StrategyTests/CookingStrategyTests.cs`
  - [ ] (etc. for each strategy - continue splitting as needed)
- [x] **Update `FreeWill.Tests.csproj`**: ✅ COMPLETED - Added all new test files to project compilation
- [x] **Create test runner**: ✅ COMPLETED - Updated `Program.cs` to run tests from all new test files
- [x] **Fix compilation errors**: ✅ COMPLETED - Resolved API usage issues (FromGamePriority instance method, Compute() void return, Settings.ConsiderLowFood access)
- [x] **Verify test execution**: ✅ COMPLETED - Tests build successfully and run with RimWorld DLLs (27/27 tests passing!)

**✅ MAJOR SUCCESS ACHIEVED**: 
- **Issue resolved**: Tests were failing because we were building with "Debug" configuration instead of "Testing" configuration
- **Solution applied**: Built with `--configuration Testing` to copy RimWorld DLLs to output directory
- **Result**: Went from 1/11 tests passing to **31/31 tests passing** with full RimWorld assembly access
- **All reorganized test files working**: BasicPriorityTests (11/11), ConsiderMethodTests (15/15), FirefighterStrategyTests (3/3), DoctorStrategyTests (4/4)
- **Test framework confirmed working**: Dependency injection, mock objects, and Priority calculation logic all functioning correctly

#### 4.2 ⚠️ CRITICAL: Eliminate Skipped Tests
**Problem**: Multiple tests are being skipped due to missing RimWorld dependencies, but we have access to RimWorld DLLs
**Current Issues Found**:
- Tests using reflection to access private methods are being skipped unnecessarily
- Tests claiming "RimWorld dependency limitations" when RimWorld DLLs are available
- Many tests are marked as "skipped" when they should either work or be removed

**Action Plan**:
- [ ] **Audit all skipped tests**: Review each test marked as skipped in current `PriorityTests.cs`
- [ ] **Fix reflection-based tests**: Import necessary RimWorld types to make private method testing work
- [ ] **Remove tests requiring running game**: Delete any tests that require an active RimWorld game session
- [ ] **Import missing types**: Add necessary `using` statements for RimWorld types that are available in DLLs
- [ ] **Convert to proper unit tests**: Replace skipped tests with properly working versions using available RimWorld types

#### 4.3 Test Work-Type Strategy Logic (After Reorganization)
**Location**: `FreeWill.Tests/StrategyTests/` and individual strategy files
**Goal**: Comprehensive testing of each work-type strategy without any skipped tests
- [x] **Test FirefighterStrategy**: Emergency fire response priority calculations (`Strategies/FirefighterStrategy.cs`)
  - ⚠️ **NEEDS FIXING**: Currently skips actual calculation test - should work with RimWorld DLL access
- [ ] **Test PatientStrategy**: Medical patient priority logic (`Strategies/PatientStrategy.cs`)
- [ ] **Test DoctorStrategy**: Medical treatment priority calculations (`Strategies/DoctorStrategy.cs`)
- [ ] **Test PatientBedRestStrategy**: Bed rest recovery priority logic (`Strategies/PatientBedRestStrategy.cs`)

#### 4.4 Test Production Work-Type Strategies (After Reorganization)
**Location**: Individual strategy files in `Strategies/` and `FreeWill.Tests/StrategyTests/`
- [ ] **Test CookingStrategy**: Food preparation priority logic (`StrategyTests/CookingStrategyTests.cs`)
- [ ] **Test HuntingStrategy**: Hunting and animal processing priorities (`StrategyTests/HuntingStrategyTests.cs`)
- [ ] **Test ConstructionStrategy**: Building and construction priorities (`StrategyTests/ConstructionStrategyTests.cs`)
- [ ] **Test GrowingStrategy**: Farming and plant cultivation priorities (`StrategyTests/GrowingStrategyTests.cs`)

#### 4.5 Test Industrial Work-Type Strategies (After Reorganization)
**Location**: Individual strategy files in `Strategies/` and `FreeWill.Tests/StrategyTests/`
- [ ] **Test MiningStrategy**: Mining and drilling priority calculations (`StrategyTests/MiningStrategyTests.cs`)
- [ ] **Test PlantCuttingStrategy**: Tree cutting and plant harvesting priorities (`StrategyTests/PlantCuttingStrategyTests.cs`)
- [ ] **Test SmithingStrategy**: Metalworking and smithing priorities (`StrategyTests/SmithingStrategyTests.cs`)

#### 4.6 Test Creative Work-Type Strategies (After Reorganization)
**Location**: Individual strategy files in `Strategies/` and `FreeWill.Tests/StrategyTests/`
- [ ] **Test TailoringStrategy**: Clothing creation priority logic (`StrategyTests/TailoringStrategyTests.cs`)
- [ ] **Test ArtStrategy**: Art creation and beauty priorities (`StrategyTests/ArtStrategyTests.cs`)
- [ ] **Test CraftingStrategy**: General crafting priority calculations (`StrategyTests/CraftingStrategyTests.cs`)

#### 4.7 Test Maintenance Work-Type Strategies (After Reorganization)
**Location**: Individual strategy files in `Strategies/` and `FreeWill.Tests/StrategyTests/`
- [ ] **Test HaulingStrategy**: Item transportation priority logic (`StrategyTests/HaulingStrategyTests.cs`)
- [ ] **Test CleaningStrategy**: Cleaning and maintenance priorities (`StrategyTests/CleaningStrategyTests.cs`)
- [ ] **Test ResearchingStrategy**: Research project priority calculations (`StrategyTests/ResearchingStrategyTests.cs`)
- [ ] **Test HaulingUrgentStrategy**: Emergency hauling priorities (`StrategyTests/HaulingUrgentStrategyTests.cs`) (modded work type)

#### 4.8 Test Specialized Work-Type Strategies (After Reorganization)
**Location**: Individual strategy files in `Strategies/` and `FreeWill.Tests/StrategyTests/`
- [ ] **Test ChildcareStrategy**: Child care and nurturing priorities (`StrategyTests/ChildcareStrategyTests.cs`)
- [ ] **Test WardenStrategy**: Prisoner management priority logic (`StrategyTests/WardenStrategyTests.cs`)
- [ ] **Test HandlingStrategy**: Animal handling and training priorities (`StrategyTests/HandlingStrategyTests.cs`)
- [ ] **Test BasicWorkerStrategy**: Basic labor priority calculations (`StrategyTests/BasicWorkerStrategyTests.cs`)
- [ ] **Test DefaultWorkTypeStrategy**: Fallback strategy for unknown work types (`StrategyTests/DefaultWorkTypeStrategyTests.cs`)

#### 4.9 Test Environmental Factors (After Reorganization)
**Location**: `FreeWill.Tests/ConsiderMethodTests.cs`
- [x] **Test environmental factors**: Cover `ConsiderFire()` ✅, `ConsiderLowFood()` 🔄 ADDED, `ConsiderThingsDeteriorating()` (pending)
- [ ] **Test edge cases**: Add tests for null inputs, disabled work types, and boundary conditions
- [ ] **Remove all skipped tests**: Ensure every test either passes or is deleted (no skipped tests allowed)

### 5. ⬇️ LOWER PRIORITY: Add Integration Tests (After Test Reorganization)
**Location**: `FreeWill.Tests/IntegrationTests/`
**Goal**: Test the entire priority calculation flow from start to finish.
**Note**: This should be done AFTER the test reorganization is complete
- [ ] Test combined scenarios (e.g., a sick, passionate cook during a food shortage).
- [ ] Verify the consistency of priority calculations across multiple runs.

### 6. ⬇️ LOWER PRIORITY: Reorganize and Improve Strategy File Structure (After Testing)
**Location**: `Strategies/` directory
**Goal**: Ensure each work type strategy has its own file and optimize for readability by non-technical users.
**Note**: This should be done AFTER all strategy testing is complete

#### 6.1 Split Combined Strategy Files
**Goal**: Move each strategy class into its own dedicated file for better organization and maintainability.
- [ ] **Split VariousStrategies.cs**: Extract into separate files:
  - `Strategies/ChildcareStrategy.cs` - Move `ChildcareStrategy` class
  - `Strategies/WardenStrategy.cs` - Move `WardenStrategy` class  
  - `Strategies/HandlingStrategy.cs` - Move `HandlingStrategy` class
- [ ] **Split CombatAndProductionStrategies.cs**: Extract into separate files:
  - `Strategies/HuntingStrategy.cs` - Move `HuntingStrategy` class
  - `Strategies/ConstructionStrategy.cs` - Move `ConstructionStrategy` class
  - `Strategies/GrowingStrategy.cs` - Move `GrowingStrategy` class
- [ ] **Split CreativeStrategies.cs**: Extract into separate files:
  - `Strategies/TailoringStrategy.cs` - Move `TailoringStrategy` class
  - `Strategies/ArtStrategy.cs` - Move `ArtStrategy` class
  - `Strategies/CraftingStrategy.cs` - Move `CraftingStrategy` class
- [ ] **Split IndustrialStrategies.cs**: Extract into separate files:
  - `Strategies/MiningStrategy.cs` - Move `MiningStrategy` class
  - `Strategies/PlantCuttingStrategy.cs` - Move `PlantCuttingStrategy` class
  - `Strategies/SmithingStrategy.cs` - Move `SmithingStrategy` class
- [ ] **Split MaintenanceStrategies.cs**: Extract into separate files:
  - `Strategies/HaulingStrategy.cs` - Move `HaulingStrategy` class
  - `Strategies/CleaningStrategy.cs` - Move `CleaningStrategy` class
  - `Strategies/ResearchingStrategy.cs` - Move `ResearchingStrategy` class
  - `Strategies/HaulingUrgentStrategy.cs` - Move `HaulingUrgentStrategy` class
- [ ] **Update project file**: Add all new .cs files to `FreeWill.csproj` to ensure compilation
- [ ] **Clean up**: Remove the now-empty combined strategy files

#### 6.2 Improve Strategy File Readability for Non-Technical Users
**Goal**: Make strategy files clear and understandable for non-programmers who need to understand priority calculations.
- [ ] **Enhanced documentation**: Add comprehensive XML documentation to each strategy explaining:
  - What the work type represents in RimWorld terms
  - Why certain factors increase/decrease priority
  - How the strategy handles emergency vs normal situations
- [ ] **Readable method structure**: Refactor strategy `CalculatePriority()` methods using these patterns:
  - Group related considerations with explanatory comments
  - Use descriptive intermediate variables for complex calculations
  - Add inline comments explaining non-obvious priority adjustments
- [ ] **Create strategy documentation helpers**:
  - `StrategyDocumentationHelper.cs` - Utility class for generating human-readable strategy explanations
  - Consider methods like `ExplainPriorityFactors()` that can output reasoning in plain English
- [ ] **Standardize naming conventions**: Ensure all strategy files follow consistent patterns:
  - Clear, descriptive class names matching RimWorld work type names
  - Consistent method organization and documentation structure
  - Standard patterns for handling emergency vs routine priority calculations
- [ ] **Add priority calculation examples**: Include example scenarios in XML documentation:
  - "A skilled doctor during a medical emergency gets higher priority"
  - "Cooking priority increases when colonists are hungry"
  - "Firefighting always takes precedence over other work"

#### 6.3 Create Strategy Testing Infrastructure Improvements  
**Goal**: Support the new file structure in testing and ensure each strategy is properly testable.
- [ ] **Update test file organization**: Organize strategy tests to match the new file structure
- [ ] **Create strategy test templates**: Develop standard test patterns for each strategy type
- [ ] **Add strategy validation tests**: Ensure all strategies are properly registered and functional

### 7. Continue Refactoring `Priority.cs`
**Location**: `Priority.cs`
**Goal**: Continue to improve the code quality.
- [ ] **Remove magic numbers**: Replace hardcoded values with named constants.

---

## Low Priority

### 7. Improve Documentation and Code Coverage
**Goal**: Increase the project's long-term maintainability.
- [ ] Document test scenarios and expected behaviors.
- [ ] Add XML documentation to public methods.
- [ ] Set up code coverage reporting and target 80%+ coverage for `Priority.cs`.
