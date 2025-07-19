using FastEndpoints;
using FluentValidation;
using Zapper.Client.Devices;

namespace Zapper.API.Validators.Devices;

public class DiscoverSteamDeckDevicesRequestValidator : Validator<DiscoverSteamDeckDevicesRequest>
{
    public DiscoverSteamDeckDevicesRequestValidator()
    {
        // No properties to validate - DiscoverSteamDeckDevicesRequest is an empty record
    }
}