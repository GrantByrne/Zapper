using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using System.Net.Http.Json;
using Zapper.Client;
using Zapper.Core.Models;

namespace Zapper.Blazor.Pages;

public partial class UsbRemotes(HttpClient httpClient, NavigationManager navigation) : ComponentBase, IAsyncDisposable
{
    private List<UsbRemote> _remotes = new();
    private bool _isLoading = true;
    private string? _errorMessage;
    private HubConnection? _hubConnection;
    private bool _showSettingsDialog;
    private UsbRemote? _selectedRemote;
    private MudForm? _settingsForm;

    private readonly DialogOptions _dialogOptions = new()
    {
        CloseButton = true,
        MaxWidth = MaxWidth.Small,
        FullWidth = true
    };

    protected override async Task OnInitializedAsync()
    {
        await LoadRemotes();
        await SetupSignalRConnection();
    }

    private async Task LoadRemotes()
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;

            var response = await httpClient.GetFromJsonAsync<IEnumerable<UsbRemote>>("/api/usb-remotes");
            _remotes = response?.ToList() ?? new List<UsbRemote>();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load USB remotes: {ex.Message}";
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task SetupSignalRConnection()
    {
        try
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(navigation.ToAbsoluteUri("/hubs/zapper"))
                .Build();

            _hubConnection.On<object>("UsbRemoteStatusChanged", async (data) =>
            {
                await LoadRemotes();
                StateHasChanged();
            });

            await _hubConnection.StartAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SignalR connection failed: {ex.Message}");
        }
    }

    private void ConfigureRemote(UsbRemote remote)
    {
        navigation.NavigateTo($"/usb-remotes/{remote.Id}/configure");
    }

    private void EditRemoteSettings(UsbRemote remote)
    {
        _selectedRemote = new UsbRemote
        {
            Id = remote.Id,
            Name = remote.Name,
            IsActive = remote.IsActive,
            InterceptSystemButtons = remote.InterceptSystemButtons,
            LongPressTimeoutMs = remote.LongPressTimeoutMs
        };
        _showSettingsDialog = true;
    }

    private async Task SaveRemoteSettings()
    {
        if (_selectedRemote == null) return;

        try
        {
            var request = new
            {
                _selectedRemote.Id,
                _selectedRemote.Name,
                _selectedRemote.IsActive,
                _selectedRemote.InterceptSystemButtons,
                _selectedRemote.LongPressTimeoutMs
            };

            var response = await httpClient.PutAsJsonAsync($"/api/usb-remotes/{_selectedRemote.Id}", request);

            if (response.IsSuccessStatusCode)
            {
                _showSettingsDialog = false;
                await LoadRemotes();
            }
            else
            {
                _errorMessage = "Failed to save remote settings";
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to save settings: {ex.Message}";
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}