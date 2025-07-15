using Zapper.Core.Models;

namespace Zapper.Services;

public interface ISettingsService
{
    Task<ZapperSettings> GetSettingsAsync();
    Task UpdateSettingsAsync(ZapperSettings settings);
    Task<T> GetSettingAsync<T>(string key, T defaultValue = default!);
    Task SetSettingAsync<T>(string key, T value);
}