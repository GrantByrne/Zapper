using Zapper.Core.Models;

namespace Zapper.API.Endpoints.Settings;

public class UpdateSettingsRequest
{
    public ZapperSettings Settings { get; set; } = new();
}