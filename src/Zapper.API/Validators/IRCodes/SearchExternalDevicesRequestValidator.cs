using FastEndpoints;
using FluentValidation;
using Zapper.Client.IRCodes;

namespace Zapper.API.Validators.IRCodes;

public class SearchExternalDevicesRequestValidator : Validator<SearchExternalDevicesRequest>
{
    public SearchExternalDevicesRequestValidator()
    {
        RuleFor(x => x.Manufacturer)
            .NotEmpty().WithMessage("Manufacturer is required")
            .MaximumLength(100).WithMessage("Manufacturer must not exceed 100 characters");
    }
}