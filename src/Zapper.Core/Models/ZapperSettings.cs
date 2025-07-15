namespace Zapper.Core.Models;

public class ZapperSettings
{
    public GeneralSettings General { get; set; } = new();
    public DeviceSettings Device { get; set; } = new();
    public NetworkSettings Network { get; set; } = new();
    public HardwareSettings Hardware { get; set; } = new();
    public AdvancedSettings Advanced { get; set; } = new();
}

public class GeneralSettings
{
    public bool EnableNotifications { get; set; } = true;
    public bool EnableHapticFeedback { get; set; } = true;
    public bool EnableAutoDiscovery { get; set; } = true;
    public string DefaultActivity { get; set; } = "watch-tv";
}

public class DeviceSettings
{
    public int DeviceTimeout { get; set; } = 30;
    public int RetryAttempts { get; set; } = 3;
    public int IrPowerLevel { get; set; } = 5;
}

public class NetworkSettings
{
    public int DiscoveryPort { get; set; } = 1900;
    public int ApiTimeout { get; set; } = 5000;
    public bool EnableSsdp { get; set; } = true;
}

public class HardwareSettings
{
    public bool EnableGpio { get; set; } = true;
    public IrHardwareSettings Infrared { get; set; } = new();
}

public class IrHardwareSettings
{
    public int TransmitterGpioPin { get; set; } = 18;
    public int ReceiverGpioPin { get; set; } = 19;
    public int CarrierFrequency { get; set; } = 38000;
    public double DutyCycle { get; set; } = 0.33;
}

public class AdvancedSettings
{
    public bool EnableDebugLogging { get; set; } = false;
    public bool EnableTelemetry { get; set; } = true;
}