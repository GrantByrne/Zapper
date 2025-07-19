using FluentValidation.TestHelper;
using Zapper.Client.Devices;

namespace Zapper.API.Tests.Unit.Validators.Devices;

public class DiscoverPlayStationDevicesRequestValidatorTests
{
    private readonly DiscoverPlayStationDevicesRequestValidator _validator = new();

    [Fact]
    public void Should_Have_No_Errors_When_Request_Is_Valid()
    {
        var request = new DiscoverPlayStationDevicesRequest { TimeoutSeconds = 30 };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_No_Errors_When_Using_Default_Timeout()
    {
        var request = new DiscoverPlayStationDevicesRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Timeout_Is_Too_Small()
    {
        var request = new DiscoverPlayStationDevicesRequest { TimeoutSeconds = 0 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.TimeoutSeconds)
            .WithErrorMessage("Timeout must be between 1 and 60 seconds");
    }

    [Fact]
    public void Should_Have_Error_When_Timeout_Is_Too_Large()
    {
        var request = new DiscoverPlayStationDevicesRequest { TimeoutSeconds = 61 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.TimeoutSeconds)
            .WithErrorMessage("Timeout must be between 1 and 60 seconds");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(60)]
    public void Should_Have_No_Errors_For_Valid_Timeout_Values(int timeout)
    {
        var request = new DiscoverPlayStationDevicesRequest { TimeoutSeconds = timeout };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}