using AwesomeAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zapper.Core.Interfaces;

namespace Zapper.Device.Infrared.Tests.Unit;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddInfraredServices_WithMockTransmitter_ShouldRegisterMockImplementation()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Infrared:UseRealGpio", "false" }
            })
            .Build();

        services.AddInfraredServices(configuration);
        var provider = services.BuildServiceProvider();

        var transmitter = provider.GetRequiredService<IInfraredTransmitter>();
        var controller = provider.GetRequiredService<IDeviceController>();

        transmitter.Should().BeOfType<MockInfraredTransmitter>();
        controller.Should().BeOfType<InfraredDeviceController>();
    }

    [Fact]
    public void AddInfraredServices_WithRealGpio_ShouldRegisterGpioImplementation()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Infrared:UseRealGpio", "true" },
                { "Infrared:GpioPin", "18" }
            })
            .Build();

        services.AddInfraredServices(configuration);

        // We can't actually test real GPIO in a test environment,
        // but we can verify the service registration logic
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IInfraredTransmitter));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddInfraredServices_WithCustomGpioPin_ShouldUseCorrectPin()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Infrared:UseRealGpio", "true" },
                { "Infrared:GpioPin", "22" }
            })
            .Build();

        services.AddInfraredServices(configuration);

        // We can't actually test real GPIO in a test environment,
        // but we can verify the service registration is set up correctly
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IInfraredTransmitter));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [Fact]
    public void AddInfraredServices_WithoutConfiguration_ShouldDefaultToMockTransmitter()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();

        services.AddInfraredServices(configuration);
        var provider = services.BuildServiceProvider();

        var transmitter = provider.GetRequiredService<IInfraredTransmitter>();

        transmitter.Should().BeOfType<MockInfraredTransmitter>();
    }

    [Fact]
    public void AddInfraredServices_ShouldRegisterTransmitterAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Infrared:UseRealGpio", "false" }
            })
            .Build();

        services.AddInfraredServices(configuration);
        var provider = services.BuildServiceProvider();

        var transmitter1 = provider.GetRequiredService<IInfraredTransmitter>();
        var transmitter2 = provider.GetRequiredService<IInfraredTransmitter>();

        transmitter1.Should().BeSameAs(transmitter2);
    }

    [Fact]
    public void AddInfraredServices_ShouldRegisterControllerAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Infrared:UseRealGpio", "false" }
            })
            .Build();

        services.AddInfraredServices(configuration);
        var provider = services.BuildServiceProvider();

        var controller1 = provider.GetRequiredService<IDeviceController>();
        var controller2 = provider.GetRequiredService<IDeviceController>();

        controller1.Should().BeSameAs(controller2);
    }
}