using FluentValidation.TestHelper;
using Zapper.Client.Devices;

namespace Zapper.API.Tests.Unit.Validators.Devices.Bluetooth;

public class BluetoothScanRequestValidatorTests
{
    private readonly BluetoothScanRequestValidator _validator = new();

    [Fact]
    public void Should_Have_No_Errors_When_Request_Is_Valid()
    {
        var request = new BluetoothScanRequest { DurationSeconds = 30 };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_No_Errors_When_Using_Default_Duration()
    {
        var request = new BluetoothScanRequest();
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Duration_Is_Too_Small()
    {
        var request = new BluetoothScanRequest { DurationSeconds = 0 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DurationSeconds)
            .WithErrorMessage("Duration must be between 1 and 60 seconds");
    }

    [Fact]
    public void Should_Have_Error_When_Duration_Is_Too_Large()
    {
        var request = new BluetoothScanRequest { DurationSeconds = 61 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DurationSeconds)
            .WithErrorMessage("Duration must be between 1 and 60 seconds");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(60)]
    public void Should_Have_No_Errors_For_Valid_Duration_Values(int duration)
    {
        var request = new BluetoothScanRequest { DurationSeconds = duration };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}