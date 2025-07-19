namespace Zapper.API.Tests.Unit.Validators;

public class TestIrCodeRequestValidatorTests
{
    private readonly TestIrCodeRequestValidator _validator = new();

    [Fact]
    public void Should_Pass_When_Request_Is_Valid()
    {
        var request = new TestIrCodeRequest
        {
            CodeSetId = 1,
            CommandName = "PowerOn"
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_CodeSetId_Is_Zero()
    {
        var request = new TestIrCodeRequest
        {
            CodeSetId = 0,
            CommandName = "PowerOn"
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CodeSetId)
            .WithErrorMessage("Code set ID must be greater than 0");
    }

    [Fact]
    public void Should_Fail_When_CodeSetId_Is_Negative()
    {
        var request = new TestIrCodeRequest
        {
            CodeSetId = -1,
            CommandName = "PowerOn"
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CodeSetId)
            .WithErrorMessage("Code set ID must be greater than 0");
    }

    [Fact]
    public void Should_Fail_When_CommandName_Is_Empty()
    {
        var request = new TestIrCodeRequest
        {
            CodeSetId = 1,
            CommandName = ""
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CommandName)
            .WithErrorMessage("Command name is required");
    }

    [Fact]
    public void Should_Fail_When_CommandName_Is_Null()
    {
        var request = new TestIrCodeRequest
        {
            CodeSetId = 1,
            CommandName = null!
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CommandName)
            .WithErrorMessage("Command name is required");
    }

    [Fact]
    public void Should_Fail_When_CommandName_Exceeds_100_Characters()
    {
        var request = new TestIrCodeRequest
        {
            CodeSetId = 1,
            CommandName = new string('a', 101)
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CommandName)
            .WithErrorMessage("Command name must not exceed 100 characters");
    }

    [Fact]
    public void Should_Pass_When_CommandName_Is_Exactly_100_Characters()
    {
        var request = new TestIrCodeRequest
        {
            CodeSetId = 1,
            CommandName = new string('a', 100)
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.CommandName);
    }

    [Fact]
    public void Should_Have_Multiple_Errors_For_Invalid_Request()
    {
        var request = new TestIrCodeRequest
        {
            CodeSetId = 0,
            CommandName = ""
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CodeSetId);
        result.ShouldHaveValidationErrorFor(x => x.CommandName);
    }
}