using FluentValidation.TestHelper;
using Zapper.Client.Devices;

namespace Zapper.API.Tests.Unit.Validators.Devices;

public class DiscoverXboxDevicesRequestValidatorTests
{
    private readonly DiscoverXboxDevicesRequestValidator _validator = new();

    [Fact]
    public void Should_Have_No_Errors_When_Request_Is_Valid()
    {
        var request = new DiscoverXboxDevicesRequest { DurationSeconds = 30 };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_No_Errors_When_Using_Default_Duration()
    {
        var request = new DiscoverXboxDevicesRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Duration_Is_Too_Small()
    {
        var request = new DiscoverXboxDevicesRequest { DurationSeconds = 0 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DurationSeconds)
            .WithErrorMessage("Duration must be between 1 and 60 seconds");
    }

    [Fact]
    public void Should_Have_Error_When_Duration_Is_Too_Large()
    {
        var request = new DiscoverXboxDevicesRequest { DurationSeconds = 61 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DurationSeconds)
            .WithErrorMessage("Duration must be between 1 and 60 seconds");
    }
}