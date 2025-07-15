namespace Zapper.API.Endpoints.System;

public class SystemInfo
{
    public string MachineName { get; set; } = Environment.MachineName;
    public string Platform { get; set; } = Environment.OSVersion.Platform.ToString();
    public string OsVersion { get; set; } = Environment.OSVersion.VersionString;
    public long WorkingSet { get; set; } = Environment.WorkingSet;
    public TimeSpan Uptime { get; set; } = TimeSpan.FromMilliseconds(Environment.TickCount64);
}