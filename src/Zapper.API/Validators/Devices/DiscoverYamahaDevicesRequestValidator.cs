using FastEndpoints;
using FluentValidation;
using Zapper.Client.Devices;

namespace Zapper.API.Validators.Devices;

public class DiscoverYamahaDevicesRequestValidator : Validator<DiscoverYamahaDevicesRequest>
{
    public DiscoverYamahaDevicesRequestValidator()
    {
        RuleFor(x => x.TimeoutSeconds)
            .InclusiveBetween(1, 60).WithMessage("Timeout must be between 1 and 60 seconds");
    }
}