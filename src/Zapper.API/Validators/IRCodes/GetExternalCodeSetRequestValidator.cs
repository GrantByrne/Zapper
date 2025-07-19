using FastEndpoints;
using FluentValidation;
using Zapper.Client.IRCodes;

namespace Zapper.API.Validators.IRCodes;

public class GetExternalCodeSetRequestValidator : Validator<GetExternalCodeSetRequest>
{
    public GetExternalCodeSetRequestValidator()
    {
        RuleFor(x => x.Manufacturer)
            .NotEmpty().WithMessage("Manufacturer is required")
            .MaximumLength(100).WithMessage("Manufacturer must not exceed 100 characters");

        RuleFor(x => x.Device)
            .NotEmpty().WithMessage("Device is required")
            .MaximumLength(100).WithMessage("Device must not exceed 100 characters");
    }
}