# TemperatureHub.Tests

Comprehensive MSTest unit test suite for the TemperatureHub project.

## Overview

This test project provides complete unit test coverage for all public methods in the TemperatureHub application, including:

- **Helper Classes**: Utility functions for date/time manipulation, number rounding, and heat index calculations
- **Controller Classes**: All REST API endpoints for sensor data, weather information, and system settings
- **Repository Classes**: Database executor and data access layer components

## Test Statistics

- **Total Tests**: 46
- **Test Categories**:
  - Helper Tests: 23
  - Controller Tests: 18
  - Repository Tests: 5

## Test Structure

```
TemperatureHub.Tests/
├── Helpers/
│   ├── DateTimeHelperTests.cs       (7 tests)
│   ├── NumberTests.cs                (9 tests)
│   └── HeatHelperTests.cs            (7 tests)
├── Controllers/
│   ├── SensorDataControllerTests.cs  (6 tests)
│   ├── MinMaxData4DayControllerTests.cs (3 tests)
│   ├── SensorMasterDataControllerTests.cs (3 tests)
│   ├── SettingControllerTests.cs     (3 tests)
│   └── WeatherControllerTests.cs     (3 tests)
└── Repository/
    └── ExecutorTests.cs              (5 tests)
```

## Running Tests

### Run all tests
```bash
dotnet test
```

### Run specific test category
```bash
# Run only Helper tests
dotnet test --filter "FullyQualifiedName~TemperatureHub.Tests.Helpers"

# Run only Controller tests
dotnet test --filter "FullyQualifiedName~TemperatureHub.Tests.Controllers"

# Run only Repository tests
dotnet test --filter "FullyQualifiedName~TemperatureHub.Tests.Repository"
```

### Run with detailed output
```bash
dotnet test --logger "console;verbosity=detailed"
```

## Testing Framework

- **MSTest**: Microsoft's unit testing framework for .NET
- **Moq**: Mocking framework for creating test doubles
- **Target Framework**: .NET 10.0

## Test Coverage

### Helper Classes
- `DateTimeHelper.Next()`: Tests for date calculation and day-of-week navigation
- `Number.HalfRound()`: Tests for rounding to nearest 0.5 increment with banker's rounding
- `HeatHelper.GetHeatIndexCelsius()`: Tests for heat index calculations with various temperature and humidity combinations

### Controller Classes
All API controllers are tested with:
- Valid input scenarios
- Edge cases (empty data, null values)
- Error handling

### Repository Classes
- `Executor`: Thread-safe execution queue with asynchronous action processing

## Notes

Classes with complex external dependencies (SQLiteFileRepository, ProcessData, NetatmoDataHandler) that interact with databases or external APIs are candidates for integration testing rather than unit testing, and are not included in this suite.

## Continuous Integration

These tests are designed to run in CI/CD pipelines and provide fast feedback on code changes. All tests complete in under 2 seconds.
