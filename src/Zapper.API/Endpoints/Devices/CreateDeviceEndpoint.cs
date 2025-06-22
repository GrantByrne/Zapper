using FastEndpoints;
using Zapper.API.Models.Requests;
using Zapper.API.Models.Responses;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.Endpoints.Devices;

public class CreateDeviceEndpoint(IDeviceService deviceService) : Endpoint<CreateDeviceRequest, CreateDeviceResponse>
{
    private readonly IDeviceService _deviceService = deviceService;

    public override void Configure()
    {
        Post("/api/devices");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new device";
            s.Description = "Create a new device configuration";
        });
    }

    public override async Task HandleAsync(CreateDeviceRequest req, CancellationToken ct)
    {
        var device = new Device
        {
            Name = req.Name,
            Brand = req.Brand ?? string.Empty,
            Model = req.Model ?? string.Empty,
            Type = req.Type,
            ConnectionType = req.ConnectionType,
            IpAddress = req.IpAddress,
            Port = req.Port,
            MacAddress = req.MacAddress,
            AuthenticationToken = req.AuthenticationToken
        };

        var createdDevice = await _deviceService.CreateDeviceAsync(device);
        
        var response = new CreateDeviceResponse
        {
            Id = createdDevice.Id,
            Name = createdDevice.Name,
            Brand = createdDevice.Brand,
            Model = createdDevice.Model,
            Type = createdDevice.Type,
            ConnectionType = createdDevice.ConnectionType,
            IpAddress = createdDevice.IpAddress,
            Port = createdDevice.Port,
            MacAddress = createdDevice.MacAddress,
            AuthenticationToken = createdDevice.AuthenticationToken,
            CreatedAt = createdDevice.CreatedAt,
            LastSeen = createdDevice.LastSeen
        };

        await SendOkAsync(response, ct);
    }
}