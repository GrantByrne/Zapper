using FluentValidation.TestHelper;
using Zapper.Client.Devices;

namespace Zapper.API.Tests.Unit.Validators.Devices;

public class DiscoverSteamDeckDevicesRequestValidatorTests
{
    private readonly DiscoverSteamDeckDevicesRequestValidator _validator = new();

    [Fact]
    public void Should_Have_No_Errors_When_Request_Is_Valid()
    {
        // DiscoverSteamDeckDevicesRequest is an empty record
        var request = new DiscoverSteamDeckDevicesRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}