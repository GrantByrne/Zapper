using FastEndpoints;
using FluentValidation;
using Zapper.Client.Devices;

namespace Zapper.API.Validators.Devices;

public class WebOsScanRequestValidator : Validator<WebOsScanRequest>
{
    public WebOsScanRequestValidator()
    {
        RuleFor(x => x.DurationSeconds)
            .InclusiveBetween(1, 300).WithMessage("Duration must be between 1 and 300 seconds");
    }
}