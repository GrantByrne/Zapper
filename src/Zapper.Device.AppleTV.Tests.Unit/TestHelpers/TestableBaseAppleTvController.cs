using Microsoft.Extensions.Logging;
using Zapper.Core.Models;
using Zapper.Device.AppleTV.Controllers;
using Zapper.Device.AppleTV.Models;

namespace Zapper.Device.AppleTV.Tests.Unit.TestHelpers;

public class TestableBaseAppleTvController : BaseAppleTvController
{
    public bool EstablishConnectionAsyncCalled { get; private set; }
    public bool CloseConnectionAsyncCalled { get; private set; }
    public bool ShouldEstablishConnectionSucceed { get; set; } = true;
    public bool ShouldSendCommandSucceed { get; set; } = true;
    public DeviceCommand? LastSentCommand { get; private set; }
    public AppleTvStatus? StatusToReturn { get; set; }
    public bool ShouldPairSucceed { get; set; } = true;
    public string? LastPairingPin { get; private set; }

    public TestableBaseAppleTvController(ILogger<BaseAppleTvController> logger) : base(logger)
    {
    }

    public override ConnectionType SupportedProtocol => ConnectionType.MediaRemoteProtocol;

    protected override Task<bool> EstablishConnectionAsync(Zapper.Core.Models.Device device)
    {
        EstablishConnectionAsyncCalled = true;
        return Task.FromResult(ShouldEstablishConnectionSucceed);
    }

    protected override Task CloseConnectionAsync()
    {
        CloseConnectionAsyncCalled = true;
        return Task.CompletedTask;
    }

    public override Task<bool> SendCommandAsync(DeviceCommand command)
    {
        LastSentCommand = command;
        return Task.FromResult(ShouldSendCommandSucceed);
    }

    public override Task<AppleTvStatus?> GetStatusAsync()
    {
        return Task.FromResult(StatusToReturn);
    }

    public override Task<bool> PairAsync(string pin)
    {
        LastPairingPin = pin;
        return Task.FromResult(ShouldPairSucceed);
    }

    // Expose protected method for testing
    public CommandCode TestMapDeviceCommand(DeviceCommand command)
    {
        return MapDeviceCommand(command);
    }

    // Expose protected fields for testing
    public Zapper.Core.Models.Device? GetConnectedDevice() => ConnectedDevice;
    public bool GetIsConnected() => IsConnected;
    public void SetIsConnected(bool value) => IsConnected = value;
}