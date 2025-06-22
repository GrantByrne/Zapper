# Claude Development Guidelines for Zapper

## Project Overview
Zapper is an open-source replacement for Logitech Harmony universal remotes built on Raspberry Pi. It uses ASP.NET Core backend with FastEndpoints, React frontend, and supports IR, Bluetooth, and network-based device control.

## Coding Conventions

### C# Conventions
1. **File-scoped namespaces**: Always use file-scoped namespaces (C# 10+)
   ```csharp
   namespace Zapper.Services;
   ```

2. **One class per file**: Each class should be in its own file with a matching filename

3. **Dependency injection**: Always use constructor injection, never property injection or service locator pattern

4. **Primary constructors**: Use primary constructors (C# 12+) when possible
   ```csharp
   public class DeviceService(IDeviceRepository repository, ILogger<DeviceService> logger)
   {
       // Use repository and logger directly
   }
   ```

5. **FastEndpoints**: Use FastEndpoints instead of traditional ASP.NET controllers

### General Conventions
- No Docker deployment - the app runs directly on the host
- Commit after each feature implementation
- Build and fix errors before committing
- Run lint and typecheck commands when available

## Technology Stack
- **Backend**: ASP.NET Core 9.0, FastEndpoints, Entity Framework Core, SQLite
- **Frontend**: React, TypeScript, Vite, Tailwind CSS, React Query
- **Hardware**: GPIO for IR, HidSharp for USB remotes, Bluetooth for Android TV/Apple TV
- **Protocols**: WebSocket for LG webOS, SSDP/UPnP for discovery

## Project Structure
```
src/
├── Zapper.API/          # FastEndpoints API
├── Zapper.Core/         # Domain models and interfaces
├── Zapper.Data/         # Entity Framework and repositories
├── Zapper.Services/     # Business logic services
├── Zapper.Integrations/ # Hardware and protocol implementations
└── Zapper.Web/          # React frontend
```

## Development Workflow
1. Implement feature
2. Commit changes
3. Build solution
4. Fix any build errors
5. Commit fixes