namespace Zapper.API.Tests.Unit.Validators;

public class SendCommandRequestValidatorTests
{
    private readonly SendCommandRequestValidator _validator = new();

    [Fact]
    public void Should_Pass_When_Request_Is_Valid()
    {
        var request = new SendCommandApiRequest
        {
            Id = 1,
            Command = "PowerOn"
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_Id_Is_Zero()
    {
        var request = new SendCommandApiRequest
        {
            Id = 0,
            Command = "PowerOn"
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Device ID must be greater than 0");
    }

    [Fact]
    public void Should_Fail_When_Id_Is_Negative()
    {
        var request = new SendCommandApiRequest
        {
            Id = -1,
            Command = "PowerOn"
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Device ID must be greater than 0");
    }

    [Fact]
    public void Should_Fail_When_Command_Is_Empty()
    {
        var request = new SendCommandApiRequest
        {
            Id = 1,
            Command = ""
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Command)
            .WithErrorMessage("Command is required");
    }

    [Fact]
    public void Should_Fail_When_Command_Is_Null()
    {
        var request = new SendCommandApiRequest
        {
            Id = 1,
            Command = null!
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Command)
            .WithErrorMessage("Command is required");
    }

    [Fact]
    public void Should_Fail_When_Command_Exceeds_100_Characters()
    {
        var request = new SendCommandApiRequest
        {
            Id = 1,
            Command = new string('a', 101)
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Command)
            .WithErrorMessage("Command must not exceed 100 characters");
    }

    [Fact]
    public void Should_Pass_When_Command_Is_Exactly_100_Characters()
    {
        var request = new SendCommandApiRequest
        {
            Id = 1,
            Command = new string('a', 100)
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Command);
    }

    [Fact]
    public void Should_Have_Multiple_Errors_For_Invalid_Request()
    {
        var request = new SendCommandApiRequest
        {
            Id = 0,
            Command = ""
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.ShouldHaveValidationErrorFor(x => x.Command);
    }
}