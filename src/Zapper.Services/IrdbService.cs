using System.Text.Json;
using Microsoft.Extensions.Logging;
using Zapper.Core.Models;
using Zapper.Data;
using Microsoft.EntityFrameworkCore;

namespace Zapper.Services;

public class IrdbService(HttpClient httpClient, ZapperContext context, ILogger<IrdbService> logger) : IExternalIrCodeService
{
    private const string BaseUrl = "https://cdn.jsdelivr.net/gh/probonopd/irdb@master/codes";
    private const int CacheExpiryHours = 24;

    public async Task<IEnumerable<string>> GetAvailableManufacturersAsync()
    {
        const string cacheKey = "irdb:manufacturers";

        var cachedData = await GetFromCacheAsync(cacheKey);
        if (cachedData != null)
        {
            return JsonSerializer.Deserialize<IEnumerable<string>>(cachedData) ?? [];
        }

        try
        {
            var indexUrl = $"{BaseUrl}/index";
            var response = await httpClient.GetStringAsync(indexUrl);
            var manufacturers = ParseManufacturersFromIndex(response);

            await CacheDataAsync(cacheKey, JsonSerializer.Serialize(manufacturers));
            return manufacturers;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch manufacturers from IRDB");
            return [];
        }
    }

    public async Task<IEnumerable<(string Manufacturer, string DeviceType, string Device, string Subdevice)>> SearchDevicesAsync(string? manufacturer = null, string? deviceType = null)
    {
        var cacheKey = $"irdb:devices:{manufacturer}:{deviceType}";

        var cachedData = await GetFromCacheAsync(cacheKey);
        if (cachedData != null)
        {
            return JsonSerializer.Deserialize<IEnumerable<(string, string, string, string)>>(cachedData) ?? [];
        }

        try
        {
            var indexUrl = $"{BaseUrl}/index";
            var response = await httpClient.GetStringAsync(indexUrl);
            var devices = ParseDevicesFromIndex(response, manufacturer, deviceType);

            await CacheDataAsync(cacheKey, JsonSerializer.Serialize(devices));
            return devices;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to search devices from IRDB");
            return [];
        }
    }

