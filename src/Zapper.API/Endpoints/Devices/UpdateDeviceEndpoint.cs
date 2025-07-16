using FastEndpoints;
using Zapper.Contracts.Devices;
using Zapper.Core.Models;
using Zapper.Services;

namespace Zapper.API.Endpoints.Devices;

public class UpdateDeviceEndpoint(IDeviceService deviceService) : Endpoint<UpdateDeviceRequest>
{

    public override void Configure()
    {
        Put("/api/devices/{id}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Update a device";
            s.Description = "Update an existing device configuration. All fields will be updated with the provided values.";
            s.ExampleRequest = new UpdateDeviceRequest
            {
                Id = 1,
                Name = "Living Room TV Updated",
                Brand = "Samsung",
                Model = "UN55MU8000",
                Type = DeviceType.Television,
                ConnectionType = ConnectionType.NetworkTcp,
                IpAddress = "192.168.1.101",
                Port = 8001
            };
            s.Responses[204] = "Device updated successfully";
            s.Responses[400] = "Invalid request - validation errors";
            s.Responses[404] = "Device not found";
            s.Responses[500] = "Internal server error";
        });
        Tags("Devices");
    }

    public override async Task HandleAsync(UpdateDeviceRequest req, CancellationToken ct)
    {
        var device = new Zapper.Core.Models.Device
        {
            Id = req.Id,
            Name = req.Name,
            Brand = req.Brand ?? "",
            Model = req.Model ?? "",
            Type = (Zapper.Core.Models.DeviceType)req.Type,
            ConnectionType = (Zapper.Core.Models.ConnectionType)req.ConnectionType,
            IpAddress = req.IpAddress,
            Port = req.Port,
            MacAddress = req.MacAddress,
            AuthenticationToken = req.AuthenticationToken
        };

        var updatedDevice = await deviceService.UpdateDevice(req.Id, device);
        if (updatedDevice == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendNoContentAsync(ct);
    }
}