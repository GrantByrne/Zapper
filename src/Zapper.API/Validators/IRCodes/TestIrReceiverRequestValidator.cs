using FastEndpoints;
using FluentValidation;
using Zapper.Client.IRCodes;

namespace Zapper.API.Validators.IRCodes;

public class TestIrReceiverRequestValidator : Validator<TestIrReceiverRequest>
{
    public TestIrReceiverRequestValidator()
    {
        RuleFor(x => x.TimeoutSeconds)
            .InclusiveBetween(1, 60).WithMessage("Timeout must be between 1 and 60 seconds");
    }
}