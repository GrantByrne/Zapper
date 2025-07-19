namespace Zapper.API.Tests.Unit.Validators;

public class UpdateActivityRequestValidatorTests
{
    private readonly UpdateActivityRequestValidator _validator = new();

    [Fact]
    public void Should_Pass_When_Request_Is_Valid()
    {
        var request = new UpdateActivityRequest
        {
            Id = 1,
            Name = "Watch TV",
            Description = "Turn on TV and cable box",
            Type = "Composite"
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_Id_Is_Zero()
    {
        var request = new UpdateActivityRequest
        {
            Id = 0,
            Name = "Watch TV",
            Type = "Composite"
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Activity ID must be greater than 0");
    }

    [Fact]
    public void Should_Fail_When_Id_Is_Negative()
    {
        var request = new UpdateActivityRequest
        {
            Id = -1,
            Name = "Watch TV",
            Type = "Composite"
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Activity ID must be greater than 0");
    }

    [Fact]
    public void Should_Fail_When_Name_Is_Empty()
    {
        var request = new UpdateActivityRequest
        {
            Id = 1,
            Name = "",
            Type = "Composite"
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Activity name is required");
    }

    [Fact]
    public void Should_Fail_When_Name_Is_Null()
    {
        var request = new UpdateActivityRequest
        {
            Id = 1,
            Name = null!,
            Type = "Composite"
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Activity name is required");
    }

    [Fact]
    public void Should_Fail_When_Name_Exceeds_100_Characters()
    {
        var request = new UpdateActivityRequest
        {
            Id = 1,
            Name = new string('a', 101),
            Type = "Composite"
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Activity name must not exceed 100 characters");
    }

    [Fact]
    public void Should_Pass_When_Description_Is_Null()
    {
        var request = new UpdateActivityRequest
        {
            Id = 1,
            Name = "Watch TV",
            Description = null,
            Type = "Composite"
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Fail_When_Description_Exceeds_500_Characters()
    {
        var request = new UpdateActivityRequest
        {
            Id = 1,
            Name = "Watch TV",
            Description = new string('a', 501),
            Type = "Composite"
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description must not exceed 500 characters");
    }

    [Fact]
    public void Should_Fail_When_Type_Is_Empty()
    {
        var request = new UpdateActivityRequest
        {
            Id = 1,
            Name = "Watch TV",
            Type = ""
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorMessage("Activity type is required");
    }

    [Theory]
    [InlineData("Composite")]
    [InlineData("Macro")]
    [InlineData("Scene")]
    public void Should_Pass_When_Type_Is_Valid(string type)
    {
        var request = new UpdateActivityRequest
        {
            Id = 1,
            Name = "Watch TV",
            Type = type
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Type);
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("Unknown")]
    [InlineData("composite")]
    public void Should_Fail_When_Type_Is_Invalid(string type)
    {
        var request = new UpdateActivityRequest
        {
            Id = 1,
            Name = "Watch TV",
            Type = type
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorMessage("Invalid activity type");
    }

    [Fact]
    public void Should_Have_Multiple_Errors_For_Invalid_Request()
    {
        var request = new UpdateActivityRequest
        {
            Id = 0,
            Name = "",
            Description = new string('a', 501),
            Type = "Invalid"
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.Description);
        result.ShouldHaveValidationErrorFor(x => x.Type);
    }
}