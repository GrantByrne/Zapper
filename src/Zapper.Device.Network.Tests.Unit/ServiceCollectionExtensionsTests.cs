using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Zapper.Device.Network.Tests.Unit;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNetworkServices_ShouldRegisterRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddNetworkServices();
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var networkController = serviceProvider.GetService<INetworkDeviceController>();
        networkController.Should().NotBeNull();
        networkController.Should().BeOfType<NetworkDeviceController>();

        var httpClient = serviceProvider.GetService<HttpClient>();
        httpClient.Should().NotBeNull();
    }

    [Fact]
    public void AddNetworkServices_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddNetworkServices();

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddNetworkServices_CalledMultipleTimes_ShouldNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var act = () =>
        {
            services.AddNetworkServices();
            services.AddNetworkServices();
        };
        act.Should().NotThrow();
    }
}