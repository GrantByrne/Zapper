using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZapperHub.Data;
using ZapperHub.Models;

namespace ZapperHub.Controllers;

[ApiController]
[Route("api/devices/{deviceId}/commands")]
public class DeviceCommandsController : ControllerBase
{
    private readonly ZapperContext _context;
    private readonly ILogger<DeviceCommandsController> _logger;

    public DeviceCommandsController(ZapperContext context, ILogger<DeviceCommandsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all commands for a device
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeviceCommand>>> GetDeviceCommands(int deviceId)
    {
        var device = await _context.Devices.FindAsync(deviceId);
        if (device == null)
        {
            return NotFound();
        }

        var commands = await _context.DeviceCommands
            .Where(c => c.DeviceId == deviceId)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(commands);
    }

    /// <summary>
    /// Get a specific command for a device
    /// </summary>
    [HttpGet("{commandId}")]
    public async Task<ActionResult<DeviceCommand>> GetDeviceCommand(int deviceId, int commandId)
    {
        var command = await _context.DeviceCommands
            .FirstOrDefaultAsync(c => c.Id == commandId && c.DeviceId == deviceId);

        if (command == null)
        {
            return NotFound();
        }

        return Ok(command);
    }

    /// <summary>
    /// Create a new command for a device
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<DeviceCommand>> CreateDeviceCommand(int deviceId, DeviceCommand command)
    {
        var device = await _context.Devices.FindAsync(deviceId);
        if (device == null)
        {
            return NotFound();
        }

        command.DeviceId = deviceId;

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        _context.DeviceCommands.Add(command);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created command {CommandName} for device {DeviceId}", command.Name, deviceId);

        return CreatedAtAction(nameof(GetDeviceCommand), 
            new { deviceId, commandId = command.Id }, command);
    }

    /// <summary>
    /// Update a command for a device
    /// </summary>
    [HttpPut("{commandId}")]
    public async Task<IActionResult> UpdateDeviceCommand(int deviceId, int commandId, DeviceCommand command)
    {
        var existingCommand = await _context.DeviceCommands
            .FirstOrDefaultAsync(c => c.Id == commandId && c.DeviceId == deviceId);

        if (existingCommand == null)
        {
            return NotFound();
        }

        existingCommand.Name = command.Name;
        existingCommand.Type = command.Type;
        existingCommand.IrCode = command.IrCode;
        existingCommand.NetworkPayload = command.NetworkPayload;
        existingCommand.HttpMethod = command.HttpMethod;
        existingCommand.HttpEndpoint = command.HttpEndpoint;
        existingCommand.DelayMs = command.DelayMs;
        existingCommand.IsRepeatable = command.IsRepeatable;

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated command {CommandName} for device {DeviceId}", command.Name, deviceId);

        return NoContent();
    }

    /// <summary>
    /// Delete a command for a device
    /// </summary>
    [HttpDelete("{commandId}")]
    public async Task<IActionResult> DeleteDeviceCommand(int deviceId, int commandId)
    {
        var command = await _context.DeviceCommands
            .FirstOrDefaultAsync(c => c.Id == commandId && c.DeviceId == deviceId);

        if (command == null)
        {
            return NotFound();
        }

        _context.DeviceCommands.Remove(command);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted command {CommandName} for device {DeviceId}", command.Name, deviceId);

        return NoContent();
    }
}