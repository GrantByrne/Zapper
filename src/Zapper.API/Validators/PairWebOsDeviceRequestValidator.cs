using FastEndpoints;
using FluentValidation;
using Zapper.Client.Devices;

namespace Zapper.API.Validators;

public class PairWebOsDeviceRequestValidator : Validator<PairWebOsDeviceRequest>
{
    public PairWebOsDeviceRequestValidator()
    {
        RuleFor(x => x.DeviceId)
            .GreaterThan(0).WithMessage("Device ID must be greater than 0");
    }
}