using FluentValidation.TestHelper;
using Zapper.Client.IRCodes;

namespace Zapper.API.Tests.Unit.Validators.IRCodes;

public class GetIrCodesRequestValidatorTests
{
    private readonly GetIrCodesRequestValidator _validator = new();

    [Fact]
    public void Should_Have_No_Errors_When_Request_Is_Valid()
    {
        var request = new GetIrCodesRequest { CodeSetId = 1 };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_CodeSetId_Is_Zero()
    {
        var request = new GetIrCodesRequest { CodeSetId = 0 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CodeSetId)
            .WithErrorMessage("Code Set ID must be greater than 0");
    }

    [Fact]
    public void Should_Have_Error_When_CodeSetId_Is_Negative()
    {
        var request = new GetIrCodesRequest { CodeSetId = -1 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CodeSetId)
            .WithErrorMessage("Code Set ID must be greater than 0");
    }
}