using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Zapper.Core.Models;
using Zapper.Device.AppleTV.Controllers;
using Zapper.Device.AppleTV.Interfaces;
using Zapper.Device.AppleTV.Services;

namespace Zapper.Device.AppleTV.Tests.Unit.Services;

public class AppleTvControllerFactoryTests
{
    private readonly IServiceProvider _mockServiceProvider;
    private readonly ILogger<AppleTvControllerFactory> _mockLogger;
    private readonly AppleTvControllerFactory _factory;
    private readonly CompanionProtocolController _mockCompanionController;
    private readonly MrpProtocolController _mockMrpController;

    public AppleTvControllerFactoryTests()
    {
        _mockServiceProvider = Substitute.For<IServiceProvider>();
        _mockLogger = Substitute.For<ILogger<AppleTvControllerFactory>>();

        // Create mocks of concrete types with required constructor parameters
        _mockCompanionController = Substitute.For<CompanionProtocolController>(
            Substitute.For<ILogger<CompanionProtocolController>>());
        _mockMrpController = Substitute.For<MrpProtocolController>(
            Substitute.For<ILogger<MrpProtocolController>>());

        // Mock GetService instead of GetRequiredService (which is an extension method)
        _mockServiceProvider.GetService(typeof(CompanionProtocolController)).Returns(_mockCompanionController);
        _mockServiceProvider.GetService(typeof(MrpProtocolController)).Returns(_mockMrpController);

        _factory = new AppleTvControllerFactory(_mockServiceProvider, _mockLogger);
    }

    [Fact]
    public void CreateController_CompanionProtocol_ReturnsCompanionController()
    {
        // Act
        var result = _factory.CreateController(ConnectionType.CompanionProtocol);

        // Assert
        result.Should().Be(_mockCompanionController);
    }

    [Fact]
    public void CreateController_MediaRemoteProtocol_ReturnsMrpController()
    {
        // Act
        var result = _factory.CreateController(ConnectionType.MediaRemoteProtocol);

        // Assert
        result.Should().Be(_mockMrpController);
    }

    [Fact]
    public void CreateController_DacpProtocol_ThrowsNotImplementedException()
    {
        // Act & Assert
        Assert.Throws<NotImplementedException>(() =>
            _factory.CreateController(ConnectionType.Dacp));
    }

    [Fact]
    public void CreateController_AirPlayProtocol_ThrowsNotImplementedException()
    {
        // Act & Assert
        Assert.Throws<NotImplementedException>(() =>
            _factory.CreateController(ConnectionType.AirPlay));
    }

    [Fact]
    public void CreateController_UnsupportedProtocol_ThrowsNotSupportedException()
    {
        // Act & Assert
        Assert.Throws<NotSupportedException>(() =>
            _factory.CreateController(ConnectionType.Bluetooth));
    }

    [Fact]
    public void CreateControllerForDevice_Version15OrHigher_ReturnsCompanionController()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            ProtocolVersion = "15.0",
            ConnectionType = ConnectionType.MediaRemoteProtocol // Should be overridden by version
        };

        // Act
        var result = _factory.CreateControllerForDevice(device);

        // Assert
        result.Should().Be(_mockCompanionController);
    }

    [Fact]
    public void CreateControllerForDevice_Version16_ReturnsCompanionController()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            ProtocolVersion = "16.4.1"
        };

        // Act
        var result = _factory.CreateControllerForDevice(device);

        // Assert
        result.Should().Be(_mockCompanionController);
    }

    [Fact]
    public void CreateControllerForDevice_Version14_ReturnsMrpController()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            ProtocolVersion = "14.7"
        };

        // Act
        var result = _factory.CreateControllerForDevice(device);

        // Assert
        result.Should().Be(_mockMrpController);
    }

    [Fact]
    public void CreateControllerForDevice_NoVersion_UsesConnectionType()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            ProtocolVersion = null,
            ConnectionType = ConnectionType.CompanionProtocol
        };

        // Act
        var result = _factory.CreateControllerForDevice(device);

        // Assert
        result.Should().Be(_mockCompanionController);
    }

    [Fact]
    public void CreateControllerForDevice_InvalidVersion_UsesConnectionType()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            ProtocolVersion = "invalid.version",
            ConnectionType = ConnectionType.MediaRemoteProtocol
        };

        // Act
        var result = _factory.CreateControllerForDevice(device);

        // Assert
        result.Should().Be(_mockMrpController);
    }

    [Fact]
    public void CreateControllerForDevice_NoVersionOrConnectionType_DefaultsToMrp()
    {
        // Arrange
        var device = new Zapper.Core.Models.Device
        {
            ProtocolVersion = null,
            ConnectionType = ConnectionType.NetworkHttp // Not a valid Apple TV connection type
        };

        // Act
        var result = _factory.CreateControllerForDevice(device);

        // Assert
        result.Should().Be(_mockMrpController);
    }

    [Fact]
    public void CreateControllerForDevice_ServiceProviderReturnsNull_ThrowsInvalidOperationException()
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(MrpProtocolController)).Returns(null);

        var factory = new AppleTvControllerFactory(serviceProvider, _mockLogger);
        var device = new Zapper.Core.Models.Device();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            factory.CreateControllerForDevice(device));
    }
}