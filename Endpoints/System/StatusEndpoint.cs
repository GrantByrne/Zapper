using FastEndpoints;

namespace ZapperHub.Endpoints.System;

public class StatusResponse
{
    public string Status { get; set; } = "OK";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Version { get; set; } = "1.0.0";
    public int ConnectedClients { get; set; }
    public SystemInfo System { get; set; } = new();
}

public class SystemInfo
{
    public string MachineName { get; set; } = Environment.MachineName;
    public string Platform { get; set; } = Environment.OSVersion.Platform.ToString();
    public string OSVersion { get; set; } = Environment.OSVersion.VersionString;
    public long WorkingSet { get; set; } = Environment.WorkingSet;
    public TimeSpan Uptime { get; set; } = TimeSpan.FromMilliseconds(Environment.TickCount64);
}

public class StatusEndpoint : EndpointWithoutRequest<StatusResponse>
{
    public override void Configure()
    {
        Get("/api/system/status");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get system status";
            s.Description = "Returns current system status, health information, and basic statistics";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var response = new StatusResponse
        {
            ConnectedClients = 0 // TODO: Get actual count from SignalR hub
        };

        await SendOkAsync(response, ct);
    }
}