using FastEndpoints;
using FluentValidation;
using Zapper.Client.Devices;
using Zapper.Client.IRCodes;
using Zapper.Client.Settings;
using Zapper.Client.System;
using Zapper.Client.UsbRemotes;

namespace Zapper.API.Validators;

public class PairWebOsDeviceRequestValidator : Validator<PairWebOsDeviceRequest>
{
    public PairWebOsDeviceRequestValidator()
    {
        RuleFor(x => x.DeviceId)
            .GreaterThan(0).WithMessage("Device ID must be greater than 0");
    }
}