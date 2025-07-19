using FluentValidation.TestHelper;
using Zapper.Client.IRCodes;
using Zapper.Core.Models;

namespace Zapper.API.Tests.Unit.Validators.IRCodes;

public class AddIrCodeRequestValidatorTests
{
    private readonly AddIrCodeRequestValidator _validator = new();

    [Fact]
    public void Should_Have_No_Errors_When_Request_Is_Valid()
    {
        var request = new AddIrCodeRequest
        {
            CodeSetId = 1,
            Code = new IrCode
            {
                Brand = "Samsung",
                Model = "QN90A",
                DeviceType = DeviceType.Television,
                CommandName = "Power",
                Protocol = "NEC",
                HexCode = "0x20DF10EF",
                Frequency = 38000
            }
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_CodeSetId_Is_Zero()
    {
        var request = new AddIrCodeRequest { CodeSetId = 0 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CodeSetId)
            .WithErrorMessage("Code Set ID must be greater than 0");
    }

    [Fact]
    public void Should_Have_Error_When_Code_Is_Null()
    {
        var request = new AddIrCodeRequest { CodeSetId = 1, Code = null! };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Code)
            .WithErrorMessage("IR Code is required");
    }

    [Fact]
    public void Should_Have_Error_When_Brand_Is_Empty()
    {
        var request = new AddIrCodeRequest
        {
            CodeSetId = 1,
            Code = new IrCode()
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Code.Brand)
            .WithErrorMessage("Brand is required");
    }

    [Fact]
    public void Should_Have_Error_When_Model_Is_Empty()
    {
        var request = new AddIrCodeRequest
        {
            CodeSetId = 1,
            Code = new IrCode { Brand = "Samsung", Model = "" }
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Code.Model)
            .WithErrorMessage("Model is required");
    }

    [Fact]
    public void Should_Have_Error_When_CommandName_Is_Empty()
    {
        var request = new AddIrCodeRequest
        {
            CodeSetId = 1,
            Code = new IrCode { Brand = "Samsung", Model = "QN90A", CommandName = "" }
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Code.CommandName)
            .WithErrorMessage("Command name is required");
    }

    [Fact]
    public void Should_Have_Error_When_Protocol_Is_Empty()
    {
        var request = new AddIrCodeRequest
        {
            CodeSetId = 1,
            Code = new IrCode { Brand = "Samsung", Model = "QN90A", CommandName = "Power", Protocol = "" }
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Code.Protocol)
            .WithErrorMessage("Protocol is required");
    }

    [Fact]
    public void Should_Have_Error_When_HexCode_Is_Empty()
    {
        var request = new AddIrCodeRequest
        {
            CodeSetId = 1,
            Code = new IrCode
            {
                Brand = "Samsung",
                Model = "QN90A",
                CommandName = "Power",
                Protocol = "NEC",
                HexCode = ""
            }
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Code.HexCode)
            .WithErrorMessage("Hex code is required");
    }

    [Fact]
    public void Should_Have_Error_When_Frequency_Is_Zero()
    {
        var request = new AddIrCodeRequest
        {
            CodeSetId = 1,
            Code = new IrCode
            {
                Brand = "Samsung",
                Model = "QN90A",
                CommandName = "Power",
                Protocol = "NEC",
                HexCode = "0x20DF10EF",
                Frequency = 0
            }
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Code.Frequency)
            .WithErrorMessage("Frequency must be greater than 0");
    }
}