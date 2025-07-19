using FluentValidation.TestHelper;
using Zapper.Client.IRCodes;

namespace Zapper.API.Tests.Unit.Validators.IRCodes;

public class SearchExternalDevicesRequestValidatorTests
{
    private readonly SearchExternalDevicesRequestValidator _validator = new();

    [Fact]
    public void Should_Have_No_Errors_When_Request_Is_Valid()
    {
        var request = new SearchExternalDevicesRequest
        {
            Manufacturer = "Samsung"
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Manufacturer_Is_Empty()
    {
        var request = new SearchExternalDevicesRequest
        {
            Manufacturer = ""
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Manufacturer)
            .WithErrorMessage("Manufacturer is required");
    }

    [Fact]
    public void Should_Have_Error_When_Manufacturer_Exceeds_Max_Length()
    {
        var request = new SearchExternalDevicesRequest
        {
            Manufacturer = new string('A', 101)
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Manufacturer)
            .WithErrorMessage("Manufacturer must not exceed 100 characters");
    }
}