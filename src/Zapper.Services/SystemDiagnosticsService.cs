using System.Diagnostics;
using System.Runtime.InteropServices;
using HidSharp;
using Microsoft.Extensions.Logging;
using Zapper.Core.Models;

namespace Zapper.Services;

public class SystemDiagnosticsService(ILogger<SystemDiagnosticsService> logger) : ISystemDiagnosticsService
{
    public async Task<UsbPermissionStatus> CheckUsbPermissionsAsync()
    {
        var status = new UsbPermissionStatus
        {
            IsRunningAsRoot = Environment.UserName == "root",
            Platform = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "Linux" :
                      RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Windows" : "Other",
            Issues = new List<UsbPermissionIssue>()
        };

        if (status.Platform != "Linux")
        {
            status.HasPermissionIssues = false;
            return status;
        }

        try
        {
            var deviceList = DeviceList.Local;
            var hidDevices = deviceList.GetHidDevices();

            foreach (var device in hidDevices)
            {
                try
                {
                    using var stream = device.Open();
                }
                catch (UnauthorizedAccessException ex)
                {
                    status.HasPermissionIssues = true;
                    status.Issues.Add(new UsbPermissionIssue
                    {
                        DevicePath = device.DevicePath,
                        VendorId = device.VendorID.ToString("X4"),
                        ProductId = device.ProductID.ToString("X4"),
                        ProductName = device.GetProductName() ?? "Unknown Device",
                        ErrorMessage = ex.Message
                    });
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Error checking device {DevicePath}", device.DevicePath);
                }
            }

            if (!status.HasPermissionIssues)
            {
                var groups = await GetUserGroupsAsync();
                status.UserInPlugdevGroup = groups.Contains("plugdev");
                status.UdevRulesExist = File.Exists("/etc/udev/rules.d/99-zapper-usb.rules");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check USB permissions");
            status.ErrorMessage = ex.Message;
        }

        return status;
    }

    public async Task<UsbPermissionFixResult> FixUsbPermissionsAsync(string password)
    {
        var result = new UsbPermissionFixResult();

        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            result.Success = false;
            result.Message = "USB permission fix is only available on Linux";
            return result;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            result.Success = false;
            result.Message = "Password is required to fix USB permissions";
            return result;
        }

        try
        {
            result.Steps.Add("Creating udev rules file");
            var udevRules = @"# Zapper USB Remote Control Access Rules
# Grant access to all USB HID devices for the plugdev group
SUBSYSTEM==""hidraw"", MODE=""0666"", GROUP=""plugdev""
SUBSYSTEM==""usb"", MODE=""0666"", GROUP=""plugdev""

# Specific rules for known USB remote vendors
# Alienware Hub Controller
SUBSYSTEM==""hidraw"", ATTRS{idVendor}==""187c"", MODE=""0666"", GROUP=""plugdev""
SUBSYSTEM==""usb"", ATTRS{idVendor}==""187c"", MODE=""0666"", GROUP=""plugdev""
";

            var tempFile = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempFile, udevRules);

            result.Steps.Add("Installing udev rules");
            var installResult = await RunCommandWithPasswordAsync($"cp {tempFile} /etc/udev/rules.d/99-zapper-usb.rules", password);
            if (!installResult.Success)
            {
                result.Success = false;
                result.Message = $"Failed to install udev rules: {installResult.Error}";
                return result;
            }

            File.Delete(tempFile);

            result.Steps.Add("Checking if plugdev group exists");
            var groupCheckResult = await RunCommandAsync("getent group plugdev");
            if (!groupCheckResult.Success)
            {
                result.Steps.Add("Creating plugdev group");
                var createGroupResult = await RunCommandWithPasswordAsync("groupadd plugdev", password);
                if (!createGroupResult.Success)
                {
                    result.Success = false;
                    result.Message = $"Failed to create plugdev group: {createGroupResult.Error}";
                    return result;
                }
            }

            result.Steps.Add("Adding user to plugdev group");
            var userName = Environment.UserName;
            var groupResult = await RunCommandWithPasswordAsync($"usermod -a -G plugdev {userName}", password);
            if (!groupResult.Success)
            {
                result.Success = false;
                result.Message = $"Failed to add user to plugdev group: {groupResult.Error}";
                return result;
            }

            result.Steps.Add("Reloading udev rules");
            var reloadResult = await RunCommandWithPasswordAsync("udevadm control --reload-rules", password);
            if (!reloadResult.Success)
            {
                result.Success = false;
                result.Message = $"Failed to reload udev rules: {reloadResult.Error}";
                return result;
            }

            result.Steps.Add("Triggering udev");
            var triggerResult = await RunCommandWithPasswordAsync("udevadm trigger", password);
            if (!triggerResult.Success)
            {
                result.Success = false;
                result.Message = $"Failed to trigger udev: {triggerResult.Error}";
                return result;
            }

            result.Success = true;
            result.Message = "USB permissions fixed successfully. You may need to reconnect your USB devices or restart the application.";
            result.RequiresRestart = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fix USB permissions");
            result.Success = false;
            result.Message = $"Error: {ex.Message}";
        }

        return result;
    }

    public async Task<SystemInfo> GetSystemInfoAsync()
    {
        var info = new SystemInfo
        {
            Platform = RuntimeInformation.OSDescription,
            IsRaspberryPi = await IsRaspberryPiAsync(),
            HasGpioSupport = RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && await IsRaspberryPiAsync()
        };

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var groups = await GetUserGroupsAsync();
            info.UserGroups = groups;

            if (!groups.Contains("gpio") && info.IsRaspberryPi)
            {
                info.GpioWarnings.Add("User is not in 'gpio' group");
            }

            if (!groups.Contains("plugdev"))
            {
                info.GpioWarnings.Add("User is not in 'plugdev' group (required for USB devices)");
            }
        }

        return info;
    }

    private async Task<bool> IsRaspberryPiAsync()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            return false;

        try
        {
            if (File.Exists("/proc/device-tree/model"))
            {
                var model = await File.ReadAllTextAsync("/proc/device-tree/model");
                return model.Contains("Raspberry Pi", StringComparison.OrdinalIgnoreCase);
            }
        }
        catch
        {
        }

        return false;
    }

    private async Task<List<string>> GetUserGroupsAsync()
    {
        try
        {
            var result = await RunCommandAsync("groups");
            if (result.Success && !string.IsNullOrEmpty(result.Output))
            {
                return result.Output.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            }
        }
        catch
        {
        }

        return new List<string>();
    }

    private async Task<CommandResult> RunCommandAsync(string command)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                return new CommandResult { Success = false, Error = "Failed to start process" };
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            return new CommandResult
            {
                Success = process.ExitCode == 0,
                Output = output.Trim(),
                Error = error.Trim()
            };
        }
        catch (Exception ex)
        {
            return new CommandResult { Success = false, Error = ex.Message };
        }
    }

    private async Task<CommandResult> RunCommandWithPasswordAsync(string command, string password)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = "-c \"sudo -S " + command + "\"",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                return new CommandResult { Success = false, Error = "Failed to start process" };
            }

            await process.StandardInput.WriteLineAsync(password);
            await process.StandardInput.FlushAsync();
            process.StandardInput.Close();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            return new CommandResult
            {
                Success = process.ExitCode == 0,
                Output = output.Trim(),
                Error = error.Trim()
            };
        }
        catch (Exception ex)
        {
            return new CommandResult { Success = false, Error = ex.Message };
        }
    }

    private class CommandResult
    {
        public bool Success { get; set; }
        public string Output { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }
}