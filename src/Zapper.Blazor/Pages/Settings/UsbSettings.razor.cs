using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Net.Http.Json;
using Zapper.Core.Models;

namespace Zapper.Blazor.Pages.Settings;

public partial class UsbSettings(HttpClient httpClient, ISnackbar snackbar) : ComponentBase
{
    private bool _enableUsbRemotes = true;
    private List<UsbRemote> _connectedRemotes = new();
    private UsbPermissionStatus? _usbPermissionStatus;
    private UsbPermissionFixResult? _fixResult;
    private bool _isCheckingUsbPermissions;
    private bool _isFixingPermissions;
    private bool _showFixPermissionsDialog;
    private bool _isLoading = true;
    private bool _isSaving;
    private ZapperSettings? _currentSettings;
    private string _sudoPassword = string.Empty;
    private bool _showPassword = false;

    private readonly DialogOptions _dialogOptions = new()
    {
        CloseButton = true,
        MaxWidth = MaxWidth.Medium,
        FullWidth = true
    };

    protected override async Task OnInitializedAsync()
    {
        await Task.WhenAll(
            LoadSettings(),
            LoadConnectedRemotes(),
            CheckUsbPermissions()
        );
    }

    private async Task LoadSettings()
    {
        try
        {
            _isLoading = true;
            var response = await httpClient.GetAsync("/api/settings");
            if (response.IsSuccessStatusCode)
            {
                _currentSettings = await response.Content.ReadFromJsonAsync<ZapperSettings>();
                if (_currentSettings != null)
                {
                    _enableUsbRemotes = _currentSettings.Hardware.EnableUsbRemotes;
                }
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error loading settings: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task LoadConnectedRemotes()
    {
        try
        {
            var response = await httpClient.GetFromJsonAsync<IEnumerable<UsbRemote>>("/api/usb-remotes");
            _connectedRemotes = response?.ToList() ?? new List<UsbRemote>();
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error loading USB remotes: {ex.Message}", Severity.Warning);
        }
    }

    private async Task CheckUsbPermissions()
    {
        try
        {
            _isCheckingUsbPermissions = true;
            _usbPermissionStatus = await httpClient.GetFromJsonAsync<UsbPermissionStatus>("/api/diagnostics/usb-permissions");
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error checking USB permissions: {ex.Message}", Severity.Warning);
        }
        finally
        {
            _isCheckingUsbPermissions = false;
        }
    }

    private void ShowFixPermissionsDialog()
    {
        _fixResult = null;
        _sudoPassword = string.Empty;
        _showPassword = false;
        _showFixPermissionsDialog = true;
    }

    private void TogglePasswordVisibility()
    {
        _showPassword = !_showPassword;
    }

    private async Task FixUsbPermissions()
    {
        if (string.IsNullOrWhiteSpace(_sudoPassword))
        {
            snackbar.Add("Please enter your password", Severity.Warning);
            return;
        }

        try
        {
            _isFixingPermissions = true;

            var request = new { Password = _sudoPassword };
            var response = await httpClient.PostAsJsonAsync("/api/diagnostics/fix-usb-permissions", request);
            _fixResult = await response.Content.ReadFromJsonAsync<UsbPermissionFixResult>();

            if (_fixResult?.Success == true)
            {
                snackbar.Add("USB permissions fixed successfully", Severity.Success);
                await Task.Delay(2000);
                await CheckUsbPermissions();
                _showFixPermissionsDialog = false;
            }
            else
            {
                snackbar.Add(_fixResult?.Message ?? "Failed to fix USB permissions", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error fixing permissions: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isFixingPermissions = false;
            _sudoPassword = string.Empty; // Clear password after use
        }
    }

    private async Task SaveSettings()
    {
        try
        {
            _isSaving = true;

            if (_currentSettings == null)
            {
                _currentSettings = new ZapperSettings();
            }

            _currentSettings.Hardware.EnableUsbRemotes = _enableUsbRemotes;

            var response = await httpClient.PutAsJsonAsync("/api/settings", new { Settings = _currentSettings });

            if (response.IsSuccessStatusCode)
            {
                snackbar.Add("USB settings saved successfully", Severity.Success);
            }
            else
            {
                snackbar.Add("Failed to save settings", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            snackbar.Add($"Error saving settings: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isSaving = false;
        }
    }
}