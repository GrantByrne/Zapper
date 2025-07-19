using FastEndpoints;
using FluentValidation;
using Zapper.Client.Remotes;

namespace Zapper.API.Validators.Remotes;

public class StartBluetoothAdvertisingRequestValidator : Validator<StartBluetoothAdvertisingRequest>
{
    public StartBluetoothAdvertisingRequestValidator()
    {
        RuleFor(x => x.RemoteName)
            .NotEmpty().WithMessage("Remote name is required")
            .MaximumLength(50).WithMessage("Remote name must not exceed 50 characters");

        RuleFor(x => x.DeviceType)
            .NotEmpty().WithMessage("Device type is required")
            .Must(BeAValidDeviceType).WithMessage("Device type must be one of: TVRemote, GameController, MediaRemote");
    }

    private static bool BeAValidDeviceType(string deviceType)
    {
        var validTypes = new[] { "TVRemote", "GameController", "MediaRemote" };
        return validTypes.Contains(deviceType);
    }
}