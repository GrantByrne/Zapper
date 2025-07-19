using FluentValidation.TestHelper;
using Zapper.Client.UsbRemotes;
using Zapper.Core.Models;

namespace Zapper.API.Tests.Unit.Validators.UsbRemotes;

public class CreateButtonMappingRequestValidatorTests
{
    private readonly CreateButtonMappingRequestValidator _validator = new();

    [Fact]
    public void Should_Have_No_Errors_When_Request_Is_Valid()
    {
        var request = new CreateButtonMappingRequest
        {
            ButtonId = 1,
            DeviceId = 1,
            DeviceCommandId = 1,
            EventType = ButtonEventType.KeyPress
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_ButtonId_Is_Zero()
    {
        var request = new CreateButtonMappingRequest
        {
            ButtonId = 0,
            DeviceId = 1,
            DeviceCommandId = 1,
            EventType = ButtonEventType.KeyPress
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ButtonId)
            .WithErrorMessage("Button ID must be greater than 0");
    }

    [Fact]
    public void Should_Have_Error_When_DeviceId_Is_Zero()
    {
        var request = new CreateButtonMappingRequest
        {
            ButtonId = 1,
            DeviceId = 0,
            DeviceCommandId = 1,
            EventType = ButtonEventType.KeyPress
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DeviceId)
            .WithErrorMessage("Device ID must be greater than 0");
    }

    [Fact]
    public void Should_Have_Error_When_DeviceCommandId_Is_Zero()
    {
        var request = new CreateButtonMappingRequest
        {
            ButtonId = 1,
            DeviceId = 1,
            DeviceCommandId = 0,
            EventType = ButtonEventType.KeyPress
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DeviceCommandId)
            .WithErrorMessage("Device Command ID must be greater than 0");
    }

    [Theory]
    [InlineData(ButtonEventType.KeyPress)]
    [InlineData(ButtonEventType.KeyDown)]
    [InlineData(ButtonEventType.KeyUp)]
    [InlineData(ButtonEventType.LongPress)]
    public void Should_Have_No_Errors_For_Valid_EventTypes(ButtonEventType eventType)
    {
        var request = new CreateButtonMappingRequest
        {
            ButtonId = 1,
            DeviceId = 1,
            DeviceCommandId = 1,
            EventType = eventType
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_EventType_Is_Invalid()
    {
        var request = new CreateButtonMappingRequest
        {
            ButtonId = 1,
            DeviceId = 1,
            DeviceCommandId = 1,
            EventType = (ButtonEventType)999
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.EventType)
            .WithErrorMessage("Invalid event type");
    }
}