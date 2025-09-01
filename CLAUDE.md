# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

StellaNow C# SDK is a .NET library for integrating C# applications with the StellaNow Platform using MQTT over secure WebSockets. The SDK provides automated connection handling, message queuing, authentication management, and extensible architecture for custom implementations.

## Architecture

The SDK follows a modular architecture with clear separation of concerns:

- **Core Services**: `IStellaNowSdk` and `StellaNowSdk` provide the main SDK interface
- **Message System**: Type-safe message wrappers extending `StellaNowMessageBase` with JSON serialization
- **Queue Strategies**: Pluggable queue implementations (`FifoMessageQueueStrategy`, `LifoMessageQueueStrategy`) implementing `IMessageQueueStrategy`
- **Sinks**: Message destinations with `IStellaNowSink` interface (currently MQTT-based)
- **Authentication**: Multiple auth strategies including OIDC and no-auth via `IMqttAuthStrategy`
- **Converters**: Custom JSON converters in `/src/Converters/` for data type handling
- **Configuration**: Environment-specific configs (`StellaNowDevEnvironmentConfig`, `StellaNowProdEnvironmentConfig`)

## Development Commands

### Build and Test
```bash
# Restore dependencies
dotnet restore StellaNowSDK.sln

# Build solution
dotnet build StellaNowSDK.sln --configuration Release --no-restore

# Run all tests
dotnet test StellaNowSDKTests/StellaNowSDKTests.csproj

# Run specific test
dotnet test StellaNowSDKTests/StellaNowSDKTests.csproj --filter "TestMethodName"
```

### Project Structure
- `StellaNowSDK/` - Main SDK library
- `StellaNowSDKTests/` - MSTest-based test project using Moq for mocking
- `StellaNowSDKDemo/` - Demo console application

## Key Implementation Details

### Message Format
Messages extend `StellaNowMessageBase` and require:
- Type identifier string
- List of `EntityType` objects
- JSON property attributes for serialization
- Entity ID marked with `[JsonIgnore]`

### Dependency Injection
SDK registers via extension methods:
- `AddStellaNowSdkWithMqttAndOidcAuth()` for OIDC authentication
- `AddStellaNowSdkWithMqttAndNoAuth()` for development/testing

### Extensibility Points
- Custom queue strategies via `IMessageQueueStrategy`
- Custom sinks via `IStellaNowSink`
- Custom auth strategies via `IMqttAuthStrategy`
- Custom connection strategies via `IMqttConnectionStrategy`

### Testing Framework
Uses MSTest with:
- Moq for mocking dependencies
- Test utilities in `StellaNowSDKTests/src/TestUtilities/`
- Separate test classes for each major component

## Environment Configuration

The SDK supports multiple environments through configuration classes that define MQTT broker endpoints, OIDC endpoints, and other environment-specific settings.