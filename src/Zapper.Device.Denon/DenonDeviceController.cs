using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Web;
using Microsoft.Extensions.Logging;

namespace Zapper.Device.Denon;

public class DenonDeviceController(HttpClient httpClient, ILogger<DenonDeviceController> logger) : IDenonDeviceController
{
    private readonly Dictionary<string, string> _inputMappings = new()
    {
        { "PHONO", "Phono" },
        { "CD", "CD" },
        { "TUNER", "Tuner" },
        { "DVD", "DVD" },
        { "BD", "Blu-ray" },
        { "TV", "TV Audio" },
        { "SAT/CBL", "SAT/Cable" },
        { "MPLAY", "Media Player" },
        { "GAME", "Game" },
        { "HDRADIO", "HD Radio" },
        { "NET", "Network" },
        { "PANDORA", "Pandora" },
        { "SIRIUSXM", "SiriusXM" },
        { "SPOTIFY", "Spotify" },
        { "LASTFM", "Last.fm" },
        { "FLICKR", "Flickr" },
        { "IRADIO", "Internet Radio" },
        { "SERVER", "Server" },
        { "FAVORITES", "Favorites" },
        { "AUX1", "Aux 1" },
        { "AUX2", "Aux 2" },
        { "AUX3", "Aux 3" },
        { "AUX4", "Aux 4" },
        { "AUX5", "Aux 5" },
        { "AUX6", "Aux 6" },
        { "AUX7", "Aux 7" },
        { "BT", "Bluetooth" },
        { "USB", "USB" }
    };

