# Contributing to Zapper

Thank you for your interest in contributing to Zapper! This guide will help you get started with contributing to this open-source universal remote control project.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Contributing Guidelines](#contributing-guidelines)
- [Pull Request Process](#pull-request-process)
- [Issue Reporting](#issue-reporting)
- [Development Standards](#development-standards)
- [Testing Guidelines](#testing-guidelines)
- [Documentation](#documentation)

## Code of Conduct

This project follows a [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you agree to abide by its terms. Please report any unacceptable behavior to the project maintainers.

### Our Standards

- **Be respectful**: Treat all contributors with respect and kindness
- **Be inclusive**: Welcome newcomers and help them succeed
- **Be collaborative**: Work together to improve the project
- **Be constructive**: Provide helpful feedback and suggestions

## Getting Started

### Ways to Contribute

- **Bug Reports**: Help identify and document issues
- **Feature Requests**: Suggest new functionality
- **Code Contributions**: Fix bugs or implement features
- **Documentation**: Improve guides and API documentation
- **Testing**: Help test new features and verify bug fixes
- **IR Code Database**: Add IR codes for new devices
- **Translations**: Help localize the interface

### Areas Needing Help

- **Hardware Support**: Additional IR protocols, RF modules
- **Device Integration**: Support for new smart TV platforms
- **Mobile App**: React Native or Flutter mobile client
- **Voice Control**: Integration with Alexa, Google Assistant
- **Cloud Sync**: Optional cloud backup and sync features

## Development Setup

### Prerequisites

- **.NET 9.0 SDK**: [Download here](https://dotnet.microsoft.com/download)
- **Git**: Version control
- **Code Editor**: Visual Studio, VS Code, or Rider
- **Raspberry Pi** (optional): For hardware testing

### Initial Setup

```bash
# Clone the repository
git clone https://github.com/yourusername/zapper-next-gen.git
cd zapper-next-gen

# Create development branch
git checkout -b feature/your-feature-name

# Restore dependencies
cd src
dotnet restore

# Build the project
dotnet build

# Run tests
dotnet test

# Run the application
dotnet run --project Zapper
```

### Development Environment

#### Visual Studio Code Setup

```json
// .vscode/settings.json
{
  "dotnet.defaultSolution": "src/Zapper.sln",
  "files.exclude": {
    "**/bin": true,
    "**/obj": true
  },
  "editor.formatOnSave": true,
  "omnisharp.enableEditorConfigSupport": true
}
```

#### Recommended Extensions

- C# for Visual Studio Code
- .NET Core Tools
- GitLens
- EditorConfig for VS Code
- REST Client (for API testing)

### Project Structure

```
zapper-next-gen/
├── src/
│   ├── Zapper/               # Main application
│   │   ├── Controllers/         # API controllers
│   │   ├── Services/            # Business logic
│   │   ├── Models/              # Data models
│   │   ├── Hardware/            # Hardware abstraction
│   │   ├── Endpoints/           # FastEndpoints
│   │   └── wwwroot/             # Web interface
│   └── Zapper.Tests/         # Unit tests
├── docs/                        # Documentation
├── examples/                    # Integration examples
└── tools/                       # Development tools
```

## Contributing Guidelines

### Before You Start

1. **Check existing issues**: Look for related work or discussions
2. **Open an issue**: Discuss your proposal before starting major work
3. **Fork the repository**: Create your own fork for development
4. **Create a branch**: Use descriptive branch names

### Branch Naming Convention

```bash
# Feature branches
feature/add-sony-ir-codes
feature/bluetooth-le-support
feature/web-interface-improvements

# Bug fix branches
bugfix/ir-timing-issue
bugfix/webos-discovery-timeout

# Documentation branches
docs/api-reference-update
docs/troubleshooting-guide

# Hotfix branches (for urgent production fixes)
hotfix/security-vulnerability
```

### Commit Message Format

Use the [Conventional Commits](https://www.conventionalcommits.org/) format:

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

**Examples:**
```bash
feat(ir): add support for RC5 protocol
fix(webos): resolve discovery timeout issue
docs(api): update device endpoint documentation
test(bluetooth): add unit tests for pairing process
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

## Pull Request Process

### Creating a Pull Request

1. **Ensure your code follows the project standards**
2. **Add tests for new functionality**
3. **Update documentation if needed**
4. **Test your changes thoroughly**
5. **Submit the pull request**

### Pull Request Template

```markdown
## Description
Brief description of the changes

## Type of Change
- [ ] Bug fix (non-breaking change that fixes an issue)
- [ ] New feature (non-breaking change that adds functionality)
- [ ] Breaking change (fix or feature that would cause existing functionality to not work as expected)
- [ ] Documentation update

## How Has This Been Tested?
Describe the tests you ran to verify your changes

## Checklist
- [ ] My code follows the project's style guidelines
- [ ] I have performed a self-review of my code
- [ ] I have commented my code, particularly in hard-to-understand areas
- [ ] I have made corresponding changes to the documentation
- [ ] My changes generate no new warnings
- [ ] I have added tests that prove my fix is effective or that my feature works
- [ ] New and existing unit tests pass locally with my changes
```

### Review Process

1. **Automated checks** must pass (build, tests, linting)
2. **Code review** by project maintainers
3. **Testing** on actual hardware when applicable
4. **Documentation review** for user-facing changes
5. **Approval** and merge by maintainers

## Issue Reporting

### Bug Reports

Use the bug report template:

```markdown
**Describe the Bug**
A clear and concise description of what the bug is.

**To Reproduce**
Steps to reproduce the behavior:
1. Go to '...'
2. Click on '....'
3. Scroll down to '....'
4. See error

**Expected Behavior**
A clear and concise description of what you expected to happen.

**Screenshots**
If applicable, add screenshots to help explain your problem.

**Environment:**
- OS: [e.g. Raspberry Pi OS Bullseye]
- .NET Version: [e.g. 9.0.0]
- Zapper Version: [e.g. 1.0.0]
- Hardware: [e.g. Raspberry Pi 4B 4GB]

**Additional Context**
Add any other context about the problem here.
```

### Feature Requests

```markdown
**Is your feature request related to a problem?**
A clear and concise description of what the problem is.

**Describe the solution you'd like**
A clear and concise description of what you want to happen.

**Describe alternatives you've considered**
A clear and concise description of any alternative solutions or features you've considered.

**Additional context**
Add any other context or screenshots about the feature request here.
```

## Development Standards

### Code Style

#### C# Guidelines

Follow the [.NET coding conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions):

```csharp
// Use PascalCase for public members
public class DeviceService
{
    // Use camelCase for private fields with underscore prefix
    private readonly ILogger<DeviceService> _logger;
    
    // Use meaningful names
    public async Task<Device> GetDeviceByIdAsync(int deviceId)
    {
        // Use var when type is obvious
        var device = await _context.Devices.FindAsync(deviceId);
        
        // Use guard clauses
        if (device == null)
            throw new NotFoundException($"Device with ID {deviceId} not found");
            
        return device;
    }
}
```

#### JavaScript/TypeScript Guidelines

```javascript
// Use camelCase for variables and functions
const deviceManager = new DeviceManager();

// Use descriptive function names
function sendCommandToDevice(deviceId, commandName) {
    // Use consistent indentation (2 spaces)
    return fetch(`/api/devices/${deviceId}/command`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ commandName })
    });
}

// Use async/await for asynchronous operations
async function executeActivity(activityId) {
    try {
        const response = await fetch(`/api/activities/${activityId}/execute`, {
            method: 'POST'
        });
        
        if (!response.ok) {
            throw new Error(`Failed to execute activity: ${response.statusText}`);
        }
        
        return await response.json();
    } catch (error) {
        console.error('Activity execution failed:', error);
        throw error;
    }
}
```

### Error Handling

```csharp
// Use specific exception types
public async Task<Device> GetDeviceAsync(int id)
{
    try
    {
        var device = await _context.Devices.FindAsync(id);
        return device ?? throw new DeviceNotFoundException($"Device {id} not found");
    }
    catch (DeviceNotFoundException)
    {
        // Re-throw domain exceptions
        throw;
    }
    catch (Exception ex)
    {
        // Log and wrap unexpected exceptions
        _logger.LogError(ex, "Unexpected error retrieving device {DeviceId}", id);
        throw new DeviceServiceException("Failed to retrieve device", ex);
    }
}
```

### Logging

```csharp
// Use structured logging
_logger.LogInformation("Device {DeviceId} command {Command} executed successfully", 
                      deviceId, commandName);

// Log levels appropriately
_logger.LogDebug("IR code transmission details: {HexCode}", irCode.HexCode);
_logger.LogInformation("Device {DeviceName} connected", device.Name);
_logger.LogWarning("Device {DeviceName} not responding, retrying...", device.Name);
_logger.LogError(ex, "Failed to execute command {Command} on device {DeviceId}", 
                command, deviceId);
```

## Testing Guidelines

### Unit Tests

```csharp
[Test]
public async Task SendCommandAsync_WithValidDevice_ShouldExecuteSuccessfully()
{
    // Arrange
    var deviceId = 1;
    var command = "Power";
    var mockTransmitter = new Mock<IInfraredTransmitter>();
    var service = new DeviceService(mockTransmitter.Object);
    
    // Act
    var result = await service.SendCommandAsync(deviceId, command);
    
    // Assert
    Assert.That(result.Success, Is.True);
    mockTransmitter.Verify(x => x.TransmitAsync(It.IsAny<string>(), 1, default), Times.Once);
}
```

### Integration Tests

```csharp
[Test]
public async Task DeviceController_CreateDevice_ShouldPersistToDatabase()
{
    // Arrange
    using var factory = new WebApplicationFactory<Program>();
    var client = factory.CreateClient();
    
    var deviceDto = new CreateDeviceRequest
    {
        Name = "Test TV",
        Brand = "Samsung",
        Type = DeviceType.Television
    };
    
    // Act
    var response = await client.PostAsJsonAsync("/api/devices", deviceDto);
    
    // Assert
    response.EnsureSuccessStatusCode();
    var device = await response.Content.ReadFromJsonAsync<Device>();
    Assert.That(device.Name, Is.EqualTo("Test TV"));
}
```

### Hardware Tests

For hardware-specific features:

```csharp
[Test]
[Category("Hardware")]
public async Task IRTransmitter_TransmitCode_ShouldActivateGPIO()
{
    // This test requires actual Raspberry Pi hardware
    // Skip if running in CI/CD environment
    if (!HardwareDetection.IsRaspberryPi())
        Assert.Ignore("Hardware test requires Raspberry Pi");
        
    // Test actual GPIO transmission
    var transmitter = new GpioInfraredTransmitter(18, Mock.Of<ILogger>());
    transmitter.Initialize();
    
    await transmitter.TransmitAsync("0xE0E040BF");
    
    // Verify GPIO state changes (requires additional hardware or mock)
}
```

## Documentation

### Code Documentation

```csharp
/// <summary>
/// Sends an infrared command to the specified device.
/// </summary>
/// <param name="deviceId">The unique identifier of the target device.</param>
/// <param name="commandName">The name of the command to execute (e.g., "Power", "VolumeUp").</param>
/// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
/// <returns>A task representing the asynchronous operation, containing the command execution result.</returns>
/// <exception cref="DeviceNotFoundException">Thrown when the device is not found.</exception>
/// <exception cref="CommandNotSupportedException">Thrown when the command is not supported by the device.</exception>
public async Task<CommandResult> SendCommandAsync(int deviceId, string commandName, CancellationToken cancellationToken = default)
```

### API Documentation

Update OpenAPI/Swagger documentation:

```csharp
[HttpPost("{id}/command")]
[ProducesResponseType(typeof(CommandResult), 200)]
[ProducesResponseType(404)]
[ProducesResponseType(400)]
public async Task<IActionResult> SendCommand(
    [FromRoute] int id,
    [FromBody] SendCommandRequest request)
```

### README Updates

When adding new features:

1. Update feature list in main README
2. Add configuration examples
3. Update hardware requirements if applicable
4. Add troubleshooting notes

## Release Process

### Version Numbering

Follow [Semantic Versioning](https://semver.org/):

- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes (backward compatible)

### Changelog

Update CHANGELOG.md with each release:

```markdown
## [1.2.0] - 2025-01-20

### Added
- Support for Sony IR protocol
- Bluetooth LE device support
- Activity scheduling feature

### Changed
- Improved WebOS discovery reliability
- Updated web interface styling

### Fixed
- IR timing accuracy issues
- Memory leak in device polling

### Deprecated
- Legacy API endpoints (will be removed in v2.0)
```

## Community

### Communication Channels

- **GitHub Issues**: Bug reports and feature requests
- **GitHub Discussions**: General questions and community chat
- **Pull Requests**: Code review and collaboration

### Getting Help

- Check the [documentation](../README.md)
- Search existing [issues](https://github.com/yourusername/zapper-next-gen/issues)
- Ask in [discussions](https://github.com/yourusername/zapper-next-gen/discussions)

### Recognition

Contributors will be recognized in:
- CONTRIBUTORS.md file
- Release notes
- Annual contributor spotlight

---

**Thank you for contributing to Zapper! Together, we're building the best open-source universal remote control system.** 🚀