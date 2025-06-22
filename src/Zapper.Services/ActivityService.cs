using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zapper.Data;
using Zapper.Core.Models;

namespace Zapper.Services;

public class ActivityService(
    ZapperContext context,
    IDeviceService deviceService,
    INotificationService notificationService,
    ILogger<ActivityService> logger) : IActivityService
{
    public async Task<IEnumerable<Activity>> GetAllActivitiesAsync()
    {
        return await context.Activities
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
        return await context.Activities
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
            var maxSortOrder = await context.Activities.MaxAsync(a => (int?)a.SortOrder) ?? 0;
            activity.SortOrder = maxSortOrder + 1;
        }

        context.Activities.Add(activity);
        await context.SaveChangesAsync();

        logger.LogInformation("Created activity: {ActivityName}", activity.Name);
        return activity;
    }

    public async Task<Activity?> UpdateActivityAsync(int id, Activity activity)
    {
        var existingActivity = await context.Activities.FindAsync(id);
        if (existingActivity == null)
            return null;

        existingActivity.Name = activity.Name;
        existingActivity.Description = activity.Description;
        existingActivity.IconUrl = activity.IconUrl;
        existingActivity.IsEnabled = activity.IsEnabled;
        existingActivity.SortOrder = activity.SortOrder;

        await context.SaveChangesAsync();

        logger.LogInformation("Updated activity: {ActivityName}", existingActivity.Name);
        return existingActivity;
    }

    public async Task<bool> DeleteActivityAsync(int id)
    {
        var activity = await context.Activities.FindAsync(id);
        if (activity == null)
            return false;

        context.Activities.Remove(activity);
        await context.SaveChangesAsync();

        logger.LogInformation("Deleted activity: {ActivityName}", activity.Name);
        return true;
    }

    public async Task<bool> ExecuteActivityAsync(int activityId, CancellationToken cancellationToken = default)
    {
        var activity = await GetActivityAsync(activityId);
        if (activity == null || !activity.IsEnabled)
        {
            logger.LogWarning("Activity not found or disabled: {ActivityId}", activityId);
            return false;
        }

        logger.LogInformation("Executing activity: {ActivityName}", activity.Name);
        
        // Notify clients that activity has started
        await notificationService.NotifyActivityStartedAsync(activity.Id, activity.Name);

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
                    logger.LogDebug("Waiting {DelayMs}ms before step {StepOrder}", step.DelayBeforeMs, step.StepOrder);
                    await Task.Delay(step.DelayBeforeMs, cancellationToken);
                }

                // Execute the command
                var success = await deviceService.SendCommandAsync(
                    step.DeviceCommand.DeviceId, 
                    step.DeviceCommand.Name, 
                    cancellationToken);

                // Notify clients of step execution
                await notificationService.NotifyActivityStepExecutedAsync(
                    activity.Id, activity.Name, step.StepOrder, 
                    $"{step.DeviceCommand.Device.Name} - {step.DeviceCommand.Name}", success);

                if (!success && step.IsRequired)
                {
                    logger.LogError("Required step failed in activity {ActivityName}: {DeviceName} - {CommandName}", 
                                   activity.Name, step.DeviceCommand.Device.Name, step.DeviceCommand.Name);
                    
                    // Notify clients of activity failure
                    await notificationService.NotifyActivityCompletedAsync(activity.Id, activity.Name, false);
                    return false;
                }

                if (!success)
                {
                    logger.LogWarning("Optional step failed in activity {ActivityName}: {DeviceName} - {CommandName}", 
                                     activity.Name, step.DeviceCommand.Device.Name, step.DeviceCommand.Name);
                }
                else
                {
                    executedSteps++;
                    logger.LogDebug("Executed step {StepOrder}: {DeviceName} - {CommandName}", 
                                   step.StepOrder, step.DeviceCommand.Device.Name, step.DeviceCommand.Name);
                }

                // Delay after step if specified
                if (step.DelayAfterMs > 0)
                {
                    logger.LogDebug("Waiting {DelayMs}ms after step {StepOrder}", step.DelayAfterMs, step.StepOrder);
                    await Task.Delay(step.DelayAfterMs, cancellationToken);
                }
            }

            // Update last used timestamp
            activity.LastUsed = DateTime.UtcNow;
            await context.SaveChangesAsync();

            logger.LogInformation("Activity {ActivityName} completed successfully. Executed {ExecutedSteps}/{TotalSteps} steps", 
                                 activity.Name, executedSteps, steps.Count);
            
            // Notify clients of successful activity completion
            await notificationService.NotifyActivityCompletedAsync(activity.Id, activity.Name, true);
            return true;
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Activity execution cancelled: {ActivityName}", activity.Name);
            await notificationService.NotifyActivityCompletedAsync(activity.Id, activity.Name, false);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute activity: {ActivityName}", activity.Name);
            await notificationService.NotifyActivityCompletedAsync(activity.Id, activity.Name, false);
            return false;
        }
    }

    public async Task<bool> StopActivityAsync(int activityId, CancellationToken cancellationToken = default)
    {
        // For now, this is a placeholder - in a real implementation you'd track running activities
        // and provide a way to cancel their execution
        logger.LogInformation("Stop activity requested for activity ID: {ActivityId}", activityId);
        await Task.CompletedTask;
        return true;
    }

    public async Task<Activity?> AddDeviceToActivityAsync(int activityId, int deviceId, bool isPrimary = false)
    {
        var activity = await context.Activities.FindAsync(activityId);
        var device = await context.Devices.FindAsync(deviceId);

        if (activity == null || device == null)
            return null;

        // Check if device is already in activity
        var existingAssociation = await context.ActivityDevices
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

            context.ActivityDevices.Add(activityDevice);
        }

        await context.SaveChangesAsync();

        logger.LogInformation("Added device {DeviceName} to activity {ActivityName}", device.Name, activity.Name);
        return await GetActivityAsync(activityId);
    }

    public async Task<bool> RemoveDeviceFromActivityAsync(int activityId, int deviceId)
    {
        var activityDevice = await context.ActivityDevices
            .FirstOrDefaultAsync(ad => ad.ActivityId == activityId && ad.DeviceId == deviceId);

        if (activityDevice == null)
            return false;

        context.ActivityDevices.Remove(activityDevice);
        await context.SaveChangesAsync();

        logger.LogInformation("Removed device {DeviceId} from activity {ActivityId}", deviceId, activityId);
        return true;
    }

    public async Task<ActivityStep?> AddStepToActivityAsync(int activityId, int deviceCommandId, int stepOrder, int delayBeforeMs = 0, int delayAfterMs = 0)
    {
        var activity = await context.Activities.FindAsync(activityId);
        var deviceCommand = await context.DeviceCommands.FindAsync(deviceCommandId);

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

        context.ActivitySteps.Add(step);
        await context.SaveChangesAsync();

        logger.LogInformation("Added step to activity {ActivityName}: {StepOrder} - {CommandName}", 
                             activity.Name, stepOrder, deviceCommand.Name);
        return step;
    }

    public async Task<bool> RemoveStepFromActivityAsync(int activityId, int stepId)
    {
        var step = await context.ActivitySteps
            .FirstOrDefaultAsync(s => s.Id == stepId && s.ActivityId == activityId);

        if (step == null)
            return false;

        context.ActivitySteps.Remove(step);
        await context.SaveChangesAsync();

        logger.LogInformation("Removed step {StepId} from activity {ActivityId}", stepId, activityId);
        return true;
    }

    public async Task<bool> ReorderActivityStepsAsync(int activityId, int[] stepIds)
    {
        var steps = await context.ActivitySteps
            .Where(s => s.ActivityId == activityId && stepIds.Contains(s.Id))
            .ToListAsync();

        if (steps.Count != stepIds.Length)
            return false;

        for (int i = 0; i < stepIds.Length; i++)
        {
            var step = steps.First(s => s.Id == stepIds[i]);
            step.StepOrder = i + 1;
        }

        await context.SaveChangesAsync();

        logger.LogInformation("Reordered steps for activity {ActivityId}", activityId);
        return true;
    }
}