    public async Task<bool> SetPowerAsync(bool powerOn, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = powerOn ? "PWON" : "PWSTANDBY";
            return await SendCommandAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set power to {PowerState}", powerOn ? "ON" : "OFF");
            return false;
        }
    }

    public async Task<bool> GetPowerStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await SendCommandWithResponseAsync("PW?", cancellationToken);
            return response?.StartsWith("PWON") ?? false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get power status");
            return false;
        }
    }

    public async Task<bool> SetVolumeAsync(int volume, CancellationToken cancellationToken = default)
    {
        try
        {
            if (volume < 0) volume = 0;
            if (volume > 98) volume = 98;

            var denonVolume = (int)(volume * 0.8);
            var command = $"MV{denonVolume:00}";
            return await SendCommandAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set volume to {Volume}", volume);
            return false;
        }
    }

    public async Task<int> GetVolumeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await SendCommandWithResponseAsync("MV?", cancellationToken);
            if (!string.IsNullOrEmpty(response) && response.StartsWith("MV"))
            {
                var volumeStr = response[2..];
                if (int.TryParse(volumeStr, out var denonVolume))
                {
                    return (int)(denonVolume / 0.8);
                }
            }
            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get volume");
            return 0;
        }
    }

    public async Task<bool> SetMuteAsync(bool mute, CancellationToken cancellationToken = default)
    {
        try
        {
            var command = mute ? "MUON" : "MUOFF";
            return await SendCommandAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set mute to {Mute}", mute);
            return false;
        }
    }

    public async Task<bool> GetMuteStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await SendCommandWithResponseAsync("MU?", cancellationToken);
            return response?.StartsWith("MUON") ?? false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get mute status");
            return false;
        }
    }

    public async Task<bool> SetInputAsync(string input, CancellationToken cancellationToken = default)
    {
        try
        {
            var denonInput = GetDenonInputCode(input);
            var command = $"SI{denonInput}";
            return await SendCommandAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set input to {Input}", input);
            return false;
        }
    }

    public async Task<string> GetCurrentInputAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await SendCommandWithResponseAsync("SI?", cancellationToken);
            if (!string.IsNullOrEmpty(response) && response.StartsWith("SI"))
            {
                var inputCode = response[2..];
                return GetFriendlyInputName(inputCode);
            }
            return string.Empty;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get current input");
            return string.Empty;
        }
    }

    public async Task<bool> VolumeUpAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await SendCommandAsync("MVUP", cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to increase volume");
            return false;
        }
    }

    public async Task<bool> VolumeDownAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await SendCommandAsync("MVDOWN", cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to decrease volume");
            return false;
        }
    }

    public async Task<Dictionary<string, string>> GetAvailableInputsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var availableInputs = new Dictionary<string, string>();

            foreach (var input in _inputMappings)
            {
                var command = $"SI{input.Key}";
                var response = await SendCommandWithResponseAsync(command + "?", cancellationToken);
                if (!string.IsNullOrEmpty(response) && !response.Contains("Command Error"))
                {
                    availableInputs[input.Key] = input.Value;
                }
            }

            return availableInputs;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get available inputs");
            return _inputMappings;
        }
    }

    public async Task<string> GetModelInfoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetStringAsync("/goform/Deviceinfo.xml", cancellationToken);

            var modelStart = response.IndexOf("<ModelName>") + 11;
            var modelEnd = response.IndexOf("</ModelName>");
            if (modelStart > 10 && modelEnd > modelStart)
            {
                return response[modelStart..modelEnd];
            }

            return "Denon AVR";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get model info");
            return "Denon AVR";
        }
    }

    public async Task<bool> SetZonePowerAsync(string zone, bool powerOn, CancellationToken cancellationToken = default)
    {
        try
        {
            var zonePrefix = GetZonePrefix(zone);
            var command = $"{zonePrefix}{(powerOn ? "ON" : "OFF")}";
            return await SendCommandAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set zone {Zone} power to {PowerState}", zone, powerOn ? "ON" : "OFF");
            return false;
        }
    }

    public async Task<bool> SetZoneVolumeAsync(string zone, int volume, CancellationToken cancellationToken = default)
    {
        try
        {
            if (volume < 0) volume = 0;
            if (volume > 98) volume = 98;

            var zonePrefix = GetZonePrefix(zone);
            var denonVolume = (int)(volume * 0.8);
            var command = $"{zonePrefix}{denonVolume:00}";
            return await SendCommandAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set zone {Zone} volume to {Volume}", zone, volume);
            return false;
        }
    }

    public async Task<bool> SetZoneInputAsync(string zone, string input, CancellationToken cancellationToken = default)
    {
        try
        {
            var zonePrefix = GetZonePrefix(zone);
            var denonInput = GetDenonInputCode(input);
            var command = $"{zonePrefix}{denonInput}";
            return await SendCommandAsync(command, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to set zone {Zone} input to {Input}", zone, input);
            return false;
        }
    }

    private async Task<bool> SendCommandAsync(string command, CancellationToken cancellationToken)
    {
        try
        {
            var encodedCommand = HttpUtility.UrlEncode(command);
            var url = $"/goform/formiPhoneAppDirect.xml?{encodedCommand}";

            var response = await httpClient.GetAsync(url, cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send command: {Command}", command);
            return false;
        }
    }

    private async Task<string?> SendCommandWithResponseAsync(string command, CancellationToken cancellationToken)
    {
        try
        {
            var encodedCommand = HttpUtility.UrlEncode(command);
            var url = $"/goform/AppCommand.xml";

            var content = new StringContent($"cmd1={encodedCommand}", Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await httpClient.PostAsync(url, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseText = await response.Content.ReadAsStringAsync(cancellationToken);
                var startTag = "<cmd>";
                var endTag = "</cmd>";
                var startIndex = responseText.IndexOf(startTag) + startTag.Length;
                var endIndex = responseText.IndexOf(endTag);

                if (startIndex >= startTag.Length && endIndex > startIndex)
                {
                    return responseText[startIndex..endIndex].Trim();
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send command with response: {Command}", command);
            return null;
        }
    }

    private string GetDenonInputCode(string input)
    {
        var upperInput = input.ToUpperInvariant();

        foreach (var mapping in _inputMappings)
        {
            if (mapping.Value.Equals(input, StringComparison.OrdinalIgnoreCase) ||
                mapping.Key.Equals(upperInput, StringComparison.OrdinalIgnoreCase))
            {
                return mapping.Key;
            }
        }

        return upperInput;
    }

    private string GetFriendlyInputName(string denonCode)
    {
        return _inputMappings.TryGetValue(denonCode.ToUpperInvariant(), out var friendlyName)
            ? friendlyName
            : denonCode;
    }

    private string GetZonePrefix(string zone)
    {
        return zone.ToUpperInvariant() switch
        {
            "MAIN" or "1" => "MV",
            "ZONE2" or "2" => "Z2",
            "ZONE3" or "3" => "Z3",
            _ => "MV"
        };
    }
}