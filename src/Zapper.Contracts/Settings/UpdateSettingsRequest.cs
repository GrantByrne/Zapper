using Zapper.Core.Models;

namespace Zapper.Contracts.Settings;

public class UpdateSettingsRequest
{
    public ZapperSettings Settings { get; set; } = new();
}