using Microsoft.AspNetCore.Mvc;
using ZapperHub.Models;
using ZapperHub.Services;

namespace ZapperHub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ActivitiesController : ControllerBase
{
    private readonly IActivityService _activityService;
    private readonly ILogger<ActivitiesController> _logger;

    public ActivitiesController(IActivityService activityService, ILogger<ActivitiesController> logger)
    {
        _activityService = activityService;
        _logger = logger;
    }

    /// <summary>
    /// Get all activities
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Activity>>> GetActivities()
    {
        var activities = await _activityService.GetAllActivitiesAsync();
        return Ok(activities);
    }

    /// <summary>
    /// Get a specific activity by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Activity>> GetActivity(int id)
    {
        var activity = await _activityService.GetActivityAsync(id);
        if (activity == null)
        {
            return NotFound();
        }

        return Ok(activity);
    }

    /// <summary>
    /// Create a new activity
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Activity>> CreateActivity(Activity activity)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdActivity = await _activityService.CreateActivityAsync(activity);
        return CreatedAtAction(nameof(GetActivity), new { id = createdActivity.Id }, createdActivity);
    }

    /// <summary>
    /// Update an existing activity
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateActivity(int id, Activity activity)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedActivity = await _activityService.UpdateActivityAsync(id, activity);
        if (updatedActivity == null)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Delete an activity
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteActivity(int id)
    {
        var success = await _activityService.DeleteActivityAsync(id);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Execute an activity
    /// </summary>
    [HttpPost("{id}/execute")]
    public async Task<IActionResult> ExecuteActivity(int id, CancellationToken cancellationToken)
    {
        var success = await _activityService.ExecuteActivityAsync(id, cancellationToken);
        if (!success)
        {
            return BadRequest($"Failed to execute activity {id}");
        }

        return Ok(new { message = "Activity executed successfully" });
    }

    /// <summary>
    /// Stop an activity execution
    /// </summary>
    [HttpPost("{id}/stop")]
    public async Task<IActionResult> StopActivity(int id, CancellationToken cancellationToken)
    {
        var success = await _activityService.StopActivityAsync(id, cancellationToken);
        return Ok(new { message = success ? "Activity stopped" : "Activity was not running" });
    }

    /// <summary>
    /// Add a device to an activity
    /// </summary>
    [HttpPost("{id}/devices")]
    public async Task<ActionResult<Activity>> AddDeviceToActivity(int id, [FromBody] AddDeviceToActivityRequest request)
    {
        var activity = await _activityService.AddDeviceToActivityAsync(id, request.DeviceId, request.IsPrimary);
        if (activity == null)
        {
            return BadRequest("Failed to add device to activity");
        }

        return Ok(activity);
    }

    /// <summary>
    /// Remove a device from an activity
    /// </summary>
    [HttpDelete("{id}/devices/{deviceId}")]
    public async Task<IActionResult> RemoveDeviceFromActivity(int id, int deviceId)
    {
        var success = await _activityService.RemoveDeviceFromActivityAsync(id, deviceId);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Add a step to an activity
    /// </summary>
    [HttpPost("{id}/steps")]
    public async Task<ActionResult<ActivityStep>> AddStepToActivity(int id, [FromBody] AddStepToActivityRequest request)
    {
        var step = await _activityService.AddStepToActivityAsync(
            id, 
            request.DeviceCommandId, 
            request.StepOrder, 
            request.DelayBeforeMs, 
            request.DelayAfterMs);

        if (step == null)
        {
            return BadRequest("Failed to add step to activity");
        }

        return Ok(step);
    }

    /// <summary>
    /// Remove a step from an activity
    /// </summary>
    [HttpDelete("{id}/steps/{stepId}")]
    public async Task<IActionResult> RemoveStepFromActivity(int id, int stepId)
    {
        var success = await _activityService.RemoveStepFromActivityAsync(id, stepId);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Reorder activity steps
    /// </summary>
    [HttpPut("{id}/steps/reorder")]
    public async Task<IActionResult> ReorderActivitySteps(int id, [FromBody] ReorderStepsRequest request)
    {
        var success = await _activityService.ReorderActivityStepsAsync(id, request.StepIds);
        if (!success)
        {
            return BadRequest("Failed to reorder steps");
        }

        return NoContent();
    }
}

public class AddDeviceToActivityRequest
{
    public int DeviceId { get; set; }
    public bool IsPrimary { get; set; } = false;
}

public class AddStepToActivityRequest
{
    public int DeviceCommandId { get; set; }
    public int StepOrder { get; set; }
    public int DelayBeforeMs { get; set; } = 0;
    public int DelayAfterMs { get; set; } = 0;
}

public class ReorderStepsRequest
{
    public int[] StepIds { get; set; } = Array.Empty<int>();
}