    public async Task<IrCodeSet?> GetCodeSetAsync(string manufacturer, string deviceType, string device, string subdevice)
    {
        var cacheKey = $"irdb:codeset:{manufacturer}:{deviceType}:{device}:{subdevice}";

        var cachedData = await GetFromCacheAsync(cacheKey);
        if (cachedData != null)
        {
            return JsonSerializer.Deserialize<IrCodeSet>(cachedData);
        }

        try
        {
            var csvUrl = $"{BaseUrl}/{manufacturer}/{deviceType}/{device},{subdevice}.csv";
            var csvContent = await httpClient.GetStringAsync(csvUrl);
            var codeSet = ParseCsvToCodeSet(csvContent, manufacturer, deviceType, device);

            if (codeSet != null)
            {
                await CacheDataAsync(cacheKey, JsonSerializer.Serialize(codeSet));
            }

            return codeSet;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch code set for {Manufacturer}/{DeviceType}/{Device},{Subdevice}",
                manufacturer, deviceType, device, subdevice);
            return null;
        }
    }

    public async Task<bool> IsAvailableAsync()
    {
        try
        {
            var response = await httpClient.GetAsync($"{BaseUrl}/index");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task InvalidateCacheAsync()
    {
        var expiredEntries = await context.ExternalIrCodeCache
            .Where(e => e.CacheKey.StartsWith("irdb:"))
            .ToListAsync();

        context.ExternalIrCodeCache.RemoveRange(expiredEntries);
        await context.SaveChangesAsync();

        logger.LogInformation("Invalidated {Count} IRDB cache entries", expiredEntries.Count);
    }

    private async Task<string?> GetFromCacheAsync(string cacheKey)
    {
        var cacheEntry = await context.ExternalIrCodeCache
            .FirstOrDefaultAsync(e => e.CacheKey == cacheKey && e.ExpiresAt > DateTime.UtcNow);

        return cacheEntry?.CachedData;
    }

    private async Task CacheDataAsync(string cacheKey, string data)
    {
        var existingEntry = await context.ExternalIrCodeCache
            .FirstOrDefaultAsync(e => e.CacheKey == cacheKey);

        if (existingEntry != null)
        {
            existingEntry.CachedData = data;
            existingEntry.ExpiresAt = DateTime.UtcNow.AddHours(CacheExpiryHours);
        }
        else
        {
            context.ExternalIrCodeCache.Add(new ExternalIrCodeCache
            {
                CacheKey = cacheKey,
                CachedData = data,
                ExpiresAt = DateTime.UtcNow.AddHours(CacheExpiryHours)
            });
        }

        await context.SaveChangesAsync();
    }

    private static IEnumerable<string> ParseManufacturersFromIndex(string indexContent)
    {
        var manufacturers = new HashSet<string>();

        var lines = indexContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var parts = line.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 1)
            {
                manufacturers.Add(parts[0]);
            }
        }

        return manufacturers.Order();
    }

    private static IEnumerable<(string Manufacturer, string DeviceType, string Device, string Subdevice)> ParseDevicesFromIndex(
        string indexContent, string? manufacturerFilter, string? deviceTypeFilter)
    {
        var devices = new List<(string, string, string, string)>();

        var lines = indexContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            var parts = line.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3)
            {
                var manufacturer = parts[0];
                var deviceType = parts[1];
                var deviceFile = parts[2];

                if (manufacturerFilter != null && !manufacturer.Equals(manufacturerFilter, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (deviceTypeFilter != null && !deviceType.Equals(deviceTypeFilter, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (deviceFile.EndsWith(".csv"))
                {
                    var deviceSubdevice = deviceFile[..^4];
                    var deviceParts = deviceSubdevice.Split(',');
                    if (deviceParts.Length == 2)
                    {
                        devices.Add((manufacturer, deviceType, deviceParts[0], deviceParts[1]));
                    }
                }
            }
        }

        return devices;
    }

    private static IrCodeSet? ParseCsvToCodeSet(string csvContent, string manufacturer, string deviceType, string device)
    {
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0) return null;

        var codes = new List<IrCode>();

        foreach (var line in lines)
        {
            var parts = line.Split(',');
            if (parts.Length >= 4)
            {
                var functionName = parts[0].Trim();
                var protocol = parts[1].Trim();
                var deviceCode = parts[2].Trim();
                var functionCode = parts[3].Trim();

                if (!string.IsNullOrWhiteSpace(functionName) && !string.IsNullOrWhiteSpace(protocol))
                {
                    var deviceTypeEnum = MapDeviceType(deviceType);
                    var hexCode = ConvertProtocolToHex(protocol, deviceCode, functionCode);

                    codes.Add(new IrCode
                    {
                        Brand = manufacturer,
                        Model = device,
                        DeviceType = deviceTypeEnum,
                        CommandName = functionName,
                        Protocol = protocol,
                        HexCode = hexCode,
                        Frequency = 38000,
                        Notes = $"Imported from IRDB - Device: {deviceCode}, Function: {functionCode}"
                    });
                }
            }
        }

        if (codes.Count == 0) return null;

        return new IrCodeSet
        {
            Brand = manufacturer,
            Model = device,
            DeviceType = codes.First().DeviceType,
            Description = $"External IR codes from IRDB for {manufacturer} {device}",
            IsVerified = false,
            Codes = codes
        };
    }

    private static DeviceType MapDeviceType(string deviceType)
    {
        return deviceType.ToLowerInvariant() switch
        {
            "tv" => DeviceType.Television,
            "dvd" => DeviceType.DvdPlayer,
            "stb" or "set-top-box" => DeviceType.CableBox,
            "ac" or "air-conditioner" => DeviceType.Receiver,
            "fan" => DeviceType.Receiver,
            "audio" or "stereo" => DeviceType.Receiver,
            _ => DeviceType.Television
        };
    }

    private static string ConvertProtocolToHex(string protocol, string device, string function)
    {
        if (int.TryParse(device, out var deviceInt) && int.TryParse(function, out var functionInt))
        {
            return protocol.ToUpperInvariant() switch
            {
                "NEC" => $"0x{(deviceInt << 8 | functionInt):X8}",
                "SONY" => $"0x{(deviceInt << 7 | functionInt):X6}",
                "RC5" => $"0x{(deviceInt << 6 | functionInt):X4}",
                "RC6" => $"0x{(deviceInt << 8 | functionInt):X6}",
                _ => $"0x{functionInt:X4}"
            };
        }

        return $"0x{function}";
    }
}