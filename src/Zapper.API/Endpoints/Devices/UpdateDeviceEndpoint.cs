using FastEndpoints;
using Zapper.API.Models.Requests;
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
                Device = new Zapper.Core.Models.Device
                {
                    Name = "Living Room TV Updated",
                    Brand = "Samsung",
                    Model = "UN55MU8000",
                    Type = Zapper.Core.Models.DeviceType.Television,
                    ConnectionType = Zapper.Core.Models.ConnectionType.NetworkTcp,
                    IpAddress = "192.168.1.101",
                    Port = 8001
                }
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
        var updatedDevice = await deviceService.UpdateDevice(req.Id, req.Device);
        if (updatedDevice == null)
        {
            await SendNotFoundAsync(ct);
            return;
        }

        await SendNoContentAsync(ct);
    }
}