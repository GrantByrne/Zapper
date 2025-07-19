using FluentValidation.TestHelper;
using Zapper.Client.Remotes;

namespace Zapper.API.Tests.Unit.Validators.Remotes;

public class StartBluetoothAdvertisingRequestValidatorTests
{
    private readonly StartBluetoothAdvertisingRequestValidator _validator = new();

    [Fact]
    public void Should_Have_No_Errors_When_Request_Is_Valid()
    {
        var request = new StartBluetoothAdvertisingRequest
        {
            RemoteName = "Living Room Remote",
            DeviceType = "TVRemote"
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_RemoteName_Is_Empty()
    {
        var request = new StartBluetoothAdvertisingRequest
        {
            RemoteName = "",
            DeviceType = "TVRemote"
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.RemoteName)
            .WithErrorMessage("Remote name is required");
    }

    [Fact]
    public void Should_Have_Error_When_RemoteName_Exceeds_Max_Length()
    {
        var request = new StartBluetoothAdvertisingRequest
        {
            RemoteName = new string('A', 51),
            DeviceType = "TVRemote"
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.RemoteName)
            .WithErrorMessage("Remote name must not exceed 50 characters");
    }

    [Fact]
    public void Should_Have_Error_When_DeviceType_Is_Empty()
    {
        var request = new StartBluetoothAdvertisingRequest
        {
            RemoteName = "Test Remote",
            DeviceType = ""
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DeviceType)
            .WithErrorMessage("Device type is required");
    }

    [Theory]
    [InlineData("TVRemote")]
    [InlineData("GameController")]
    [InlineData("MediaRemote")]
    public void Should_Have_No_Errors_For_Valid_DeviceTypes(string deviceType)
    {
        var request = new StartBluetoothAdvertisingRequest
        {
            RemoteName = "Test Remote",
            DeviceType = deviceType
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_DeviceType_Is_Invalid()
    {
        var request = new StartBluetoothAdvertisingRequest
        {
            RemoteName = "Test Remote",
            DeviceType = "InvalidType"
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DeviceType)
            .WithErrorMessage("Device type must be one of: TVRemote, GameController, MediaRemote");
    }

    [Fact]
    public void Should_Have_Multiple_Errors_When_Multiple_Properties_Are_Invalid()
    {
        var request = new StartBluetoothAdvertisingRequest
        {
            RemoteName = "",
            DeviceType = "InvalidType"
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.RemoteName);
        result.ShouldHaveValidationErrorFor(x => x.DeviceType);
    }
}