using FastEndpoints;
using FluentValidation;
using Zapper.Client.IRCodes;

namespace Zapper.API.Validators.IRCodes;

public class ImportExternalCodeSetRequestValidator : Validator<ImportExternalCodeSetRequest>
{
    public ImportExternalCodeSetRequestValidator()
    {
        RuleFor(x => x.Manufacturer)
            .NotEmpty().WithMessage("Manufacturer is required")
            .MaximumLength(100).WithMessage("Manufacturer must not exceed 100 characters");

        RuleFor(x => x.DeviceType)
            .NotEmpty().WithMessage("Device type is required")
            .MaximumLength(50).WithMessage("Device type must not exceed 50 characters");

        RuleFor(x => x.Device)
            .NotEmpty().WithMessage("Device is required")
            .MaximumLength(100).WithMessage("Device must not exceed 100 characters");

        RuleFor(x => x.Subdevice)
            .NotEmpty().WithMessage("Subdevice is required")
            .MaximumLength(100).WithMessage("Subdevice must not exceed 100 characters");
    }
}