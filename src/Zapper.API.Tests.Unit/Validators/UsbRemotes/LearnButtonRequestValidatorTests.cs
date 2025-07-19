using FluentValidation.TestHelper;
using Zapper.Client.UsbRemotes;

namespace Zapper.API.Tests.Unit.Validators.UsbRemotes;

public class LearnButtonRequestValidatorTests
{
    private readonly LearnButtonRequestValidator _validator = new();

    [Fact]
    public void Should_Have_No_Errors_When_Request_Is_Valid()
    {
        var request = new LearnButtonRequest
        {
            RemoteId = 1,
            TimeoutSeconds = 15
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_No_Errors_When_Using_Default_Timeout()
    {
        var request = new LearnButtonRequest
        {
            RemoteId = 1
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_RemoteId_Is_Zero()
    {
        var request = new LearnButtonRequest
        {
            RemoteId = 0
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.RemoteId)
            .WithErrorMessage("Remote ID must be greater than 0");
    }

    [Fact]
    public void Should_Have_Error_When_Timeout_Is_Too_Small()
    {
        var request = new LearnButtonRequest
        {
            RemoteId = 1,
            TimeoutSeconds = 0
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.TimeoutSeconds)
            .WithErrorMessage("Timeout must be between 1 and 30 seconds");
    }

    [Fact]
    public void Should_Have_Error_When_Timeout_Is_Too_Large()
    {
        var request = new LearnButtonRequest
        {
            RemoteId = 1,
            TimeoutSeconds = 31
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.TimeoutSeconds)
            .WithErrorMessage("Timeout must be between 1 and 30 seconds");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(15)]
    [InlineData(30)]
    public void Should_Have_No_Errors_For_Valid_Timeout_Values(int timeout)
    {
        var request = new LearnButtonRequest
        {
            RemoteId = 1,
            TimeoutSeconds = timeout
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}