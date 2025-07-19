using FluentValidation.TestHelper;
using Zapper.Client.IRCodes;
using Zapper.Core.Models;

namespace Zapper.API.Tests.Unit.Validators.IRCodes;

public class SearchIrCodeSetsRequestValidatorTests
{
    private readonly SearchIrCodeSetsRequestValidator _validator = new();

    [Fact]
    public void Should_Have_No_Errors_When_Brand_Is_Provided()
    {
        var request = new SearchIrCodeSetsRequest
        {
            Brand = "Samsung"
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_No_Errors_When_Model_Is_Provided()
    {
        var request = new SearchIrCodeSetsRequest
        {
            Model = "QN90A"
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_No_Errors_When_DeviceType_Is_Provided()
    {
        var request = new SearchIrCodeSetsRequest
        {
            DeviceType = DeviceType.Television
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_No_Errors_When_Multiple_Criteria_Are_Provided()
    {
        var request = new SearchIrCodeSetsRequest
        {
            Brand = "Samsung",
            Model = "QN90A",
            DeviceType = DeviceType.Television
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_No_Criteria_Are_Provided()
    {
        var request = new SearchIrCodeSetsRequest();
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("At least one search criteria (Brand, Model, or DeviceType) must be provided");
    }

    [Fact]
    public void Should_Have_Error_When_Brand_Exceeds_Max_Length()
    {
        var request = new SearchIrCodeSetsRequest
        {
            Brand = new string('A', 101)
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Brand)
            .WithErrorMessage("Brand must not exceed 100 characters");
    }

    [Fact]
    public void Should_Have_Error_When_Model_Exceeds_Max_Length()
    {
        var request = new SearchIrCodeSetsRequest
        {
            Model = new string('A', 101)
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Model)
            .WithErrorMessage("Model must not exceed 100 characters");
    }

    [Fact]
    public void Should_Not_Validate_Null_Properties()
    {
        var request = new SearchIrCodeSetsRequest
        {
            Brand = null,
            Model = null,
            DeviceType = DeviceType.Television
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Brand);
        result.ShouldNotHaveValidationErrorFor(x => x.Model);
    }
}