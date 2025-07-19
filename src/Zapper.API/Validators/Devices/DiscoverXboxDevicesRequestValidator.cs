using FastEndpoints;
using FluentValidation;
using Zapper.Client.Devices;

namespace Zapper.API.Validators.Devices;

public class DiscoverXboxDevicesRequestValidator : Validator<DiscoverXboxDevicesRequest>
{
    public DiscoverXboxDevicesRequestValidator()
    {
        RuleFor(x => x.DurationSeconds)
            .InclusiveBetween(1, 60).WithMessage("Duration must be between 1 and 60 seconds");
    }
}