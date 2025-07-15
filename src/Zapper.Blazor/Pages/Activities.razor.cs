using Microsoft.AspNetCore.Components;
using MudBlazor;
using Zapper.Client.Abstractions;
using Zapper.Core.Models;

namespace Zapper.Blazor.Pages;

public partial class Activities : ComponentBase
{
    [Inject] public IZapperApiClient? ApiClient { get; set; }

    private List<Activity> _activities = new();
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

            // Note: This assumes the activities endpoint exists in the API client
            // If not, we'll fall back to sample data for now
            try
            {
                // TODO: Once IActivityClient is implemented, use:
                // var activities = await ApiClient.Activities.GetAllActivitiesAsync();
                // _activities = activities.ToList();

                // For now, fall back to sample data since the client doesn't have activities yet
                LoadSampleActivities();
                await Task.CompletedTask;
            }
            catch (NotImplementedException)
            {
                // Activities API client not implemented yet, use sample data
                LoadSampleActivities();
            }
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to load activities: {ex.Message}";
            LoadSampleActivities(); // Fallback to sample data
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private void LoadSampleActivities()
    {
        _activities = new List<Activity>
        {
            new()
            {
                Id = 1,
                Name = "Watch Movie",
                Description = "Turn on TV, sound bar, and switch to movie input",
                IsEnabled = true,
                SortOrder = 1,
                CreatedAt = DateTime.Now.AddDays(-5),
                LastUsed = DateTime.Now.AddHours(-2)
            },
            new()
            {
                Id = 2,
                Name = "Listen to Music",
                Description = "Turn on sound bar and set to music mode",
                IsEnabled = true,
                SortOrder = 2,
                CreatedAt = DateTime.Now.AddDays(-3),
                LastUsed = DateTime.Now.AddDays(-1)
            },
            new()
            {
                Id = 3,
                Name = "Gaming Setup",
                Description = "Configure all devices for gaming",
                IsEnabled = true,
                SortOrder = 3,
                CreatedAt = DateTime.Now.AddDays(-1),
                LastUsed = DateTime.Now.AddMinutes(-30)
            }
        };
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
                // TODO: Once IActivityClient is implemented, use:
                // var newActivity = new Activity
                // {
                //     Name = _newActivity.Name,
                //     Description = _newActivity.Description,
                //     IsEnabled = true,
                //     SortOrder = _activities.Count + 1,
                //     CreatedAt = DateTime.Now
                // };
                // var createdActivity = await ApiClient.Activities.CreateActivityAsync(newActivity);
                // _activities.Add(createdActivity);

                // For now, add to local list since API client is not complete
                var newActivity = new Activity
                {
                    Id = _activities.Count + 1,
                    Name = _newActivity.Name,
                    Description = _newActivity.Description,
                    IsEnabled = true,
                    SortOrder = _activities.Count + 1,
                    CreatedAt = DateTime.Now
                };
                _activities.Add(newActivity);

                _newActivity = new ActivityModel();
                _showAddDialog = false;
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

    private async Task RunActivity(Activity activity)
    {
        try
        {
            if (ApiClient == null)
            {
                _errorMessage = "API client not configured";
                return;
            }

            // TODO: Once IActivityClient is implemented, use:
            // await ApiClient.Activities.ExecuteActivityAsync(activity.Id);

            // For now, simulate execution
            activity.LastUsed = DateTime.Now;
            StateHasChanged();

            // Simulate activity execution time
            await Task.Delay(2000);

            StateHasChanged();
        }
        catch (Exception ex)
        {
            _errorMessage = $"Failed to execute activity: {ex.Message}";
            StateHasChanged();
        }
    }

    private void EditActivity(Activity activity)
    {
        // TODO: Implement activity editing dialog
    }

    private async Task DeleteActivity(Activity activity)
    {
        try
        {
            // TODO: Once IActivityClient is implemented, use:
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

    private int GetDeviceCount(Activity activity)
    {
        // TODO: Calculate based on actual activity devices when available
        return 2; // Default placeholder
    }

    private int GetStepCount(Activity activity)
    {
        // TODO: Calculate based on actual activity steps when available
        return 3; // Default placeholder
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