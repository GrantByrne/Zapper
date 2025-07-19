using FastEndpoints;
using FluentValidation;
using Zapper.Client.Devices;

namespace Zapper.API.Validators.Devices;

public class GetDeviceRequestValidator : Validator<GetDeviceRequest>
{
    public GetDeviceRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Device ID must be greater than 0");
    }
}