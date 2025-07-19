using FluentValidation.TestHelper;
using Zapper.Client.Devices;

namespace Zapper.API.Tests.Unit.Validators.Devices.Bluetooth;

public class BluetoothControlRequestValidatorTests
{
    private readonly BluetoothControlRequestValidator _validator = new();

    [Fact]
    public void Should_Have_No_Errors_When_Request_Is_Valid()
    {
        var request = new BluetoothControlRequest
        {
            Action = "connect",
            DeviceId = "00:11:22:33:44:55",
            Text = "Test text"
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Action_Is_Empty()
    {
        var request = new BluetoothControlRequest { Action = "" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Action)
            .WithErrorMessage("Action is required");
    }

    [Fact]
    public void Should_Have_Error_When_Action_Exceeds_Max_Length()
    {
        var request = new BluetoothControlRequest { Action = new string('A', 51) };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Action)
            .WithErrorMessage("Action must not exceed 50 characters");
    }

    [Fact]
    public void Should_Have_Error_When_DeviceId_Is_Empty_With_Non_Empty_Action()
    {
        var request = new BluetoothControlRequest { Action = "connect", DeviceId = "" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DeviceId)
            .WithErrorMessage("Device ID is required when performing device actions");
    }

    [Fact]
    public void Should_Have_Error_When_Text_Exceeds_Max_Length()
    {
        var request = new BluetoothControlRequest
        {
            Action = "sendText",
            DeviceId = "00:11:22:33:44:55",
            Text = new string('A', 1001)
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Text)
            .WithErrorMessage("Text must not exceed 1000 characters");
    }

    [Fact]
    public void Should_Have_No_Error_When_Text_Is_Null()
    {
        var request = new BluetoothControlRequest
        {
            Action = "connect",
            DeviceId = "00:11:22:33:44:55",
            Text = null
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Text);
    }
}