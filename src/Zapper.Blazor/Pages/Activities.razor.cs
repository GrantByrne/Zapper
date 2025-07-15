using Microsoft.AspNetCore.Components;
using MudBlazor;
using Zapper.Client.Abstractions;
using Zapper.Contracts.Activities;

namespace Zapper.Blazor.Pages;

public partial class Activities : ComponentBase
{
    [Inject] public IZapperApiClient? ApiClient { get; set; }

    private List<ActivityDto> _activities = new();
    private bool _showAddDialog = false;
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

            if (ApiClient == null)
            {
                _errorMessage = "API client not configured";
                return;
            }

            var activities = await ApiClient.Activities.GetAllActivitiesAsync();
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
                // TODO: Implement create activity API endpoint
                // var createRequest = new CreateActivityRequest
                // {
                //     Name = _newActivity.Name,
                //     Description = _newActivity.Description,
                //     IsEnabled = true,
                //     Steps = _newActivity.Steps.Select((step, index) => new CreateActivityStepRequest
                //     {
                //         DeviceId = step.DeviceId,
                //         Command = step.Command,
                //         DelayMs = 500,
                //         SortOrder = index
                //     }).ToList()
                // };
                // var createdActivity = await ApiClient.Activities.CreateActivityAsync(createRequest);
                // _activities.Add(createdActivity);

                // For now, show a message that this feature is not implemented
                _errorMessage = "Creating activities is not yet implemented in the API.";
                StateHasChanged();

                await Task.CompletedTask;
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
            if (ApiClient == null)
            {
                _errorMessage = "API client not configured";
                return;
            }

            var response = await ApiClient.Activities.ExecuteActivityAsync(activity.Id);
            
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
        // TODO: Implement activity editing dialog
    }

    private async Task DeleteActivity(ActivityDto activity)
    {
        try
        {
            // TODO: Implement delete activity API endpoint
            // await ApiClient.Activities.DeleteActivityAsync(activity.Id);

            // For now, remove from local list
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
        public string Device { get; set; } = "";
        public string Command { get; set; } = "";
    }
}