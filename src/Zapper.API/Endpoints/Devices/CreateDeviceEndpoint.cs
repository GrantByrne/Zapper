using FastEndpoints;
using Zapper.API.Models.Requests;
using Zapper.API.Models.Responses;
using Zapper.Services;

namespace Zapper.API.Endpoints.Devices;

public class CreateDeviceEndpoint(IDeviceService deviceService) : Endpoint<CreateDeviceRequest, CreateDeviceResponse>
{

    public override void Configure()
    {
        Post("/api/devices");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Create a new device";
            s.Description = "Create a new device configuration in the system. Devices can be controlled via IR, network, or Bluetooth connections.";
            s.ExampleRequest = new CreateDeviceRequest
            {
                Name = "Living Room TV",
                Brand = "Samsung",
                Model = "UN55MU8000",
                Type = Zapper.Core.Models.DeviceType.Television,
                ConnectionType = Zapper.Core.Models.ConnectionType.InfraredIr,
                IpAddress = "192.168.1.100",
                Port = 8001,
                IrCodeSetId = 1
            };
            s.Responses[200] = "Device created successfully";
            s.Responses[400] = "Invalid request - validation errors";
            s.Responses[500] = "Internal server error";
        });
        Tags("Devices");
    }

    public override async Task HandleAsync(CreateDeviceRequest req, CancellationToken ct)
    {
        var device = new Zapper.Core.Models.Device
        {
            Name = req.Name,
            Brand = req.Brand ?? "",
            Model = req.Model ?? "",
            Type = req.Type,
            ConnectionType = req.ConnectionType,
            IpAddress = req.IpAddress,
            Port = req.Port,
            MacAddress = req.MacAddress,
            AuthenticationToken = req.AuthenticationToken,
            IrCodeSetId = req.IrCodeSetId
        };

        var createdDevice = await deviceService.CreateDevice(device);

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