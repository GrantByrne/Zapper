using FluentValidation.TestHelper;
using Zapper.Client.Devices;

namespace Zapper.API.Tests.Unit.Validators.Devices.WebOs;

public class DiscoverWebOsDevicesRequestValidatorTests
{
    private readonly DiscoverWebOsDevicesRequestValidator _validator = new();

    [Fact]
    public void Should_Have_No_Errors_When_Request_Is_Valid()
    {
        var request = new DiscoverWebOsDevicesRequest { TimeoutSeconds = 30 };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Timeout_Is_Too_Small()
    {
        var request = new DiscoverWebOsDevicesRequest { TimeoutSeconds = 0 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.TimeoutSeconds)
            .WithErrorMessage("Timeout must be between 1 and 60 seconds");
    }

    [Fact]
    public void Should_Have_Error_When_Timeout_Is_Too_Large()
    {
        var request = new DiscoverWebOsDevicesRequest { TimeoutSeconds = 61 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.TimeoutSeconds)
            .WithErrorMessage("Timeout must be between 1 and 60 seconds");
    }
}