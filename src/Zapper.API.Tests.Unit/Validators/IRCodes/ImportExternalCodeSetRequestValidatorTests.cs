using FluentValidation.TestHelper;
using Zapper.Client.IRCodes;

namespace Zapper.API.Tests.Unit.Validators.IRCodes;

public class ImportExternalCodeSetRequestValidatorTests
{
    private readonly ImportExternalCodeSetRequestValidator _validator = new();

    [Fact]
    public void Should_Have_No_Errors_When_Request_Is_Valid()
    {
        var request = new ImportExternalCodeSetRequest
        {
            Manufacturer = "Samsung",
            DeviceType = "TV",
            Device = "QN90A",
            Subdevice = "Main"
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Manufacturer_Is_Empty()
    {
        var request = new ImportExternalCodeSetRequest
        {
            Manufacturer = "",
            DeviceType = "TV",
            Device = "QN90A",
            Subdevice = "Main"
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Manufacturer)
            .WithErrorMessage("Manufacturer is required");
    }

    [Fact]
    public void Should_Have_Error_When_DeviceType_Is_Empty()
    {
        var request = new ImportExternalCodeSetRequest
        {
            Manufacturer = "Samsung",
            DeviceType = "",
            Device = "QN90A",
            Subdevice = "Main"
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.DeviceType)
            .WithErrorMessage("Device type is required");
    }

    [Fact]
    public void Should_Have_Error_When_Device_Is_Empty()
    {
        var request = new ImportExternalCodeSetRequest
        {
            Manufacturer = "Samsung",
            DeviceType = "TV",
            Device = "",
            Subdevice = "Main"
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Device)
            .WithErrorMessage("Device is required");
    }

    [Fact]
    public void Should_Have_Error_When_Subdevice_Is_Empty()
    {
        var request = new ImportExternalCodeSetRequest
        {
            Manufacturer = "Samsung",
            DeviceType = "TV",
            Device = "QN90A",
            Subdevice = ""
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Subdevice)
            .WithErrorMessage("Subdevice is required");
    }

    [Fact]
    public void Should_Have_Error_When_Properties_Exceed_Max_Length()
    {
        var request = new ImportExternalCodeSetRequest
        {
            Manufacturer = new string('A', 101),
            DeviceType = new string('B', 51),
            Device = new string('C', 101),
            Subdevice = new string('D', 101)
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Manufacturer)
            .WithErrorMessage("Manufacturer must not exceed 100 characters");
        result.ShouldHaveValidationErrorFor(x => x.DeviceType)
            .WithErrorMessage("Device type must not exceed 50 characters");
        result.ShouldHaveValidationErrorFor(x => x.Device)
            .WithErrorMessage("Device must not exceed 100 characters");
        result.ShouldHaveValidationErrorFor(x => x.Subdevice)
            .WithErrorMessage("Subdevice must not exceed 100 characters");
    }
}