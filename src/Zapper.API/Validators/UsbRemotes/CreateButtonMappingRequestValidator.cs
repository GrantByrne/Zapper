using FastEndpoints;
using FluentValidation;
using Zapper.Client.UsbRemotes;

namespace Zapper.API.Validators.UsbRemotes;

public class CreateButtonMappingRequestValidator : Validator<CreateButtonMappingRequest>
{
    public CreateButtonMappingRequestValidator()
    {
        RuleFor(x => x.ButtonId)
            .GreaterThan(0).WithMessage("Button ID must be greater than 0");

        RuleFor(x => x.DeviceId)
            .GreaterThan(0).WithMessage("Device ID must be greater than 0");

        RuleFor(x => x.DeviceCommandId)
            .GreaterThan(0).WithMessage("Device Command ID must be greater than 0");

        RuleFor(x => x.EventType)
            .IsInEnum().WithMessage("Invalid event type");
    }
}