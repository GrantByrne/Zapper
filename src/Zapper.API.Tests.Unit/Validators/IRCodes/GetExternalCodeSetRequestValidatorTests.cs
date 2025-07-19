using FluentValidation.TestHelper;
using Zapper.Client.IRCodes;

namespace Zapper.API.Tests.Unit.Validators.IRCodes;

public class GetExternalCodeSetRequestValidatorTests
{
    private readonly GetExternalCodeSetRequestValidator _validator = new();

    [Fact]
    public void Should_Have_No_Errors_When_Request_Is_Valid()
    {
        var request = new GetExternalCodeSetRequest
        {
            Manufacturer = "Samsung",
            Device = "TV",
            DeviceType = "Television",
            Subdevice = "Main"
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Manufacturer_Is_Empty()
    {
        var request = new GetExternalCodeSetRequest
        {
            Manufacturer = "",
            Device = "TV",
            DeviceType = "Television",
            Subdevice = "Main"
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Manufacturer)
            .WithErrorMessage("Manufacturer is required");
    }

    [Fact]
    public void Should_Have_Error_When_Device_Is_Empty()
    {
        var request = new GetExternalCodeSetRequest
        {
            Manufacturer = "Samsung",
            Device = "",
            DeviceType = "Television",
            Subdevice = "Main"
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Device)
            .WithErrorMessage("Device is required");
    }

    [Fact]
    public void Should_Have_Error_When_Manufacturer_Exceeds_Max_Length()
    {
        var request = new GetExternalCodeSetRequest
        {
            Manufacturer = new string('A', 101),
            Device = "TV",
            DeviceType = "Television",
            Subdevice = "Main"
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Manufacturer)
            .WithErrorMessage("Manufacturer must not exceed 100 characters");
    }

    [Fact]
    public void Should_Have_Error_When_Device_Exceeds_Max_Length()
    {
        var request = new GetExternalCodeSetRequest
        {
            Manufacturer = "Samsung",
            Device = new string('A', 101),
            DeviceType = "Television",
            Subdevice = "Main"
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Device)
            .WithErrorMessage("Device must not exceed 100 characters");
    }
}