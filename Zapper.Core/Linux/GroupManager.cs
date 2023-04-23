using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;

namespace Zapper.Core.Linux;

public class LinuxGroupManager : ILinuxGroupManager
{
    public async Task<bool> IsInInputGroup()
    {
        var result = await Cli.Wrap("id")
            .ExecuteBufferedAsync();

        var output = result.StandardOutput;
        
        const StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries;
        var inInputGroup = output.Split(new[] { ' ' }, options)
            .First(p => p.StartsWith("groups"))
            .Remove(0, "groups".Length)
            .Split(',', options)
            .Any(p => p.Contains("input"));

        return inInputGroup;
    }

    public async Task AddUserToInputGroup(string password)
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(10));

        await Cli.Wrap("bash")
            .WithArguments($"-c \"echo '{password}' | sudo -S gpasswd -a $USER input")
            .ExecuteBufferedAsync(cts.Token);
    }

    public async Task RebootSystem(string password)
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(10));
        
        await Cli.Wrap("bash")
            .WithArguments($"-c \"echo '{password}' | sudo -S reboot\"")
            .ExecuteBufferedAsync(cts.Token);
    }
}