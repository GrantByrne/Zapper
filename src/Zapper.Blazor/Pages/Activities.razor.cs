using Microsoft.AspNetCore.Components;
using MudBlazor;
using Zapper.Client;
using Zapper.Contracts;
using Zapper.Contracts.Activities;

namespace Zapper.Blazor.Pages;

public partial class Activities(IZapperApiClient? apiClient) : ComponentBase
{

    private List<ActivityDto> _activities = new();
    private bool _showAddDialog;
    private bool _isLoading = true;
    private string? _errorMessage;
    private ActivityModel _newActivity = new();

    private DialogOptions _dialogOptions = new() { MaxWidth = MaxWidth.Medium, FullWidth = true };

    protected override async Task OnInitializedAsync()
    {
        await LoadActivities();
        _newActivity = new ActivityModel();
    }

    private async Task LoadActivities()
    {
        try
        {
            _isLoading = true;
            _errorMessage = null;

            if (apiClient == null)
            {
                _errorMessage = "API client not configured";
                return;
            }

            var activities = await apiClient.Activities.GetAllActivitiesAsync();
            _activities = activities.ToList();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load activities: {ex.Message}";
            _activities.Clear();
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }


    private string GetActivityIcon(string activityName)
    {
        var name = activityName.ToLowerInvariant();
        return name switch
        {
            var n when n.Contains("movie") || n.Contains("film") => Icons.Material.Filled.Movie,
            var n when n.Contains("music") || n.Contains("audio") => Icons.Material.Filled.LibraryMusic,
            var n when n.Contains("game") || n.Contains("gaming") => Icons.Material.Filled.SportsEsports,
            var n when n.Contains("tv") || n.Contains("television") => Icons.Material.Filled.Tv,
            var n when n.Contains("sport") => Icons.Material.Filled.Sports,
            _ => Icons.Material.Filled.PlayArrow
        };
    }

    private void AddStep()
    {
        _newActivity.Steps.Add(new ActivityStep());
    }

    private void RemoveStep(int index)
    {
        _newActivity.Steps.RemoveAt(index);
    }

    private async Task CreateActivity()
    {
        if (!string.IsNullOrWhiteSpace(_newActivity.Name) && !string.IsNullOrWhiteSpace(_newActivity.Type))
        {
            try
            {
                var createRequest = new CreateActivityRequest
                {
                    Name = _newActivity.Name,
                    Description = _newActivity.Description,
                    IsEnabled = true,
                    Type = _newActivity.Type,
                    Steps = _newActivity.Steps.Select((step, index) => new CreateActivityStepRequest
                    {
                        DeviceId = step.DeviceId,
                        Command = step.Command,
                        DelayMs = 500,
                        SortOrder = index
                    }).ToList()
                };

                var createdActivity = await apiClient!.Activities.CreateActivityAsync(createRequest);
                _activities.Add(createdActivity);

                _errorMessage = null;
                _newActivity = new ActivityModel();
                StateHasChanged();
            }
            catch (Exception ex)
            {
                _errorMessage = $"Failed to create activity: {ex.Message}";
                StateHasChanged();
            }
        }
    }

    private async Task RunActivity(ActivityDto activity)
    {
        try
        {
            if (apiClient == null)
            {
                _errorMessage = "API client not configured";
                return;
            }

            var response = await apiClient.Activities.ExecuteActivityAsync(activity.Id);

            if (response.Success)
            {
                activity.LastUsed = DateTime.Now;
                StateHasChanged();
            }
            else
            {
                _errorMessage = response.Message ?? "Failed to execute activity";
                StateHasChanged();
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to execute activity: {ex.Message}";
            StateHasChanged();
        }
    }

    private void EditActivity(ActivityDto activity)
    {
        _errorMessage = "Activity editing is not yet implemented in the UI.";
        StateHasChanged();
    }

    private async Task DeleteActivity(ActivityDto activity)
    {
        try
        {
            await apiClient!.Activities.DeleteActivityAsync(activity.Id);
            _activities.Remove(activity);
            StateHasChanged();

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to delete activity: {ex.Message}";
            StateHasChanged();
        }
    }

    private int GetDeviceCount(ActivityDto activity)
    {
        // Count unique device IDs from steps
        return activity.Steps.Select(s => s.DeviceId).Distinct().Count();
    }

    private int GetStepCount(ActivityDto activity)
    {
        return activity.Steps.Count;
    }

    public class ActivityModel
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Type { get; set; } = "";
        public int DeviceCount { get; set; }
        public int StepCount { get; set; }
        public bool IsActive { get; set; }
        public List<ActivityStep> Steps { get; set; } = new();
    }

    public class ActivityStep
    {
        public int DeviceId { get; set; }
        public string Device { get; set; } = "";
        public string Command { get; set; } = "";
    }
}