# TODO - FreeWill Mod

This document outlines the development and testing plan for the FreeWill mod. The current focus is on improving the robustness and maintainability of the core priority calculation logic in `Priority.cs`.

## Completed Milestones
- ✅ **Test Infrastructure**: A full testing environment has been set up, enabling tests to run with all necessary RimWorld DLLs.
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

### 2. Test Priority Calculation Methods
**Location**: `Priority.cs` and `FreeWill.Tests/PriorityTests.cs`
**Goal**: Test the methods that pawns use to make decisions.
- [ ] **Test `Consider*()` methods**: Write tests for methods like `ConsiderRelevantSkills()`, `ConsiderPassion()`, `ConsiderHealth()`, etc.
- [ ] **Test adjustment methods**: Cover the private helper methods (`Set()`, `Add()`, `Multiply()`) that modify priority scores.

### 3. Create Mock Objects for Testing
**Location**: `FreeWill.Tests/TestHelpers/`
**Goal**: Create the necessary mock objects to simulate various game states for testing.
- [ ] Mock `FreeWill_WorldComponent` and `FreeWill_MapComponent`.
- [ ] Create builders for pawns and `WorkTypeDefs` to simplify test setup.

---

## Medium Priority

### 4. Test Work-Type and Environmental Logic
**Location**: `Priority.cs` and `FreeWill.Tests/PriorityTests.cs`
**Goal**: Ensure priority calculations are correct for specific in-game situations.
- [ ] **Test work-type logic**: Cover specific work types like `Firefighter`, `Patient`, `Doctor`, etc.
- [ ] **Test environmental factors**: Cover `ConsiderFire()`, `ConsiderLowFood()`, `ConsiderThingsDeteriorating()`, etc.
- [ ] **Test edge cases**: Add tests for null inputs, disabled work types, and boundary conditions.

### 5. Add Integration Tests
**Location**: `FreeWill.Tests/`
**Goal**: Test the entire priority calculation flow from start to finish.
- [ ] Test combined scenarios (e.g., a sick, passionate cook during a food shortage).
- [ ] Verify the consistency of priority calculations across multiple runs.

### 6. Continue Refactoring `Priority.cs`
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
