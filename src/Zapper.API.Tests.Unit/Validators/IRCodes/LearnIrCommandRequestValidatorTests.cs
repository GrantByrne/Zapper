using FluentValidation.TestHelper;
using Zapper.Client.IRCodes;

namespace Zapper.API.Tests.Unit.Validators.IRCodes;

public class LearnIrCommandRequestValidatorTests
{
    private readonly LearnIrCommandRequestValidator _validator = new();

    [Fact]
    public void Should_Have_No_Errors_When_Request_Is_Valid()
    {
        var request = new LearnIrCommandRequest
        {
            CommandName = "Power",
            TimeoutSeconds = 30
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_No_Errors_When_Using_Default_Timeout()
    {
        var request = new LearnIrCommandRequest
        {
            CommandName = "Power"
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_CommandName_Is_Empty()
    {
        var request = new LearnIrCommandRequest
        {
            CommandName = ""
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CommandName)
            .WithErrorMessage("Command name is required");
    }

    [Fact]
    public void Should_Have_Error_When_CommandName_Exceeds_Max_Length()
    {
        var request = new LearnIrCommandRequest
        {
            CommandName = new string('A', 101)
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CommandName)
            .WithErrorMessage("Command name must not exceed 100 characters");
    }

    [Fact]
    public void Should_Have_Error_When_Timeout_Is_Too_Small()
    {
        var request = new LearnIrCommandRequest
        {
            CommandName = "Power",
            TimeoutSeconds = 0
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.TimeoutSeconds)
            .WithErrorMessage("Timeout must be between 1 and 60 seconds");
    }

    [Fact]
    public void Should_Have_Error_When_Timeout_Is_Too_Large()
    {
        var request = new LearnIrCommandRequest
        {
            CommandName = "Power",
            TimeoutSeconds = 61
        };
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
        var request = new LearnIrCommandRequest
        {
            CommandName = "Power",
            TimeoutSeconds = timeout
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}