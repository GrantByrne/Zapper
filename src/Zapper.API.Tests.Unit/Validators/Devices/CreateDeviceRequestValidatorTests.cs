using Zapper.Core.Models;

namespace Zapper.API.Tests.Unit.Validators;

public class CreateDeviceRequestValidatorTests
{
    private readonly CreateDeviceRequestValidator _validator = new();

    [Fact]
    public void Should_Pass_When_Request_Is_Valid()
    {
        var request = new CreateDeviceRequest
        {
            Name = "Living Room TV",
            Brand = "Samsung",
            Model = "QN65Q80A",
            Type = DeviceType.Television,
            ConnectionType = ConnectionType.InfraredIr
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_When_Request_Has_Network_Details()
    {
        var request = new CreateDeviceRequest
        {
            Name = "Smart TV",
            Type = DeviceType.Television,
            ConnectionType = ConnectionType.Network,
            IpAddress = "192.168.1.100",
            MacAddress = "AA:BB:CC:DD:EE:FF",
            Port = 8080
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_Name_Is_Empty()
    {
        var request = new CreateDeviceRequest
        {
            Name = "",
            Type = DeviceType.Television,
            ConnectionType = ConnectionType.InfraredIr
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Device name is required");
    }

    [Fact]
    public void Should_Fail_When_Name_Is_Null()
    {
        var request = new CreateDeviceRequest
        {
            Name = null!,
            Type = DeviceType.Television,
            ConnectionType = ConnectionType.InfraredIr
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Device name is required");
    }

    [Fact]
    public void Should_Fail_When_Name_Exceeds_100_Characters()
    {
        var request = new CreateDeviceRequest
        {
            Name = new string('a', 101),
            Type = DeviceType.Television,
            ConnectionType = ConnectionType.InfraredIr
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Device name must not exceed 100 characters");
    }

    [Fact]
    public void Should_Pass_When_Brand_Is_Null()
    {
        var request = new CreateDeviceRequest
        {
            Name = "Living Room TV",
            Brand = null,
            Type = DeviceType.Television,
            ConnectionType = ConnectionType.InfraredIr
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Brand);
    }

    [Fact]
    public void Should_Fail_When_Brand_Exceeds_50_Characters()
    {
        var request = new CreateDeviceRequest
        {
            Name = "Living Room TV",
            Brand = new string('a', 51),
            Type = DeviceType.Television,
            ConnectionType = ConnectionType.InfraredIr
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Brand)
            .WithErrorMessage("Brand must not exceed 50 characters");
    }

    [Fact]
    public void Should_Pass_When_Model_Is_Null()
    {
        var request = new CreateDeviceRequest
        {
            Name = "Living Room TV",
            Model = null,
            Type = DeviceType.Television,
            ConnectionType = ConnectionType.InfraredIr
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Model);
    }

    [Fact]
    public void Should_Fail_When_Model_Exceeds_50_Characters()
    {
        var request = new CreateDeviceRequest
        {
            Name = "Living Room TV",
            Model = new string('a', 51),
            Type = DeviceType.Television,
            ConnectionType = ConnectionType.InfraredIr
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Model)
            .WithErrorMessage("Model must not exceed 50 characters");
    }

    [Theory]
    [InlineData(DeviceType.Television)]
    [InlineData(DeviceType.Receiver)]
    [InlineData(DeviceType.CableBox)]
    [InlineData(DeviceType.StreamingDevice)]
    [InlineData(DeviceType.GameConsole)]
    public void Should_Pass_When_Type_Is_Valid(DeviceType type)
    {
        var request = new CreateDeviceRequest
        {
            Name = "Device",
            Type = type,
            ConnectionType = ConnectionType.InfraredIr
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Type);
    }

    [Theory]
    [InlineData(ConnectionType.InfraredIr)]
    [InlineData(ConnectionType.Bluetooth)]
    [InlineData(ConnectionType.Network)]
    [InlineData(ConnectionType.NetworkTcp)]
    [InlineData(ConnectionType.Usb)]
    public void Should_Pass_When_ConnectionType_Is_Valid(ConnectionType connectionType)
    {
        var request = new CreateDeviceRequest
        {
            Name = "Device",
            Type = DeviceType.Television,
            ConnectionType = connectionType
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.ConnectionType);
    }

    [Theory]
    [InlineData("192.168.1.1")]
    [InlineData("10.0.0.1")]
    [InlineData("172.16.0.1")]
    [InlineData("255.255.255.255")]
    [InlineData("::1")]
    [InlineData("2001:0db8:85a3:0000:0000:8a2e:0370:7334")]
    public void Should_Pass_When_IpAddress_Is_Valid(string ipAddress)
    {
        var request = new CreateDeviceRequest
        {
            Name = "Device",
            Type = DeviceType.Television,
            ConnectionType = ConnectionType.Network,
            IpAddress = ipAddress
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.IpAddress);
    }

    [Theory]
    [InlineData("invalid.ip")]
    [InlineData("256.256.256.256")]
    [InlineData("192.168.1.256")]
    [InlineData("192.168.1.1.1")]
    [InlineData("not-an-ip")]
    public void Should_Fail_When_IpAddress_Is_Invalid(string ipAddress)
    {
        var request = new CreateDeviceRequest
        {
            Name = "Device",
            Type = DeviceType.Television,
            ConnectionType = ConnectionType.Network,
            IpAddress = ipAddress
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.IpAddress)
            .WithErrorMessage("Invalid IP address format");
    }

    [Fact]
    public void Should_Pass_When_IpAddress_Is_Null()
    {
        var request = new CreateDeviceRequest
        {
            Name = "Device",
            Type = DeviceType.Television,
            ConnectionType = ConnectionType.InfraredIr,
            IpAddress = null
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.IpAddress);
    }

    [Theory]
    [InlineData("AA:BB:CC:DD:EE:FF")]
    [InlineData("aa:bb:cc:dd:ee:ff")]
    [InlineData("AA-BB-CC-DD-EE-FF")]
    [InlineData("11:22:33:44:55:66")]
    public void Should_Pass_When_MacAddress_Is_Valid(string macAddress)
    {
        var request = new CreateDeviceRequest
        {
            Name = "Device",
            Type = DeviceType.Television,
            ConnectionType = ConnectionType.Network,
            MacAddress = macAddress
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.MacAddress);
    }

    [Theory]
    [InlineData("AA:BB:CC:DD:EE")]
    [InlineData("AA:BB:CC:DD:EE:FF:GG")]
    [InlineData("AABBCCDDEEFF")]
    [InlineData("ZZ:ZZ:ZZ:ZZ:ZZ:ZZ")]
    [InlineData("not-a-mac")]
    public void Should_Fail_When_MacAddress_Is_Invalid(string macAddress)
    {
        var request = new CreateDeviceRequest
        {
            Name = "Device",
            Type = DeviceType.Television,
            ConnectionType = ConnectionType.Network,
            MacAddress = macAddress
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.MacAddress)
            .WithErrorMessage("Invalid MAC address format");
    }

    [Fact]
    public void Should_Pass_When_MacAddress_Is_Null()
    {
        var request = new CreateDeviceRequest
        {
            Name = "Device",
            Type = DeviceType.Television,
            ConnectionType = ConnectionType.InfraredIr,
            MacAddress = null
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.MacAddress);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(80)]
    [InlineData(443)]
    [InlineData(8080)]
    [InlineData(65535)]
    public void Should_Pass_When_Port_Is_Valid(int port)
    {
        var request = new CreateDeviceRequest
        {
            Name = "Device",
            Type = DeviceType.Television,
            ConnectionType = ConnectionType.Network,
            Port = port
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Port);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(65536)]
    [InlineData(100000)]
    public void Should_Fail_When_Port_Is_Invalid(int port)
    {
        var request = new CreateDeviceRequest
        {
            Name = "Device",
            Type = DeviceType.Television,
            ConnectionType = ConnectionType.Network,
            Port = port
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Port)
            .WithErrorMessage("Port must be between 1 and 65535");
    }

    [Fact]
    public void Should_Pass_When_Port_Is_Null()
    {
        var request = new CreateDeviceRequest
        {
            Name = "Device",
            Type = DeviceType.Television,
            ConnectionType = ConnectionType.InfraredIr,
            Port = null
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Port);
    }

    [Fact]
    public void Should_Have_Multiple_Errors_For_Invalid_Request()
    {
        var request = new CreateDeviceRequest
        {
            Name = "",
            Brand = new string('a', 51),
            Model = new string('b', 51),
            Type = (DeviceType)999,
            ConnectionType = (ConnectionType)999,
            IpAddress = "invalid",
            MacAddress = "invalid",
            Port = 0
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Brand);
        result.ShouldHaveValidationErrorFor(x => x.Model);
        result.ShouldHaveValidationErrorFor(x => x.Type);
        result.ShouldHaveValidationErrorFor(x => x.ConnectionType);
        result.ShouldHaveValidationErrorFor(x => x.IpAddress);
        result.ShouldHaveValidationErrorFor(x => x.MacAddress);
        result.ShouldHaveValidationErrorFor(x => x.Port);
    }
}