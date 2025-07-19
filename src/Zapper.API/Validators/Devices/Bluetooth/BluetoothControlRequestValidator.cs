using FastEndpoints;
using FluentValidation;
using Zapper.Client.Devices;

namespace Zapper.API.Validators.Devices.Bluetooth;

public class BluetoothControlRequestValidator : Validator<BluetoothControlRequest>
{
    public BluetoothControlRequestValidator()
    {
        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("Action is required")
            .MaximumLength(50).WithMessage("Action must not exceed 50 characters");

        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("Device ID is required when performing device actions")
            .When(x => x.Action != "");

        RuleFor(x => x.Text)
            .MaximumLength(1000).WithMessage("Text must not exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Text));
    }
}