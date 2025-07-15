using Zapper.Core.Models;
using Microsoft.Extensions.Logging;

namespace Zapper.Device.Infrared;

public class MockInfraredReceiver(ILogger<MockInfraredReceiver> logger) : IInfraredReceiver
{
    private bool _isInitialized;

    public bool IsAvailable => _isInitialized;

    public void Initialize()
    {
        _isInitialized = true;
        logger.LogInformation("Mock IR receiver initialized");
    }

    public async Task<IrCode?> ReceiveAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Mock IR receiver simulating command reception");

        await Task.Delay(1000, cancellationToken);

        var mockCode = new IrCode
        {
            Brand = "Mock",
            Model = "Remote",
            CommandName = "TestCommand",
            Protocol = "NEC",
            HexCode = "0x12345678",
            RawData = "9000 4500 560 1690 560 560 560 1690 560 560",
            Frequency = 38000
        };

        logger.LogInformation("Mock IR receiver returned simulated NEC command");
        return mockCode;
    }

    public async Task<int[]?> ReceiveRawAsync(TimeSpan timeout, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Mock IR receiver simulating raw signal reception");

        await Task.Delay(1000, cancellationToken);

        var mockPulses = new int[] { 9000, 4500, 560, 1690, 560, 560, 560, 1690, 560, 560 };

        logger.LogInformation("Mock IR receiver returned simulated raw pulses");
        return mockPulses;
    }

    public void Dispose()
    {
        logger.LogInformation("Mock IR receiver disposed");
    }
}