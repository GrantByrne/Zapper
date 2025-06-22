using Microsoft.AspNetCore.Mvc;
using ZapperHub.Models;
using ZapperHub.Services;

namespace ZapperHub.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly ILogger<DevicesController> _logger;

    public DevicesController(IDeviceService deviceService, ILogger<DevicesController> logger)
    {
        _deviceService = deviceService;
        _logger = logger;
    }

    /// <summary>
    /// Get all devices
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Device>>> GetDevices()
    {
        var devices = await _deviceService.GetAllDevicesAsync();
        return Ok(devices);
    }

    /// <summary>
    /// Get a specific device by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Device>> GetDevice(int id)
    {
        var device = await _deviceService.GetDeviceAsync(id);
        if (device == null)
        {
            return NotFound();
        }

        return Ok(device);
    }

    /// <summary>
    /// Create a new device
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Device>> CreateDevice(Device device)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdDevice = await _deviceService.CreateDeviceAsync(device);
        return CreatedAtAction(nameof(GetDevice), new { id = createdDevice.Id }, createdDevice);
    }

    /// <summary>
    /// Update an existing device
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDevice(int id, Device device)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updatedDevice = await _deviceService.UpdateDeviceAsync(id, device);
        if (updatedDevice == null)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Delete a device
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDevice(int id)
    {
        var success = await _deviceService.DeleteDeviceAsync(id);
        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Send a command to a device
    /// </summary>
    [HttpPost("{id}/commands/{commandName}")]
    public async Task<IActionResult> SendCommand(int id, string commandName, CancellationToken cancellationToken)
    {
        var success = await _deviceService.SendCommandAsync(id, commandName, cancellationToken);
        if (!success)
        {
            return BadRequest($"Failed to send command '{commandName}' to device {id}");
        }

        return Ok(new { message = $"Command '{commandName}' sent successfully" });
    }

    /// <summary>
    /// Test device connection
    /// </summary>
    [HttpPost("{id}/test")]
    public async Task<IActionResult> TestConnection(int id)
    {
        var isOnline = await _deviceService.TestDeviceConnectionAsync(id);
        return Ok(new { deviceId = id, isOnline });
    }

    /// <summary>
    /// Discover devices on the network
    /// </summary>
    [HttpPost("discover")]
    public async Task<ActionResult<IEnumerable<Device>>> DiscoverDevices([FromBody] DiscoverDevicesRequest request, CancellationToken cancellationToken)
    {
        var devices = await _deviceService.DiscoverDevicesAsync(request.DeviceType, cancellationToken);
        return Ok(devices);
    }
}

public class DiscoverDevicesRequest
{
    public string DeviceType { get; set; } = string.Empty;
}