# Testing Plan for Priority.cs

## Overview
This TODO outlines the plan to add comprehensive unit tests for the `Priority.cs` file, which contains the core logic for calculating work priorities for pawns in the FreeWill mod. The `Priority` class is complex with ~2500 lines of code and contains numerous methods that calculate priority adjustments based on various game factors.

## Priority: HIGH
Adding tests for this core component is critical for:
- Ensuring priority calculations remain correct during future modifications
- Catching regressions when game mechanics change
- Documenting expected behavior of the complex priority calculation logic
- Improving maintainability of this large, complex class

## Technical Debt Analysis
The `Priority.cs` file shows several areas of technical debt that should be addressed:
1. **Large class with many responsibilities** - The class handles over 20 different work types with distinct logic
2. **Long switch statement** - The `InnerCompute()` method has a massive switch with repetitive code patterns
3. **Exception handling** - Many methods have similar try-catch blocks that could be abstracted
4. **Magic numbers** - Various hardcoded constants throughout (e.g., `DISABLED_CUTOFF = 100 / (Pawn_WorkSettings.LowestPriority + 1)`)
5. **Method complexity** - Some methods like `ConsiderBestAtDoing()` are very complex and hard to test

## Testing Strategy

### 1. Setup Test Infrastructure (Priority: HIGH)
**Location**: Create new test project structure
- [x] Create `FreeWill.Tests` project with MSTest/NUnit framework
- [x] Add necessary RimWorld references for testing
- [x] Create mock objects/test data for `Pawn`, `WorkTypeDef`, etc.
- [x] Setup test data builders for common scenarios
- [x] **Update tests to use full RimWorld DLLs** - Ensure test project has access to complete RimWorld assemblies as defined in Directory.Build.props to enable comprehensive testing without limitations

**COMPLETED**: Basic test infrastructure has been established. Created `FreeWill.Tests` project with:
- Basic project structure targeting .NET Framework 4.7.2 (matching RimWorld requirements)
- RimWorld assembly references (Assembly-CSharp, UnityEngine, etc.)
- Test helper classes for creating mock game objects and test data
- Simplified initial tests that compile and can verify basic functionality
- Note: Full MSTest integration is prepared but simplified for initial setup
- **UPDATED**: Test project now has access to complete RimWorld assemblies including 0Harmony, UnityEngine.IMGUIModule, and UnityEngine.TextRenderingModule, matching the main project's references for comprehensive testing capabilities

**TECHNICAL NOTES**: Due to RimWorld's sealed classes and complex game state dependencies, the testing approach uses:
- Simple test helper classes instead of complex mocking frameworks initially
- Basic test methods that can be run manually to verify functionality
- Foundation for expanding to full MSTest framework once core functionality is verified
- Full RimWorld DLL access enables testing of Harmony patches and Unity-dependent code paths

### 2. Core Priority Calculation Tests (Priority: HIGH)
**Location**: `Priority.cs` - Core methods
- [x] Test `Priority` constructor with various inputs
- [x] Test `Compute()` method error handling
- [x] Test `ToGamePriority()` conversion logic with boundary values
- [x] Test `FromGamePriority()` conversion logic with boundary values
- [x] Test `IComparable.CompareTo()` implementation
- [x] Test round-trip conversion between ToGamePriority and FromGamePriority
- [x] Test edge cases for both conversion methods
- [x] Test priority adjustment helper methods (Set, Add via reflection)
- [ ] Test `Compute()` method for successful calculations with valid game state
- [ ] Test `Multiply()` method with various multipliers
- [ ] Test `AlwaysDo()` and `AlwaysDoIf()` methods
- [ ] Test `NeverDo()` and `NeverDoIf()` methods
- [ ] Test behavior when `Disabled` flag is set

**COMPLETED**: Basic constructor and IComparable tests have been implemented, plus core conversion methods.
- Constructor tests verify basic property initialization
- IComparable tests verify null handling and type checking
- ToGamePriority() and FromGamePriority() conversion methods tested with boundary values and edge cases
- Round-trip conversion testing ensures consistency between conversion methods
- Error handling in Compute() method tested for invalid inputs
- Priority adjustment helper methods (Set, Add) tested via reflection
- Tests are structured to handle RimWorld's complex dependencies gracefully

**TECHNICAL CHALLENGES IDENTIFIED**:
- Priority.Compute() requires a valid Pawn with Map and game components for full testing
- Full testing of priority calculation requires creating a test harness that can simulate RimWorld game state
- Reflection-based testing used for private methods (Set, Add) - may need better approach for production tests
- Current tests focus on areas that can be validated without complex game state dependencies

**STEP 2 PROGRESS**: Core conversion methods and error handling are now tested. Next focus should be on testing with valid game state scenarios.

### 3. Priority Adjustment Method Tests (Priority: HIGH)
**Location**: `Priority.cs` - Private helper methods
- [ ] Test `Set()` method with various values and descriptions
- [ ] Test `Add()` method with positive, negative, and zero adjustments
- [ ] Test `Multiply()` method with various multipliers
- [ ] Test `AlwaysDo()` and `AlwaysDoIf()` methods
- [ ] Test `NeverDo()` and `NeverDoIf()` methods
- [ ] Test behavior when `Disabled` flag is set

