using Microsoft.EntityFrameworkCore;
using Zapper.Data;
using Zapper.Models;

namespace Zapper.Services;

public class ActivityService : IActivityService
{
    private readonly ZapperContext _context;
    private readonly IDeviceService _deviceService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ActivityService> _logger;

    public ActivityService(
        ZapperContext context,
        IDeviceService deviceService,
        INotificationService notificationService,
        ILogger<ActivityService> logger)
    {
        _context = context;
        _deviceService = deviceService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<IEnumerable<Activity>> GetAllActivitiesAsync()
    {
        return await _context.Activities
            .Include(a => a.ActivityDevices)
                .ThenInclude(ad => ad.Device)
            .Include(a => a.Steps)
                .ThenInclude(s => s.DeviceCommand)
                    .ThenInclude(dc => dc.Device)
            .OrderBy(a => a.SortOrder)
            .ThenBy(a => a.Name)
            .ToListAsync();
    }

    public async Task<Activity?> GetActivityAsync(int id)
    {
        return await _context.Activities
            .Include(a => a.ActivityDevices)
                .ThenInclude(ad => ad.Device)
            .Include(a => a.Steps.OrderBy(s => s.StepOrder))
                .ThenInclude(s => s.DeviceCommand)
                    .ThenInclude(dc => dc.Device)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Activity> CreateActivityAsync(Activity activity)
    {
        activity.CreatedAt = DateTime.UtcNow;
        activity.LastUsed = DateTime.UtcNow;

        // Set sort order if not specified
        if (activity.SortOrder == 0)
        {
            var maxSortOrder = await _context.Activities.MaxAsync(a => (int?)a.SortOrder) ?? 0;
            activity.SortOrder = maxSortOrder + 1;
        }

        _context.Activities.Add(activity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created activity: {ActivityName}", activity.Name);
        return activity;
    }

    public async Task<Activity?> UpdateActivityAsync(int id, Activity activity)
    {
        var existingActivity = await _context.Activities.FindAsync(id);
        if (existingActivity == null)
            return null;

        existingActivity.Name = activity.Name;
        existingActivity.Description = activity.Description;
        existingActivity.IconUrl = activity.IconUrl;
        existingActivity.IsEnabled = activity.IsEnabled;
        existingActivity.SortOrder = activity.SortOrder;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated activity: {ActivityName}", existingActivity.Name);
        return existingActivity;
    }

    public async Task<bool> DeleteActivityAsync(int id)
    {
        var activity = await _context.Activities.FindAsync(id);
        if (activity == null)
            return false;

        _context.Activities.Remove(activity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted activity: {ActivityName}", activity.Name);
        return true;
    }

    public async Task<bool> ExecuteActivityAsync(int activityId, CancellationToken cancellationToken = default)
    {
        var activity = await GetActivityAsync(activityId);
        if (activity == null || !activity.IsEnabled)
        {
            _logger.LogWarning("Activity not found or disabled: {ActivityId}", activityId);
            return false;
        }

        _logger.LogInformation("Executing activity: {ActivityName}", activity.Name);
        
        // Notify clients that activity has started
        await _notificationService.NotifyActivityStartedAsync(activity.Id, activity.Name);

        try
        {
            var steps = activity.Steps.OrderBy(s => s.StepOrder).ToList();
            var executedSteps = 0;

            foreach (var step in steps)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Delay before step if specified
                if (step.DelayBeforeMs > 0)
                {
                    _logger.LogDebug("Waiting {DelayMs}ms before step {StepOrder}", step.DelayBeforeMs, step.StepOrder);
                    await Task.Delay(step.DelayBeforeMs, cancellationToken);
                }

                // Execute the command
                var success = await _deviceService.SendCommandAsync(
                    step.DeviceCommand.DeviceId, 
                    step.DeviceCommand.Name, 
                    cancellationToken);

                // Notify clients of step execution
                await _notificationService.NotifyActivityStepExecutedAsync(
                    activity.Id, activity.Name, step.StepOrder, 
                    $"{step.DeviceCommand.Device.Name} - {step.DeviceCommand.Name}", success);

                if (!success && step.IsRequired)
                {
                    _logger.LogError("Required step failed in activity {ActivityName}: {DeviceName} - {CommandName}", 
                                   activity.Name, step.DeviceCommand.Device.Name, step.DeviceCommand.Name);
                    
                    // Notify clients of activity failure
                    await _notificationService.NotifyActivityCompletedAsync(activity.Id, activity.Name, false);
                    return false;
                }

                if (!success)
                {
                    _logger.LogWarning("Optional step failed in activity {ActivityName}: {DeviceName} - {CommandName}", 
                                     activity.Name, step.DeviceCommand.Device.Name, step.DeviceCommand.Name);
                }
                else
                {
                    executedSteps++;
                    _logger.LogDebug("Executed step {StepOrder}: {DeviceName} - {CommandName}", 
                                   step.StepOrder, step.DeviceCommand.Device.Name, step.DeviceCommand.Name);
                }

                // Delay after step if specified
                if (step.DelayAfterMs > 0)
                {
                    _logger.LogDebug("Waiting {DelayMs}ms after step {StepOrder}", step.DelayAfterMs, step.StepOrder);
                    await Task.Delay(step.DelayAfterMs, cancellationToken);
                }
            }

            // Update last used timestamp
            activity.LastUsed = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Activity {ActivityName} completed successfully. Executed {ExecutedSteps}/{TotalSteps} steps", 
                                 activity.Name, executedSteps, steps.Count);
            
            // Notify clients of successful activity completion
            await _notificationService.NotifyActivityCompletedAsync(activity.Id, activity.Name, true);
            return true;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Activity execution cancelled: {ActivityName}", activity.Name);
            await _notificationService.NotifyActivityCompletedAsync(activity.Id, activity.Name, false);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to execute activity: {ActivityName}", activity.Name);
            await _notificationService.NotifyActivityCompletedAsync(activity.Id, activity.Name, false);
            return false;
        }
    }

    public async Task<bool> StopActivityAsync(int activityId, CancellationToken cancellationToken = default)
    {
        // For now, this is a placeholder - in a real implementation you'd track running activities
        // and provide a way to cancel their execution
        _logger.LogInformation("Stop activity requested for activity ID: {ActivityId}", activityId);
        await Task.CompletedTask;
        return true;
    }

    public async Task<Activity?> AddDeviceToActivityAsync(int activityId, int deviceId, bool isPrimary = false)
    {
        var activity = await _context.Activities.FindAsync(activityId);
        var device = await _context.Devices.FindAsync(deviceId);

        if (activity == null || device == null)
            return null;

        // Check if device is already in activity
        var existingAssociation = await _context.ActivityDevices
            .FirstOrDefaultAsync(ad => ad.ActivityId == activityId && ad.DeviceId == deviceId);

        if (existingAssociation != null)
        {
            existingAssociation.IsPrimaryDevice = isPrimary;
        }
        else
        {
            var activityDevice = new ActivityDevice
            {
                ActivityId = activityId,
                DeviceId = deviceId,
                IsPrimaryDevice = isPrimary
            };

            _context.ActivityDevices.Add(activityDevice);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Added device {DeviceName} to activity {ActivityName}", device.Name, activity.Name);
        return await GetActivityAsync(activityId);
    }

    public async Task<bool> RemoveDeviceFromActivityAsync(int activityId, int deviceId)
    {
        var activityDevice = await _context.ActivityDevices
            .FirstOrDefaultAsync(ad => ad.ActivityId == activityId && ad.DeviceId == deviceId);

        if (activityDevice == null)
            return false;

        _context.ActivityDevices.Remove(activityDevice);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Removed device {DeviceId} from activity {ActivityId}", deviceId, activityId);
        return true;
    }

    public async Task<ActivityStep?> AddStepToActivityAsync(int activityId, int deviceCommandId, int stepOrder, int delayBeforeMs = 0, int delayAfterMs = 0)
    {
        var activity = await _context.Activities.FindAsync(activityId);
        var deviceCommand = await _context.DeviceCommands.FindAsync(deviceCommandId);

        if (activity == null || deviceCommand == null)
            return null;

        var step = new ActivityStep
        {
            ActivityId = activityId,
            DeviceCommandId = deviceCommandId,
            StepOrder = stepOrder,
            DelayBeforeMs = delayBeforeMs,
            DelayAfterMs = delayAfterMs,
            IsRequired = true
        };

        _context.ActivitySteps.Add(step);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Added step to activity {ActivityName}: {StepOrder} - {CommandName}", 
                             activity.Name, stepOrder, deviceCommand.Name);
        return step;
    }

    public async Task<bool> RemoveStepFromActivityAsync(int activityId, int stepId)
    {
        var step = await _context.ActivitySteps
            .FirstOrDefaultAsync(s => s.Id == stepId && s.ActivityId == activityId);

        if (step == null)
            return false;

        _context.ActivitySteps.Remove(step);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Removed step {StepId} from activity {ActivityId}", stepId, activityId);
        return true;
    }

    public async Task<bool> ReorderActivityStepsAsync(int activityId, int[] stepIds)
    {
        var steps = await _context.ActivitySteps
            .Where(s => s.ActivityId == activityId && stepIds.Contains(s.Id))
            .ToListAsync();

        if (steps.Count != stepIds.Length)
            return false;

        for (int i = 0; i < stepIds.Length; i++)
        {
            var step = steps.First(s => s.Id == stepIds[i]);
            step.StepOrder = i + 1;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Reordered steps for activity {ActivityId}", activityId);
        return true;
    }
}