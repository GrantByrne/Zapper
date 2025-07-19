using FluentValidation.TestHelper;
using Zapper.Client.UsbRemotes;

namespace Zapper.API.Tests.Unit.Validators.UsbRemotes;

public class GetButtonMappingsRequestValidatorTests
{
    private readonly GetButtonMappingsRequestValidator _validator = new();

    [Fact]
    public void Should_Have_No_Errors_When_Request_Is_Valid()
    {
        var request = new GetButtonMappingsRequest { RemoteId = 1 };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_RemoteId_Is_Zero()
    {
        var request = new GetButtonMappingsRequest { RemoteId = 0 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.RemoteId)
            .WithErrorMessage("Remote ID must be greater than 0");
    }

    [Fact]
    public void Should_Have_Error_When_RemoteId_Is_Negative()
    {
        var request = new GetButtonMappingsRequest { RemoteId = -1 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.RemoteId)
            .WithErrorMessage("Remote ID must be greater than 0");
    }
}