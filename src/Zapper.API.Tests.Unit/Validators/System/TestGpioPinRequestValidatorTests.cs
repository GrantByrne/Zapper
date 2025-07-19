using FluentValidation.TestHelper;
using Zapper.Client.System;

namespace Zapper.API.Tests.Unit.Validators.System;

public class TestGpioPinRequestValidatorTests
{
    private readonly TestGpioPinRequestValidator _validator = new();

    [Fact]
    public void Should_Have_No_Errors_When_Request_Is_Valid()
    {
        var request = new TestGpioPinRequest
        {
            Pin = 20,
            IsOutput = true
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Pin_Is_Too_Small()
    {
        var request = new TestGpioPinRequest
        {
            Pin = 0,
            IsOutput = true
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Pin)
            .WithErrorMessage("Pin number must be between 1 and 40");
    }

    [Fact]
    public void Should_Have_Error_When_Pin_Is_Too_Large()
    {
        var request = new TestGpioPinRequest
        {
            Pin = 41,
            IsOutput = true
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Pin)
            .WithErrorMessage("Pin number must be between 1 and 40");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(20)]
    [InlineData(40)]
    public void Should_Have_No_Errors_For_Valid_Pin_Values(int pin)
    {
        var request = new TestGpioPinRequest
        {
            Pin = pin,
            IsOutput = true
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Should_Have_No_Errors_For_Any_IsOutput_Value(bool isOutput)
    {
        var request = new TestGpioPinRequest
        {
            Pin = 20,
            IsOutput = isOutput
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}