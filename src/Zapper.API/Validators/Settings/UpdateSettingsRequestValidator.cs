using FastEndpoints;
using FluentValidation;
using Zapper.Client.Settings;

namespace Zapper.API.Validators.Settings;

public class UpdateSettingsRequestValidator : Validator<UpdateSettingsRequest>
{
    public UpdateSettingsRequestValidator()
    {
        RuleFor(x => x.Settings)
            .NotNull().WithMessage("Settings object is required");

        When(x => x.Settings?.Hardware?.Infrared != null, () =>
        {
            RuleFor(x => x.Settings.Hardware.Infrared.TransmitterGpioPin)
                .InclusiveBetween(1, 40).WithMessage("IR transmitter pin must be between 1 and 40");

            RuleFor(x => x.Settings.Hardware.Infrared.ReceiverGpioPin)
                .InclusiveBetween(1, 40).WithMessage("IR receiver pin must be between 1 and 40");

            RuleFor(x => x.Settings.Hardware.Infrared.CarrierFrequency)
                .InclusiveBetween(20000, 60000).WithMessage("Carrier frequency must be between 20000 and 60000 Hz");

            RuleFor(x => x.Settings.Hardware.Infrared.DutyCycle)
                .InclusiveBetween(0.1, 0.9).WithMessage("Duty cycle must be between 0.1 and 0.9");
        });
    }
}