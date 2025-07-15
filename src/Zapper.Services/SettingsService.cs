using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Zapper.Core.Models;

namespace Zapper.Services;

public class SettingsService(IConfiguration configuration, ILogger<SettingsService> logger) : ISettingsService
{
    private readonly string _settingsFilePath = "zapper-settings.json";
    private ZapperSettings? _cachedSettings;

    public async Task<ZapperSettings> GetSettingsAsync()
    {
        if (_cachedSettings != null)
            return _cachedSettings;

        try
        {
            if (File.Exists(_settingsFilePath))
            {
                var json = await File.ReadAllTextAsync(_settingsFilePath);
                _cachedSettings = JsonSerializer.Deserialize<ZapperSettings>(json) ?? new ZapperSettings();
            }
            else
            {
                _cachedSettings = CreateDefaultSettings();
            }

            OverrideWithConfigurationValues(_cachedSettings);
            logger.LogInformation("Settings loaded successfully");
            return _cachedSettings;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading settings, using defaults");
            _cachedSettings = CreateDefaultSettings();
            OverrideWithConfigurationValues(_cachedSettings);
            return _cachedSettings;
        }
    }

    public async Task UpdateSettingsAsync(ZapperSettings settings)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(settings, options);
            await File.WriteAllTextAsync(_settingsFilePath, json);

            _cachedSettings = settings;
            logger.LogInformation("Settings saved successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving settings");
            throw;
        }
    }

    public async Task<T> GetSettingAsync<T>(string key, T defaultValue = default!)
    {
        var settings = await GetSettingsAsync();

        try
        {
            var value = GetNestedProperty(settings, key);
            if (value is T typedValue)
                return typedValue;

            if (value != null && typeof(T) != typeof(object))
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error getting setting {Key}, using default", key);
        }

        return defaultValue;
    }

    public async Task SetSettingAsync<T>(string key, T value)
    {
        var settings = await GetSettingsAsync();

        try
        {
            SetNestedProperty(settings, key, value);
            await UpdateSettingsAsync(settings);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error setting {Key} to {Value}", key, value);
            throw;
        }
    }

    private ZapperSettings CreateDefaultSettings()
    {
        return new ZapperSettings();
    }

    private void OverrideWithConfigurationValues(ZapperSettings settings)
    {
        settings.Hardware.EnableGpio = configuration.GetValue<bool>("Hardware:EnableGPIO", true);
        settings.Hardware.Infrared.TransmitterGpioPin = configuration.GetValue<int>("Hardware:IRTransmitter:GpioPin", 18);
        settings.Hardware.Infrared.ReceiverGpioPin = configuration.GetValue<int>("Hardware:IRReceiver:GpioPin", 19);
        settings.Hardware.Infrared.CarrierFrequency = configuration.GetValue<int>("Hardware:IRTransmitter:CarrierFrequency", 38000);
        settings.Hardware.Infrared.DutyCycle = configuration.GetValue<double>("Hardware:IRTransmitter:DutyCycle", 0.33);
    }

    private object? GetNestedProperty(object obj, string propertyPath)
    {
        var properties = propertyPath.Split('.');
        var current = obj;

        foreach (var property in properties)
        {
            var type = current.GetType();
            var prop = type.GetProperty(property);
            if (prop == null)
                return null;
            current = prop.GetValue(current);
            if (current == null)
                return null;
        }

        return current;
    }

    private void SetNestedProperty(object obj, string propertyPath, object? value)
    {
        var properties = propertyPath.Split('.');
        var current = obj;

        for (int i = 0; i < properties.Length - 1; i++)
        {
            var type = current.GetType();
            var prop = type.GetProperty(properties[i]);
            if (prop == null)
                throw new ArgumentException($"Property {properties[i]} not found");

            var nextValue = prop.GetValue(current);
            if (nextValue == null)
            {
                nextValue = Activator.CreateInstance(prop.PropertyType);
                prop.SetValue(current, nextValue);
            }
            current = nextValue!;
        }

        var finalProperty = current?.GetType().GetProperty(properties[^1]);
        if (finalProperty == null)
            throw new ArgumentException($"Property {properties[^1]} not found");

        finalProperty.SetValue(current, value);
    }
}