### 4. Work Type Specific Logic Tests (Priority: MEDIUM)
**Location**: `Priority.cs` - `InnerCompute()` switch cases
- [ ] Test `FIREFIGHTER` work type priority calculation
- [ ] Test `PATIENT` work type priority calculation  
- [ ] Test `DOCTOR` work type priority calculation
- [ ] Test `COOKING` work type priority calculation
- [ ] Test `HAULING` work type priority calculation
- [ ] Test `CLEANING` work type priority calculation
- [ ] Test `RESEARCHING` work type priority calculation
- [ ] Test default case for unknown work types
- [ ] Test modded work type support (`HAULING_URGENT`)

### 5. Consideration Method Tests (Priority: HIGH)
**Location**: `Priority.cs` - `Consider*()` methods
- [ ] Test `ConsiderRelevantSkills()` with different skill levels
- [ ] Test `ConsiderPassion()` for Minor, Major, None passions
- [ ] Test `ConsiderVanillaSkillsExpanded()` for extended passion types
- [ ] Test `ConsiderHealth()` with various health states
- [ ] Test `ConsiderThoughts()` with different thought types
- [ ] Test `ConsiderInspiration()` with relevant/irrelevant inspirations
- [ ] Test `ConsiderBored()` with idle/active pawns
- [ ] Test `ConsiderCompletingTask()` when pawn is doing current work
- [ ] Test `ConsiderIsAnyoneElseDoing()` with multiple pawns
- [ ] Test `ConsiderBestAtDoing()` skill comparison logic

### 6. Environmental Factor Tests (Priority: MEDIUM)
**Location**: `Priority.cs` - Environmental consideration methods
- [ ] Test `ConsiderFire()` with fire in home area vs map fires
- [ ] Test `ConsiderLowFood()` with different food shortage levels
- [ ] Test `ConsiderThingsDeteriorating()` for hauling priorities
- [ ] Test `ConsiderBuildingImmunity()` with sick pawns
- [ ] Test `ConsiderColonistsNeedingTreatment()` scenarios
- [ ] Test `ConsiderDownedColonists()` impact on different work types
- [ ] Test `ConsiderRefueling()` urgency levels
- [ ] Test `ConsiderBeautyExpectations()` with different expectation levels

### 7. Edge Case and Error Handling Tests (Priority: MEDIUM)
**Location**: `Priority.cs` - Error conditions
- [ ] Test behavior with null pawn input
- [ ] Test behavior with null WorkTypeDef input
- [ ] Test exception handling in `Compute()` method
- [ ] Test behavior when map components are null
- [ ] Test behavior with disabled work types
- [ ] Test boundary conditions for priority value clamping (0.0f to 1.0f)
- [ ] Test game priority conversion edge cases (invalid values)

### 8. Integration Tests (Priority: MEDIUM)
**Location**: Test complete priority calculation flows
- [ ] Test full priority calculation flow for typical scenarios
- [ ] Test priority adjustments with multiple factors combined
- [ ] Test consistency of priority calculations across multiple runs
- [ ] Test performance with large numbers of pawns/work types

### 9. Mock and Test Data Setup (Priority: HIGH)
**Location**: Test infrastructure
- [ ] Create mock `FreeWill_WorldComponent` for testing
- [ ] Create mock `FreeWill_MapComponent` for testing  
- [ ] Create test `Pawn` objects with various configurations
- [ ] Create test `WorkTypeDef` objects for all supported work types
- [ ] Create builders for complex test scenarios
- [ ] Setup test data for various game states (fire, low food, etc.)

### 10. Refactoring for Testability (Priority: MEDIUM)
**Location**: `Priority.cs` - Code improvements
- [ ] Extract constants to reduce magic numbers
- [ ] Break down large methods like `ConsiderBestAtDoing()` into smaller testable units
- [ ] Consider extracting work-type-specific logic into separate classes
- [ ] Abstract common exception handling patterns
- [ ] Add dependency injection for better testability of components

### 11. Documentation and Code Coverage (Priority: LOW)
**Location**: Test documentation
- [ ] Document test scenarios and expected behaviors
- [ ] Add XML documentation to test methods
- [ ] Setup code coverage reporting
- [ ] Target 80%+ code coverage for critical paths
- [ ] Create test run documentation for CI/CD

## Dependencies and Considerations

### RimWorld Testing Challenges
- RimWorld types are not easily mockable (sealed classes, static dependencies)
- Game state dependencies make isolated unit testing difficult
- May need to use integration tests for some scenarios
- Consider using Moq or similar mocking framework where possible

### Test Execution Environment
- Tests will need RimWorld assemblies available
- May require specific .NET Framework version (4.7.2)
- Consider running tests in isolated AppDomain to avoid state pollution

### Maintenance Considerations
- Tests should be resilient to RimWorld version updates
- Mock objects should be kept in sync with actual RimWorld API changes
- Consider creating a test suite that can run against different RimWorld versions

## Implementation Order
1. Setup test infrastructure and basic mocking (Items 1, 9)
2. Test core priority calculation methods (Item 2)
3. Test priority adjustment helpers (Item 3)
4. Test key consideration methods (Item 5 - subset)
5. Add error handling and edge case tests (Item 7)
6. Expand to cover all consideration methods (Item 5 - complete)
7. Add work type specific tests (Item 4)
8. Add integration and performance tests (Item 8)
9. Address refactoring opportunities (Item 10)
10. Complete documentation and coverage (Item 11)

## Success Criteria
- [ ] 80%+ code coverage on Priority.cs core logic
- [ ] All critical priority calculation paths tested
- [ ] Tests run successfully in CI/CD pipeline
- [ ] Tests serve as documentation for expected behavior
- [ ] Regressions caught by automated test suite
- [ ] Test suite execution time under 5 minutes for full run
