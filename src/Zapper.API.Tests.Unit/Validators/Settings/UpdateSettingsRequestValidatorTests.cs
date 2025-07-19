using FluentValidation.TestHelper;
using Zapper.Client.Settings;
using Zapper.Core.Models;

namespace Zapper.API.Tests.Unit.Validators.Settings;

public class UpdateSettingsRequestValidatorTests
{
    private readonly UpdateSettingsRequestValidator _validator = new();

    [Fact]
    public void Should_Have_No_Errors_When_Request_Is_Valid()
    {
        var request = new UpdateSettingsRequest
        {
            Settings = new ZapperSettings
            {
                Hardware = new HardwareSettings
                {
                    Infrared = new IrHardwareSettings
                    {
                        TransmitterGpioPin = 18,
                        ReceiverGpioPin = 19,
                        CarrierFrequency = 38000,
                        DutyCycle = 0.33
                    }
                }
            }
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Settings_Is_Null()
    {
        var request = new UpdateSettingsRequest
        {
            Settings = null!
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Settings)
            .WithErrorMessage("Settings object is required");
    }

    [Fact]
    public void Should_Have_Error_When_TransmitterPin_Is_Too_Small()
    {
        var request = new UpdateSettingsRequest
        {
            Settings = new ZapperSettings
            {
                Hardware = new HardwareSettings
                {
                    Infrared = new IrHardwareSettings
                    {
                        TransmitterGpioPin = 0,
                        ReceiverGpioPin = 19
                    }
                }
            }
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Settings.Hardware.Infrared.TransmitterGpioPin)
            .WithErrorMessage("IR transmitter pin must be between 1 and 40");
    }

    [Fact]
    public void Should_Have_Error_When_ReceiverPin_Is_Too_Large()
    {
        var request = new UpdateSettingsRequest
        {
            Settings = new ZapperSettings
            {
                Hardware = new HardwareSettings
                {
                    Infrared = new IrHardwareSettings
                    {
                        TransmitterGpioPin = 18,
                        ReceiverGpioPin = 41
                    }
                }
            }
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Settings.Hardware.Infrared.ReceiverGpioPin)
            .WithErrorMessage("IR receiver pin must be between 1 and 40");
    }

    [Fact]
    public void Should_Have_Error_When_CarrierFrequency_Is_Too_Small()
    {
        var request = new UpdateSettingsRequest
        {
            Settings = new ZapperSettings
            {
                Hardware = new HardwareSettings
                {
                    Infrared = new IrHardwareSettings
                    {
                        TransmitterGpioPin = 18,
                        ReceiverGpioPin = 19,
                        CarrierFrequency = 19999
                    }
                }
            }
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Settings.Hardware.Infrared.CarrierFrequency)
            .WithErrorMessage("Carrier frequency must be between 20000 and 60000 Hz");
    }

    [Fact]
    public void Should_Have_Error_When_DutyCycle_Is_Too_Small()
    {
        var request = new UpdateSettingsRequest
        {
            Settings = new ZapperSettings
            {
                Hardware = new HardwareSettings
                {
                    Infrared = new IrHardwareSettings
                    {
                        TransmitterGpioPin = 18,
                        ReceiverGpioPin = 19,
                        DutyCycle = 0.05
                    }
                }
            }
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Settings.Hardware.Infrared.DutyCycle)
            .WithErrorMessage("Duty cycle must be between 0.1 and 0.9");
    }

    [Fact]
    public void Should_Have_Error_When_DutyCycle_Is_Too_Large()
    {
        var request = new UpdateSettingsRequest
        {
            Settings = new ZapperSettings
            {
                Hardware = new HardwareSettings
                {
                    Infrared = new IrHardwareSettings
                    {
                        TransmitterGpioPin = 18,
                        ReceiverGpioPin = 19,
                        DutyCycle = 0.95
                    }
                }
            }
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Settings.Hardware.Infrared.DutyCycle)
            .WithErrorMessage("Duty cycle must be between 0.1 and 0.9");
    }

    [Fact]
    public void Should_Have_No_Errors_When_Hardware_Is_Null()
    {
        var request = new UpdateSettingsRequest
        {
            Settings = new ZapperSettings
            {
                Hardware = null!
            }
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_No_Errors_When_Infrared_Is_Null()
    {
        var request = new UpdateSettingsRequest
        {
            Settings = new ZapperSettings
            {
                Hardware = new HardwareSettings
                {
                    Infrared = null!
                }
            }
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}