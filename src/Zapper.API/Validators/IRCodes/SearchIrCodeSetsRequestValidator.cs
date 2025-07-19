using FastEndpoints;
using FluentValidation;
using Zapper.Client.IRCodes;

namespace Zapper.API.Validators.IRCodes;

public class SearchIrCodeSetsRequestValidator : Validator<SearchIrCodeSetsRequest>
{
    public SearchIrCodeSetsRequestValidator()
    {
        RuleFor(x => x.Brand)
            .MaximumLength(100).WithMessage("Brand must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Brand));

        RuleFor(x => x.Model)
            .MaximumLength(100).WithMessage("Model must not exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Model));

        RuleFor(x => x.DeviceType)
            .IsInEnum().WithMessage("Invalid device type")
            .When(x => x.DeviceType.HasValue);

        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.Brand) || !string.IsNullOrEmpty(x.Model) || x.DeviceType.HasValue)
            .WithMessage("At least one search criteria (Brand, Model, or DeviceType) must be provided");
    }
}