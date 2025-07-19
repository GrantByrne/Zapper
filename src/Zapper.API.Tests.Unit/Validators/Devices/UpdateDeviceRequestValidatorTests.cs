using FluentValidation.TestHelper;
using Zapper.Client.Devices;
using Zapper.Core.Models;

namespace Zapper.API.Tests.Unit.Validators.Devices;

public class UpdateDeviceRequestValidatorTests
{
    private readonly UpdateDeviceRequestValidator _validator = new();

    [Fact]
    public void Should_Have_No_Errors_When_Request_Is_Valid()
    {
        var request = new UpdateDeviceRequest
        {
            Id = 1,
            Name = "Living Room TV",
            Brand = "Samsung",
            Model = "QN90A",
            Type = DeviceType.Television,
            ConnectionType = ConnectionType.Network,
            IpAddress = "192.168.1.100",
            MacAddress = "00:11:22:33:44:55",
            Port = 8080
        };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_Id_Is_Zero()
    {
        var request = new UpdateDeviceRequest { Id = 0, Name = "Test" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Device ID must be greater than 0");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Is_Empty()
    {
        var request = new UpdateDeviceRequest { Id = 1, Name = "" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Device name is required");
    }

    [Fact]
    public void Should_Have_Error_When_Name_Exceeds_Max_Length()
    {
        var request = new UpdateDeviceRequest { Id = 1, Name = new string('A', 101) };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Device name must not exceed 100 characters");
    }

    [Fact]
    public void Should_Have_Error_When_Brand_Exceeds_Max_Length()
    {
        var request = new UpdateDeviceRequest { Id = 1, Name = "Test", Brand = new string('A', 51) };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Brand)
            .WithErrorMessage("Brand must not exceed 50 characters");
    }

    [Fact]
    public void Should_Have_Error_When_IpAddress_Is_Invalid()
    {
        var request = new UpdateDeviceRequest { Id = 1, Name = "Test", IpAddress = "invalid.ip" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.IpAddress)
            .WithErrorMessage("Invalid IP address format");
    }

    [Fact]
    public void Should_Have_Error_When_MacAddress_Is_Invalid()
    {
        var request = new UpdateDeviceRequest { Id = 1, Name = "Test", MacAddress = "invalid:mac" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.MacAddress)
            .WithErrorMessage("Invalid MAC address format");
    }

    [Fact]
    public void Should_Have_Error_When_Port_Is_Out_Of_Range()
    {
        var request = new UpdateDeviceRequest { Id = 1, Name = "Test", Port = 70000 };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Port)
            .WithErrorMessage("Port must be between 1 and 65535");
    }

    [Fact]
    public void Should_Have_Multiple_Errors_When_Multiple_Properties_Are_Invalid()
    {
        var request = new UpdateDeviceRequest
        {
            Id = 0,
            Name = "",
            IpAddress = "invalid",
            Port = 0
        };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Id);
        result.ShouldHaveValidationErrorFor(x => x.Name);
        result.ShouldHaveValidationErrorFor(x => x.IpAddress);
        result.ShouldHaveValidationErrorFor(x => x.Port);
    }
}