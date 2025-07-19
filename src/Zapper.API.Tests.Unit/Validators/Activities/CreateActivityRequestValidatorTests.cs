namespace Zapper.API.Tests.Unit.Validators;

public class CreateActivityRequestValidatorTests
{
    private readonly CreateActivityRequestValidator _validator = new();

    [Fact]
    public void Should_Pass_When_Request_Is_Valid()
    {
        var request = new CreateActivityRequest
        {
            Name = "Watch TV",
            Description = "Turn on TV and cable box",
            Type = "Composite",
            Steps = new List<CreateActivityStepRequest>
            {
                new() { DeviceId = 1, Command = "PowerOn", DelayMs = 1000, SortOrder = 1 },
                new() { DeviceId = 2, Command = "PowerOn", DelayMs = 500, SortOrder = 2 }
            }
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Fail_When_Name_Is_Empty()
    {
        var request = new CreateActivityRequest
        {
            Name = "",
            Type = "Composite",
            Steps = new List<CreateActivityStepRequest>()
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Activity name is required");
    }

    [Fact]
    public void Should_Fail_When_Name_Is_Null()
    {
        var request = new CreateActivityRequest
        {
            Name = null!,
            Type = "Composite",
            Steps = new List<CreateActivityStepRequest>()
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Activity name is required");
    }

    [Fact]
    public void Should_Fail_When_Name_Exceeds_100_Characters()
    {
        var request = new CreateActivityRequest
        {
            Name = new string('a', 101),
            Type = "Composite",
            Steps = new List<CreateActivityStepRequest>()
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Activity name must not exceed 100 characters");
    }

    [Fact]
    public void Should_Pass_When_Description_Is_Null()
    {
        var request = new CreateActivityRequest
        {
            Name = "Watch TV",
            Description = null,
            Type = "Composite",
            Steps = new List<CreateActivityStepRequest>()
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Fail_When_Description_Exceeds_500_Characters()
    {
        var request = new CreateActivityRequest
        {
            Name = "Watch TV",
            Description = new string('a', 501),
            Type = "Composite",
            Steps = new List<CreateActivityStepRequest>()
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description must not exceed 500 characters");
    }

    [Fact]
    public void Should_Fail_When_Type_Is_Empty()
    {
        var request = new CreateActivityRequest
        {
            Name = "Watch TV",
            Type = "",
            Steps = new List<CreateActivityStepRequest>()
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
        var request = new CreateActivityRequest
        {
            Name = "Watch TV",
            Type = type,
            Steps = new List<CreateActivityStepRequest>()
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
        var request = new CreateActivityRequest
        {
            Name = "Watch TV",
            Type = type,
            Steps = new List<CreateActivityStepRequest>()
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorMessage("Invalid activity type");
    }

    [Fact]
    public void Should_Fail_When_Steps_Is_Null()
    {
        var request = new CreateActivityRequest
        {
            Name = "Watch TV",
            Type = "Composite",
            Steps = null!
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Steps)
            .WithErrorMessage("Steps collection cannot be null");
    }

    [Fact]
    public void Should_Pass_When_Steps_Is_Empty()
    {
        var request = new CreateActivityRequest
        {
            Name = "Watch TV",
            Type = "Composite",
            Steps = new List<CreateActivityStepRequest>()
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Steps);
    }

    [Fact]
    public void Should_Fail_When_Step_DeviceId_Is_Zero()
    {
        var request = new CreateActivityRequest
        {
            Name = "Watch TV",
            Type = "Composite",
            Steps = new List<CreateActivityStepRequest>
            {
                new() { DeviceId = 0, Command = "PowerOn", DelayMs = 0, SortOrder = 0 }
            }
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("Steps[0].DeviceId")
            .WithErrorMessage("Device ID must be greater than 0");
    }

    [Fact]
    public void Should_Fail_When_Step_DeviceId_Is_Negative()
    {
        var request = new CreateActivityRequest
        {
            Name = "Watch TV",
            Type = "Composite",
            Steps = new List<CreateActivityStepRequest>
            {
                new() { DeviceId = -1, Command = "PowerOn", DelayMs = 0, SortOrder = 0 }
            }
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("Steps[0].DeviceId")
            .WithErrorMessage("Device ID must be greater than 0");
    }

    [Fact]
    public void Should_Fail_When_Step_Command_Is_Empty()
    {
        var request = new CreateActivityRequest
        {
            Name = "Watch TV",
            Type = "Composite",
            Steps = new List<CreateActivityStepRequest>
            {
                new() { DeviceId = 1, Command = "", DelayMs = 0, SortOrder = 0 }
            }
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("Steps[0].Command")
            .WithErrorMessage("Command is required");
    }

    [Fact]
    public void Should_Fail_When_Step_Command_Is_Null()
    {
        var request = new CreateActivityRequest
        {
            Name = "Watch TV",
            Type = "Composite",
            Steps = new List<CreateActivityStepRequest>
            {
                new() { DeviceId = 1, Command = null!, DelayMs = 0, SortOrder = 0 }
            }
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("Steps[0].Command")
            .WithErrorMessage("Command is required");
    }

    [Fact]
    public void Should_Fail_When_Step_DelayMs_Is_Negative()
    {
        var request = new CreateActivityRequest
        {
            Name = "Watch TV",
            Type = "Composite",
            Steps = new List<CreateActivityStepRequest>
            {
                new() { DeviceId = 1, Command = "PowerOn", DelayMs = -1, SortOrder = 0 }
            }
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("Steps[0].DelayMs")
            .WithErrorMessage("Delay must be non-negative");
    }

    [Fact]
    public void Should_Pass_When_Step_DelayMs_Is_Zero()
    {
        var request = new CreateActivityRequest
        {
            Name = "Watch TV",
            Type = "Composite",
            Steps = new List<CreateActivityStepRequest>
            {
                new() { DeviceId = 1, Command = "PowerOn", DelayMs = 0, SortOrder = 0 }
            }
        };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor("Steps[0].DelayMs");
    }

    [Fact]
    public void Should_Fail_When_Step_SortOrder_Is_Negative()
    {
        var request = new CreateActivityRequest
        {
            Name = "Watch TV",
            Type = "Composite",
            Steps = new List<CreateActivityStepRequest>
            {
                new() { DeviceId = 1, Command = "PowerOn", DelayMs = 0, SortOrder = -1 }
            }
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("Steps[0].SortOrder")
            .WithErrorMessage("Sort order must be non-negative");
    }

    [Fact]
    public void Should_Validate_Multiple_Steps_Independently()
    {
        var request = new CreateActivityRequest
        {
            Name = "Watch TV",
            Type = "Composite",
            Steps = new List<CreateActivityStepRequest>
            {
                new() { DeviceId = 0, Command = "PowerOn", DelayMs = 0, SortOrder = 0 },
                new() { DeviceId = 1, Command = "", DelayMs = -1, SortOrder = -1 }
            }
        };

        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor("Steps[0].DeviceId");
        result.ShouldHaveValidationErrorFor("Steps[1].Command");
        result.ShouldHaveValidationErrorFor("Steps[1].DelayMs");
        result.ShouldHaveValidationErrorFor("Steps[1].SortOrder");
    }
}