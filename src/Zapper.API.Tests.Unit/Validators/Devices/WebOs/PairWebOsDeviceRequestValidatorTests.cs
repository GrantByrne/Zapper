namespace Zapper.API.Tests.Unit.Validators;

public class PairWebOsDeviceRequestValidatorTests
{
    private readonly PairWebOsDeviceRequestValidator _validator = new();

    [Fact]
    public void Should_Pass_When_Request_Is_Valid()
    {
        var request = new PairWebOsDeviceRequest
        {
            DeviceId = 1
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_When_DeviceId_Is_Large()
    {
        var request = new PairWebOsDeviceRequest
        {
            DeviceId = int.MaxValue
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_DeviceId_Is_Zero()
    {
        var request = new PairWebOsDeviceRequest
        {
            DeviceId = 0
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DeviceId)
            .WithErrorMessage("Device ID must be greater than 0");
    }

    [Fact]
    public void Should_Fail_When_DeviceId_Is_Negative()
    {
        var request = new PairWebOsDeviceRequest
        {
            DeviceId = -1
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DeviceId)
            .WithErrorMessage("Device ID must be greater than 0");
    }

    [Theory]
    [InlineData(-100)]
    [InlineData(-1)]
    [InlineData(0)]
    public void Should_Fail_When_DeviceId_Is_Not_Positive(int deviceId)
    {
        var request = new PairWebOsDeviceRequest
        {
            DeviceId = deviceId
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DeviceId)
            .WithErrorMessage("Device ID must be greater than 0");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(1000)]
    public void Should_Pass_When_DeviceId_Is_Positive(int deviceId)
    {
        var request = new PairWebOsDeviceRequest
        {
            DeviceId = deviceId
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.DeviceId);
    }
}