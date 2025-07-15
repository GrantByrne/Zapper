using AwesomeAssertions;

namespace Zapper.Device.Bluetooth.Tests.Unit;

public class BluetoothDeviceEventArgsTests
{
    [Fact]
    public void Constructor_WithValidDevice_ShouldSetDevice()
    {
        // Arrange
        var deviceInfo = new BluetoothDeviceInfo
        {
            Address = "AA:BB:CC:DD:EE:FF",
            Name = "Test Device"
        };

        // Act
        var eventArgs = new BluetoothDeviceEventArgs(deviceInfo);

        // Assert
        eventArgs.Device.Should().BeSameAs(deviceInfo);
    }

    [Fact]
    public void Constructor_WithNullDevice_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var act = () => new BluetoothDeviceEventArgs(null!);
        act.Should().Throw<ArgumentNullException>().WithParameterName("device");
    }
}