namespace Zapper.Blazor.Components.AddDeviceWizard.Models;

public class WebOsDeviceModel
{
    public string Name { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string? ModelName { get; set; }
    public string? ModelNumber { get; set; }
    public int Port { get; set; } = 3000;
}