using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zapper.Core.Interfaces;

namespace Zapper.Device.USB.Tests.Unit;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddUsbServices_WithMockHandler_ShouldRegisterMockImplementation()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        
        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "USB:UseMockHandler", "true" }
            })
            .Build();

        services.AddUsbServices(configuration);
        var provider = services.BuildServiceProvider();

        var remoteHandler = provider.GetRequiredService<IUsbRemoteHandler>();
        var controller = provider.GetRequiredService<IDeviceController>();
        var hostedService = provider.GetRequiredService<IHostedService>();

        remoteHandler.Should().BeOfType<MockUsbRemoteHandler>();
        controller.Should().BeOfType<UsbDeviceController>();
        hostedService.Should().BeOfType<UsbRemoteHostedService>();
    }

    [Fact]
    public void AddUsbServices_WithRealHandler_ShouldRegisterRealImplementation()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        
        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "USB:UseMockHandler", "false" }
            })
            .Build();

        services.AddUsbServices(configuration);
        var provider = services.BuildServiceProvider();

        var remoteHandler = provider.GetRequiredService<IUsbRemoteHandler>();
        var controller = provider.GetRequiredService<IDeviceController>();

        remoteHandler.Should().BeOfType<UsbRemoteHandler>();
        controller.Should().BeOfType<UsbDeviceController>();
    }

    [Fact]
    public void AddUsbServices_WithoutConfiguration_ShouldDefaultToMockHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        
        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();

        services.AddUsbServices(configuration);
        var provider = services.BuildServiceProvider();

        var remoteHandler = provider.GetRequiredService<IUsbRemoteHandler>();

        remoteHandler.Should().BeOfType<MockUsbRemoteHandler>();
    }

    [Fact]
    public void AddUsbServices_ShouldRegisterHandlerAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        
        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "USB:UseMockHandler", "true" }
            })
            .Build();

        services.AddUsbServices(configuration);
        var provider = services.BuildServiceProvider();

        var handler1 = provider.GetRequiredService<IUsbRemoteHandler>();
        var handler2 = provider.GetRequiredService<IUsbRemoteHandler>();

        handler1.Should().BeSameAs(handler2);
    }

    [Fact]
    public void AddUsbServices_ShouldRegisterControllerAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        
        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "USB:UseMockHandler", "true" }
            })
            .Build();

        services.AddUsbServices(configuration);
        var provider = services.BuildServiceProvider();

        var controller1 = provider.GetRequiredService<IDeviceController>();
        var controller2 = provider.GetRequiredService<IDeviceController>();

        controller1.Should().BeSameAs(controller2);
    }

    [Fact]
    public void AddUsbServices_ShouldRegisterHostedServiceAsSingleton()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        
        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "USB:UseMockHandler", "true" }
            })
            .Build();

        services.AddUsbServices(configuration);
        var provider = services.BuildServiceProvider();

        var service1 = provider.GetRequiredService<IHostedService>();
        var service2 = provider.GetRequiredService<IHostedService>();

        service1.Should().BeSameAs(service2);
    }

    [Fact]
    public void AddUsbServices_ShouldRegisterAllRequiredServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        
        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();

        services.AddUsbServices(configuration);

        // Verify all expected service registrations exist
        var serviceDescriptors = services.ToList();
        
        serviceDescriptors.Should().Contain(s => s.ServiceType == typeof(IUsbRemoteHandler));
        serviceDescriptors.Should().Contain(s => s.ServiceType == typeof(IDeviceController));
        serviceDescriptors.Should().Contain(s => s.ServiceType == typeof(IHostedService));
    }

    [Theory]
    [InlineData("true", typeof(MockUsbRemoteHandler))]
    [InlineData("false", typeof(UsbRemoteHandler))]
    public void AddUsbServices_WithDifferentConfigValues_ShouldRegisterCorrectImplementation(
        string configValue, Type expectedType)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        
        var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "USB:UseMockHandler", configValue }
            })
            .Build();

        services.AddUsbServices(configuration);
        var provider = services.BuildServiceProvider();

        var remoteHandler = provider.GetRequiredService<IUsbRemoteHandler>();

        remoteHandler.Should().BeOfType(expectedType);
    }

    [Fact]
    public void AddUsbServices_WithNullConfiguration_ShouldThrow()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var act = () => services.AddUsbServices(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}