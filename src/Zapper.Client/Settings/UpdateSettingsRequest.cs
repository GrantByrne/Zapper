using Zapper.Core.Models;

namespace Zapper.Client.Settings;

public class UpdateSettingsRequest
{
    public ZapperSettings Settings { get; set; } = new();
}