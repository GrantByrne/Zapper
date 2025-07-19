namespace Zapper.API.Tests.Unit.Validators;

public class ExecuteActivityRequestValidatorTests
{
    private readonly ExecuteActivityRequestValidator _validator = new();

    [Fact]
    public void Should_Pass_When_Request_Is_Valid()
    {
        var request = new ExecuteActivityRequest
        {
            Id = 1
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Pass_When_Id_Is_Large()
    {
        var request = new ExecuteActivityRequest
        {
            Id = int.MaxValue
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_Id_Is_Zero()
    {
        var request = new ExecuteActivityRequest
        {
            Id = 0
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Activity ID must be greater than 0");
    }

    [Fact]
    public void Should_Fail_When_Id_Is_Negative()
    {
        var request = new ExecuteActivityRequest
        {
            Id = -1
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Activity ID must be greater than 0");
    }

    [Theory]
    [InlineData(-100)]
    [InlineData(-1)]
    [InlineData(0)]
    public void Should_Fail_When_Id_Is_Not_Positive(int id)
    {
        var request = new ExecuteActivityRequest
        {
            Id = id
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Activity ID must be greater than 0");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(1000)]
    public void Should_Pass_When_Id_Is_Positive(int id)
    {
        var request = new ExecuteActivityRequest
        {
            Id = id
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }
}