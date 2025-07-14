using FluentAssertions;

namespace Zapper.Device.USB.Tests.Unit;

public class RemoteButtonEventArgsTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        var deviceId = "TEST:001:remote";
        var buttonName = "Power";
        var keyCode = 0x01;

        var eventArgs = new RemoteButtonEventArgs(deviceId, buttonName, keyCode);

        eventArgs.DeviceId.Should().Be(deviceId);
        eventArgs.ButtonName.Should().Be(buttonName);
        eventArgs.KeyCode.Should().Be(keyCode);
        eventArgs.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Constructor_WithEmptyDeviceId_ShouldSetProperty()
    {
        var eventArgs = new RemoteButtonEventArgs("", "VolumeUp", 0x02);

        eventArgs.DeviceId.Should().Be("");
        eventArgs.ButtonName.Should().Be("VolumeUp");
        eventArgs.KeyCode.Should().Be(0x02);
    }

    [Fact]
    public void Constructor_WithEmptyButtonName_ShouldSetProperty()
    {
        var eventArgs = new RemoteButtonEventArgs("DEVICE:001", "", 0x03);

        eventArgs.DeviceId.Should().Be("DEVICE:001");
        eventArgs.ButtonName.Should().Be("");
        eventArgs.KeyCode.Should().Be(0x03);
    }

    [Fact]
    public void Constructor_WithZeroKeyCode_ShouldSetProperty()
    {
        var eventArgs = new RemoteButtonEventArgs("DEVICE:001", "Menu", 0);

        eventArgs.DeviceId.Should().Be("DEVICE:001");
        eventArgs.ButtonName.Should().Be("Menu");
        eventArgs.KeyCode.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithMaxKeyCode_ShouldSetProperty()
    {
        var eventArgs = new RemoteButtonEventArgs("DEVICE:001", "Unknown", 255);

        eventArgs.DeviceId.Should().Be("DEVICE:001");
        eventArgs.ButtonName.Should().Be("Unknown");
        eventArgs.KeyCode.Should().Be(255);
    }

    [Fact]
    public void Timestamp_ShouldBeUtc()
    {
        var eventArgs = new RemoteButtonEventArgs("DEVICE:001", "OK", 0x0B);

        eventArgs.Timestamp.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task MultipleInstancesCreatedQuickly_ShouldHaveDifferentTimestamps()
    {
        var eventArgs1 = new RemoteButtonEventArgs("DEVICE:001", "Button1", 1);
        await Task.Delay(1); // Ensure some time passes
        var eventArgs2 = new RemoteButtonEventArgs("DEVICE:002", "Button2", 2);

        eventArgs1.Timestamp.Should().BeBefore(eventArgs2.Timestamp);
    }

    [Theory]
    [InlineData("USB:046D:C52B:Logitech", "Power", 0x01)]
    [InlineData("HID:054C:0268:Sony", "VolumeUp", 0x02)]
    [InlineData("MOCK:0001:Test", "Menu", 0x0C)]
    public void Constructor_WithVariousInputs_ShouldSetPropertiesCorrectly(
        string deviceId, string buttonName, int keyCode)
    {
        var eventArgs = new RemoteButtonEventArgs(deviceId, buttonName, keyCode);

        eventArgs.DeviceId.Should().Be(deviceId);
        eventArgs.ButtonName.Should().Be(buttonName);
        eventArgs.KeyCode.Should().Be(keyCode);
        eventArgs.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void InheritsFromEventArgs_ShouldBeTrue()
    {
        var eventArgs = new RemoteButtonEventArgs("DEVICE:001", "Test", 1);

        eventArgs.Should().BeAssignableTo<EventArgs>();
    }
}