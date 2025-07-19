using FastEndpoints;
using FluentValidation;
using Zapper.Client.Devices;

namespace Zapper.API.Validators.Devices;

public class DeleteDeviceRequestValidator : Validator<DeleteDeviceRequest>
{
    public DeleteDeviceRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Device ID must be greater than 0");
    }
}