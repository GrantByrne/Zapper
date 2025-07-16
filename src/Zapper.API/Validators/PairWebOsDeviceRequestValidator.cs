using FastEndpoints;
using FluentValidation;
using Zapper.Contracts.Devices;
using Zapper.Contracts.IRCodes;
using Zapper.Contracts.Settings;
using Zapper.Contracts.System;
using Zapper.Contracts.UsbRemotes;

namespace Zapper.API.Validators;

public class PairWebOsDeviceRequestValidator : Validator<PairWebOsDeviceRequest>
{
    public PairWebOsDeviceRequestValidator()
    {
        RuleFor(x => x.DeviceId)
            .GreaterThan(0).WithMessage("Device ID must be greater than 0");
    }
}