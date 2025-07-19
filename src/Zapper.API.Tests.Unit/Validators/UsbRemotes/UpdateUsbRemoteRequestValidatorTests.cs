using FluentValidation.TestHelper;
using Zapper.Client.UsbRemotes;

namespace Zapper.API.Tests.Unit.Validators.UsbRemotes;

public class UpdateUsbRemoteRequestValidatorTests
{
    private readonly UpdateUsbRemoteRequestValidator _validator = new();

    [Fact]
    public void Should_Have_No_Errors_When_Request_Is_Valid()
    {
        var request = new UpdateUsbRemoteRequest
        {
            Id = 1,
            Name = "Living Room Remote",
            IsActive = true,
            InterceptSystemButtons = false,
            LongPressTimeoutMs = 1000
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Id_Is_Zero()
    {
        var request = new UpdateUsbRemoteRequest
        {
            Id = 0,
            Name = "Test Remote",
            LongPressTimeoutMs = 1000
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Remote ID must be greater than 0");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var request = new UpdateUsbRemoteRequest
        {
            Id = 1,
            Name = "",
            LongPressTimeoutMs = 1000
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Remote name is required");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_Max_Length()
    {
        var request = new UpdateUsbRemoteRequest
        {
            Id = 1,
            Name = new string('A', 101),
            LongPressTimeoutMs = 1000
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Remote name must not exceed 100 characters");
    }

    [Fact]
    public void Should_Have_Error_When_LongPressTimeout_Is_Too_Small()
    {
        var request = new UpdateUsbRemoteRequest
        {
            Id = 1,
            Name = "Test Remote",
            LongPressTimeoutMs = 99
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.LongPressTimeoutMs)
            .WithErrorMessage("Long press timeout must be between 100 and 5000 milliseconds");
    }

    [Fact]
    public void Should_Have_Error_When_LongPressTimeout_Is_Too_Large()
    {
        var request = new UpdateUsbRemoteRequest
        {
            Id = 1,
            Name = "Test Remote",
            LongPressTimeoutMs = 5001
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.LongPressTimeoutMs)
            .WithErrorMessage("Long press timeout must be between 100 and 5000 milliseconds");
    }

    [Theory]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(5000)]
    public void Should_Have_No_Errors_For_Valid_LongPressTimeout_Values(int timeout)
    {
        var request = new UpdateUsbRemoteRequest
        {
            Id = 1,
            Name = "Test Remote",
            LongPressTimeoutMs = timeout
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}