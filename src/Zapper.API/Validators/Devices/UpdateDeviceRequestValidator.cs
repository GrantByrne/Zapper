using FastEndpoints;
using FluentValidation;
using Zapper.Client.Devices;
using Zapper.Core.Models;
using System.Net;
using System.Text.RegularExpressions;

namespace Zapper.API.Validators.Devices;

public class UpdateDeviceRequestValidator : Validator<UpdateDeviceRequest>
{
    public UpdateDeviceRequestValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("Device ID must be greater than 0");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Device name is required")
            .MaximumLength(100).WithMessage("Device name must not exceed 100 characters");

        RuleFor(x => x.Brand)
            .MaximumLength(50).WithMessage("Brand must not exceed 50 characters");

        RuleFor(x => x.Model)
            .MaximumLength(50).WithMessage("Model must not exceed 50 characters");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid device type");

        RuleFor(x => x.ConnectionType)
            .IsInEnum().WithMessage("Invalid connection type");

        RuleFor(x => x.IpAddress)
            .Must(BeAValidIpAddress).WithMessage("Invalid IP address format")
            .When(x => !string.IsNullOrEmpty(x.IpAddress));

        RuleFor(x => x.MacAddress)
            .Must(BeAValidMacAddress).WithMessage("Invalid MAC address format")
            .When(x => !string.IsNullOrEmpty(x.MacAddress));

        RuleFor(x => x.Port)
            .InclusiveBetween(1, 65535).WithMessage("Port must be between 1 and 65535")
            .When(x => x.Port.HasValue);
    }

    private bool BeAValidIpAddress(string? ipAddress)
    {
        if (string.IsNullOrEmpty(ipAddress))
            return true;

        return IPAddress.TryParse(ipAddress, out _);
    }

    private bool BeAValidMacAddress(string? macAddress)
    {
        if (string.IsNullOrEmpty(macAddress))
            return true;

        var regex = new Regex(@"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$");
        return regex.IsMatch(macAddress);
    }
}