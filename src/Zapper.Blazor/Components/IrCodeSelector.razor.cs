using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using Zapper.Client.Abstractions;
using Zapper.Core.Models;

namespace Zapper.Blazor.Components;

public partial class IrCodeSelector(IZapperApiClient? apiClient, HttpClient httpClient, ISnackbar snackbar) : ComponentBase
{
    [Parameter] public DeviceType DeviceType { get; set; }
    [Parameter] public EventCallback<IrCodeSet> OnCodeSetSelected { get; set; }

    private bool _isLoadingLocal = false;
    private List<IrCodeSet> _localCodeSets = new();
    private string _searchBrand = "";
    private string _searchModel = "";

    private bool _isSearchingExternal = false;
    private bool _hasSearchedExternal = false;
    private string _externalManufacturer = "";
    private List<string> _manufacturers = new();
    private List<ExternalDeviceInfo> _externalDevices = new();

    private IrCodeSet? _selectedCodeSet;
    private bool _isTesting = false;
    private string _testResult = "";

    protected override async Task OnInitializedAsync()
    {
        await LoadLocalCodeSets();
    }

    private async Task LoadLocalCodeSets()
    {
        if (apiClient == null) return;

        try
        {
            _isLoadingLocal = true;
            var response = await httpClient.GetAsync($"/api/ir-codes/sets/search?deviceType={DeviceType}");
            if (response.IsSuccessStatusCode)
            {
                _localCodeSets = await response.Content.ReadFromJsonAsync<List<IrCodeSet>>() ?? new();
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Failed to load IR codes: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isLoadingLocal = false;
        }
    }

    private async Task SearchLocalCodes()
    {
        if (apiClient == null) return;

        try
        {
            _isLoadingLocal = true;
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(_searchBrand))
                queryParams.Add($"brand={Uri.EscapeDataString(_searchBrand)}");

            if (!string.IsNullOrWhiteSpace(_searchModel))
                queryParams.Add($"model={Uri.EscapeDataString(_searchModel)}");

            queryParams.Add($"deviceType={DeviceType}");

            var queryString = string.Join("&", queryParams);
            var response = await httpClient.GetAsync($"/api/ir-codes/sets/search?{queryString}");

            if (response.IsSuccessStatusCode)
            {
                _localCodeSets = await response.Content.ReadFromJsonAsync<List<IrCodeSet>>() ?? new();
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Search failed: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isLoadingLocal = false;
        }
    }

    private async Task LoadManufacturers()
    {
        if (_manufacturers.Any()) return;

        try
        {
            var response = await httpClient.GetAsync("/api/ir-codes/external/manufacturers");
            if (response.IsSuccessStatusCode)
            {
                _manufacturers = await response.Content.ReadFromJsonAsync<List<string>>() ?? new();
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Failed to load manufacturers: {ex.Message}", Severity.Error);
        }
    }

    private async Task<IEnumerable<string>> SearchManufacturers(string value, CancellationToken cancellationToken = default)
    {
        if (!_manufacturers.Any())
        {
            await LoadManufacturers();
        }

        if (string.IsNullOrEmpty(value))
            return _manufacturers.Take(10);

        return _manufacturers
            .Where(x => x.Contains(value, StringComparison.InvariantCultureIgnoreCase))
            .Take(10);
    }

    private async Task SearchExternalDevices()
    {
        if (string.IsNullOrEmpty(_externalManufacturer)) return;

        try
        {
            _isSearchingExternal = true;
            _hasSearchedExternal = true;
            _externalDevices.Clear();

            var response = await httpClient.GetAsync($"/api/ir-codes/external/devices/search?manufacturer={Uri.EscapeDataString(_externalManufacturer)}");
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<SearchExternalDevicesResponse>();
                _externalDevices = result?.Devices?.ToList() ?? new();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
            {
                snackbar.Add("External IR database is currently unavailable", Severity.Warning);
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Search failed: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isSearchingExternal = false;
        }
    }

    private async Task LoadExternalCodeSet(ExternalDeviceInfo device)
    {
        try
        {
            var response = await httpClient.GetAsync($"/api/ir-codes/external/codeset/{Uri.EscapeDataString(device.Manufacturer)}/{Uri.EscapeDataString(device.DeviceType)}/{Uri.EscapeDataString(device.Device)}/{Uri.EscapeDataString(device.Subdevice)}");

            if (response.IsSuccessStatusCode)
            {
                var codeSet = await response.Content.ReadFromJsonAsync<IrCodeSet>();
                if (codeSet != null)
                {
                    _selectedCodeSet = codeSet;
                    await OnCodeSetSelected.InvokeAsync(codeSet);
                    snackbar.Add($"Loaded {codeSet.Codes.Count} IR codes for {codeSet.Brand} {codeSet.Model}", Severity.Success);
                }
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                snackbar.Add("Code set not found in external database", Severity.Warning);
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Failed to load code set: {ex.Message}", Severity.Error);
        }
    }

    private void SelectCodeSet(IrCodeSet codeSet)
    {
        _selectedCodeSet = codeSet;
        _testResult = "";
        OnCodeSetSelected.InvokeAsync(codeSet);
    }

    private string GetCodeSetCardClass(IrCodeSet codeSet)
    {
        return _selectedCodeSet?.Id == codeSet.Id ? "mud-card selected" : "mud-card";
    }

    private async Task TestSelectedCodeSet()
    {
        if (_selectedCodeSet == null) return;

        try
        {
            _isTesting = true;
            _testResult = "";

            var powerCommand = _selectedCodeSet.Codes.FirstOrDefault(c =>
                c.CommandName.Equals("Power", StringComparison.OrdinalIgnoreCase));

            if (powerCommand == null)
            {
                _testResult = "No power command found in this code set";
                return;
            }

            var testRequest = new TestIrCodeRequest
            {
                CodeSetId = _selectedCodeSet.Id,
                CommandName = powerCommand.CommandName
            };

            var response = await httpClient.PostAsJsonAsync("/api/ir-codes/test", testRequest);

            if (response.IsSuccessStatusCode)
            {
                _testResult = "Test command sent successfully! Check if your device responded.";
            }
            else
            {
                _testResult = "Failed to send test command";
            }
        }
        catch (Exception ex)
        {
            _testResult = $"Error: {ex.Message}";
        }
        finally
        {
            _isTesting = false;
        }
    }
}

public class SearchExternalDevicesResponse
{
    public IEnumerable<ExternalDeviceInfo> Devices { get; set; } = [];
}

public class ExternalDeviceInfo
{
    public required string Manufacturer { get; set; }
    public required string DeviceType { get; set; }
    public required string Device { get; set; }
    public required string Subdevice { get; set; }
}

public class TestIrCodeRequest
{
    public int CodeSetId { get; set; }
    public string CommandName { get; set; } = "";
}