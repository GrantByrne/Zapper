using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zapper.Data;
using Zapper.Core.Models;
using Zapper.Contracts;
using Zapper.Contracts.Activities;

namespace Zapper.Services;

public class ActivityService(
    ZapperContext context,
    IDeviceService deviceService,
    INotificationService notificationService,
    ILogger<ActivityService> logger) : IActivityService
{
    public async Task<IEnumerable<Activity>> GetAllActivities()
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

    public async Task<Activity?> GetActivity(int id)
    {
        return await context.Activities
            .Include(a => a.ActivityDevices)
                .ThenInclude(ad => ad.Device)
            .Include(a => a.Steps.OrderBy(s => s.StepOrder))
                .ThenInclude(s => s.DeviceCommand)
                    .ThenInclude(dc => dc.Device)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Activity> CreateActivity(Activity activity)
    {
        activity.CreatedAt = DateTime.UtcNow;
        activity.LastUsed = DateTime.UtcNow;

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

    public async Task<Activity?> UpdateActivity(int id, Activity activity)
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

    public async Task<bool> DeleteActivity(int id)
    {
        var activity = await context.Activities.FindAsync(id);
        if (activity == null)
            return false;

        context.Activities.Remove(activity);
        await context.SaveChangesAsync();

        logger.LogInformation("Deleted activity: {ActivityName}", activity.Name);
        return true;
    }

    public async Task<bool> ExecuteActivity(int activityId, CancellationToken cancellationToken = default)
    {
        var activity = await GetActivity(activityId);
        if (activity == null || !activity.IsEnabled)
        {
            logger.LogWarning("Activity not found or disabled: {ActivityId}", activityId);
            return false;
        }

        logger.LogInformation("Executing activity: {ActivityName}", activity.Name);

        await notificationService.NotifyActivityStarted(activity.Id, activity.Name);

        try
        {
            var steps = activity.Steps.OrderBy(s => s.StepOrder).ToList();
            var executedSteps = 0;

            foreach (var step in steps)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (step.DelayBeforeMs > 0)
                {
                    logger.LogDebug("Waiting {DelayMs}ms before step {StepOrder}", step.DelayBeforeMs, step.StepOrder);
                    await Task.Delay(step.DelayBeforeMs, cancellationToken);
                }

                var success = await deviceService.SendCommand(
                    step.DeviceCommand.DeviceId,
                    step.DeviceCommand.Name,
                    cancellationToken);

                await notificationService.NotifyActivityStepExecuted(
                    activity.Id, activity.Name, step.StepOrder,
                    $"{step.DeviceCommand.Device.Name} - {step.DeviceCommand.Name}", success);

                if (!success && step.IsRequired)
                {
                    logger.LogError("Required step failed in activity {ActivityName}: {DeviceName} - {CommandName}",
                                   activity.Name, step.DeviceCommand.Device.Name, step.DeviceCommand.Name);

                    await notificationService.NotifyActivityCompleted(activity.Id, activity.Name, false);
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

                if (step.DelayAfterMs > 0)
                {
                    logger.LogDebug("Waiting {DelayMs}ms after step {StepOrder}", step.DelayAfterMs, step.StepOrder);
                    await Task.Delay(step.DelayAfterMs, cancellationToken);
                }
            }

            activity.LastUsed = DateTime.UtcNow;
            await context.SaveChangesAsync();

            logger.LogInformation("Activity {ActivityName} completed successfully. Executed {ExecutedSteps}/{TotalSteps} steps",
                                 activity.Name, executedSteps, steps.Count);

            await notificationService.NotifyActivityCompleted(activity.Id, activity.Name, true);
            return true;
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Activity execution cancelled: {ActivityName}", activity.Name);
            await notificationService.NotifyActivityCompleted(activity.Id, activity.Name, false);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute activity: {ActivityName}", activity.Name);
            await notificationService.NotifyActivityCompleted(activity.Id, activity.Name, false);
            return false;
        }
    }

    public async Task<bool> StopActivity(int activityId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Stop activity requested for activity ID: {ActivityId}", activityId);
        await Task.CompletedTask;
        return true;
    }

    public async Task<Activity?> AddDeviceToActivity(int activityId, int deviceId, bool isPrimary = false)
    {
        var activity = await context.Activities.FindAsync(activityId);
        var device = await context.Devices.FindAsync(deviceId);

        if (activity == null || device == null)
            return null;

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
        return await GetActivity(activityId);
    }

    public async Task<bool> RemoveDeviceFromActivity(int activityId, int deviceId)
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

    public async Task<ActivityStep?> AddStepToActivity(int activityId, int deviceCommandId, int stepOrder, int delayBeforeMs = 0, int delayAfterMs = 0)
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

    public async Task<bool> RemoveStepFromActivity(int activityId, int stepId)
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

    public async Task<bool> ReorderActivitySteps(int activityId, int[] stepIds)
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

    public async Task<Contracts.Activities.ActivityDto?> GetActivityDto(int id)
    {
        var activity = await GetActivity(id);
        if (activity == null)
            return null;

        return new Contracts.Activities.ActivityDto
        {
            Id = activity.Id,
            Name = activity.Name,
            Description = activity.Description,
            Type = activity.Type.ToString(),
            SortOrder = activity.SortOrder,
            IsEnabled = activity.IsEnabled,
            LastUsed = activity.LastUsed,
            CreatedAt = activity.CreatedAt,
            Steps = activity.Steps?.Select(s => new Contracts.Activities.ActivityStepDto
            {
                Id = s.Id,
                DeviceId = s.DeviceCommand.DeviceId,
                DeviceName = s.DeviceCommand.Device?.Name ?? "",
                Command = s.DeviceCommand.Name,
                SortOrder = s.StepOrder,
                DelayMs = s.DelayBeforeMs
            }).ToList() ?? new()
        };
    }

    public async Task<Contracts.Activities.ActivityDto> CreateActivity(CreateActivityRequest request)
    {
        var activity = new Activity
        {
            Name = request.Name,
            Description = request.Description,
            Type = Enum.Parse<ActivityType>(request.Type),
            IsEnabled = request.IsEnabled
        };

        var createdActivity = await CreateActivity(activity);

        // Add steps if provided
        if (request.Steps?.Any() == true)
        {
            foreach (var stepRequest in request.Steps.OrderBy(s => s.SortOrder))
            {
                await AddStepToActivity(
                    createdActivity.Id,
                    stepRequest.DeviceId,
                    stepRequest.SortOrder,
                    stepRequest.DelayMs,
                    stepRequest.DelayMs);
            }
        }

        return await GetActivityDto(createdActivity.Id) ?? new Contracts.Activities.ActivityDto();
    }

    public async Task<Contracts.Activities.ActivityDto?> UpdateActivity(UpdateActivityRequest request)
    {
        var activity = await GetActivity(request.Id);
        if (activity == null)
            return null;

        activity.Name = request.Name;
        activity.Description = request.Description;
        activity.Type = Enum.Parse<ActivityType>(request.Type);
        activity.IsEnabled = request.IsEnabled;

        var updated = await UpdateActivity(request.Id, activity);
        if (updated == null)
            return null;

        return await GetActivityDto(updated.Id) ?? new Contracts.Activities.ActivityDto();
    }
}