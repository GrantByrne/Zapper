using AwesomeAssertions;

namespace Zapper.Device.Bluetooth.Tests.Unit;

public class BluetoothDeviceInfoTests
{
    [Fact]
    public void BluetoothDeviceInfo_DefaultConstructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var deviceInfo = new BluetoothDeviceInfo();

        // Assert
        deviceInfo.Address.Should().BeEmpty();
        deviceInfo.Name.Should().BeEmpty();
        deviceInfo.Alias.Should().BeNull();
        deviceInfo.IsConnected.Should().BeFalse();
        deviceInfo.IsPaired.Should().BeFalse();
        deviceInfo.IsTrusted.Should().BeFalse();
        deviceInfo.IsBlocked.Should().BeFalse();
        deviceInfo.Rssi.Should().BeNull();
        deviceInfo.TxPower.Should().BeNull();
        deviceInfo.Class.Should().BeNull();
        deviceInfo.UUIDs.Should().BeEmpty();
    }

    [Fact]
    public void BluetoothDeviceInfo_SetProperties_ShouldRetainValues()
    {
        // Arrange
        var deviceInfo = new BluetoothDeviceInfo();
        var expectedAddress = "AA:BB:CC:DD:EE:FF";
        var expectedName = "Test Device";
        var expectedAlias = "My Test Device";
        var expectedUuids = new[] { "00001124-0000-1000-8000-00805f9b34fb" };

        // Act
        deviceInfo.Address = expectedAddress;
        deviceInfo.Name = expectedName;
        deviceInfo.Alias = expectedAlias;
        deviceInfo.IsConnected = true;
        deviceInfo.IsPaired = true;
        deviceInfo.IsTrusted = true;
        deviceInfo.IsBlocked = false;
        deviceInfo.Rssi = -50;
        deviceInfo.TxPower = 4;
        deviceInfo.Class = 0x240404;
        deviceInfo.UUIDs = expectedUuids;

        // Assert
        deviceInfo.Address.Should().Be(expectedAddress);
        deviceInfo.Name.Should().Be(expectedName);
        deviceInfo.Alias.Should().Be(expectedAlias);
        deviceInfo.IsConnected.Should().BeTrue();
        deviceInfo.IsPaired.Should().BeTrue();
        deviceInfo.IsTrusted.Should().BeTrue();
        deviceInfo.IsBlocked.Should().BeFalse();
        deviceInfo.Rssi.Should().Be(-50);
        deviceInfo.TxPower.Should().Be(4);
        deviceInfo.Class.Should().Be(0x240404u);
        deviceInfo.UUIDs.Should().BeEquivalentTo(expectedUuids);
    }
}

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