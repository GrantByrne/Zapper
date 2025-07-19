using FluentValidation.TestHelper;
using Zapper.Client.IRCodes;

namespace Zapper.API.Tests.Unit.Validators.IRCodes;

public class GetIrCodeRequestValidatorTests
{
    private readonly GetIrCodeRequestValidator _validator = new();

    [Fact]
    public void Should_Have_No_Errors_When_Request_Is_Valid()
    {
        var request = new GetIrCodeRequest
        {
            CodeSetId = 1,
            CommandName = "Power"
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_CodeSetId_Is_Zero()
    {
        var request = new GetIrCodeRequest
        {
            CodeSetId = 0,
            CommandName = "Power"
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CodeSetId)
            .WithErrorMessage("Code Set ID must be greater than 0");
    }

    [Fact]
    public void Should_Have_Error_When_CommandName_Is_Empty()
    {
        var request = new GetIrCodeRequest
        {
            CodeSetId = 1,
            CommandName = ""
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CommandName)
            .WithErrorMessage("Command name is required");
    }

    [Fact]
    public void Should_Have_Error_When_CommandName_Exceeds_Max_Length()
    {
        var request = new GetIrCodeRequest
        {
            CodeSetId = 1,
            CommandName = new string('A', 101)
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CommandName)
            .WithErrorMessage("Command name must not exceed 100 characters");
    }

    [Fact]
    public void Should_Have_Multiple_Errors_When_Multiple_Properties_Are_Invalid()
    {
        var request = new GetIrCodeRequest
        {
            CodeSetId = 0,
            CommandName = ""
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CodeSetId);
        result.ShouldHaveValidationErrorFor(x => x.CommandName);
    }
}