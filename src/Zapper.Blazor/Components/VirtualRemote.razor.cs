using Microsoft.AspNetCore.Components;
using Zapper.Client.Devices;
using Zapper.Client;
using CommandType = Zapper.Core.Models.CommandType;

namespace Zapper.Blazor.Components;

public partial class VirtualRemote : ComponentBase
{
    [Parameter] public DeviceDto? Device { get; set; }
    [Parameter] public EventCallback<CommandType> OnCommandSend { get; set; }
    [Parameter] public EventCallback<string> OnCustomCommandSend { get; set; }
    [Parameter] public EventCallback<int> OnNumberCommandSend { get; set; }
    [Parameter] public List<string>? AdditionalCommands { get; set; }
    [Parameter] public bool IsCompact { get; set; }

    private async Task SendCommand(CommandType commandType)
    {
        if (OnCommandSend.HasDelegate)
        {
            await OnCommandSend.InvokeAsync(commandType);
        }
    }

    private async Task SendCustomCommand(string command)
    {
        if (OnCustomCommandSend.HasDelegate)
        {
            await OnCustomCommandSend.InvokeAsync(command);
        }
    }

    private async Task SendNumberCommand(int number)
    {
        if (OnNumberCommandSend.HasDelegate)
        {
            await OnNumberCommandSend.InvokeAsync(number);
        }
    }
}