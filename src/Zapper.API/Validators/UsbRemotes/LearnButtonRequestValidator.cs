using FastEndpoints;
using FluentValidation;
using Zapper.Client.UsbRemotes;

namespace Zapper.API.Validators.UsbRemotes;

public class LearnButtonRequestValidator : Validator<LearnButtonRequest>
{
    public LearnButtonRequestValidator()
    {
        RuleFor(x => x.RemoteId)
            .GreaterThan(0).WithMessage("Remote ID must be greater than 0");

        RuleFor(x => x.TimeoutSeconds)
            .InclusiveBetween(1, 30).WithMessage("Timeout must be between 1 and 30 seconds");
    }
}