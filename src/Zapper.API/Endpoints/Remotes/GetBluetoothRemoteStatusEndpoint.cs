using FastEndpoints;
using Zapper.Client.Remotes;
using Zapper.Services;

namespace Zapper.API.Endpoints.Remotes;

public class GetBluetoothRemoteStatusEndpoint(
    IBluetoothRemoteService bluetoothRemoteService) : EndpointWithoutRequest<BluetoothRemoteStatusResponse>
{
    public override void Configure()
    {
        Get("/api/remotes/bluetooth/status");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get current Bluetooth remote status";
            s.Description = "Returns the current state of the Bluetooth remote functionality";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var status = await bluetoothRemoteService.GetStatusAsync(ct);

            await SendOkAsync(new BluetoothRemoteStatusResponse
            {
                IsAdvertising = status.IsAdvertising,
                RemoteName = status.RemoteName,
                DeviceType = status.DeviceType.ToString(),
                ConnectedHostAddress = status.ConnectedHostAddress,
                ConnectedHostName = status.ConnectedHostName,
                ConnectedAt = status.ConnectedAt
            }, ct);
        }
        catch (Exception)
        {
            await SendErrorsAsync(500, ct);
        }
    }
}