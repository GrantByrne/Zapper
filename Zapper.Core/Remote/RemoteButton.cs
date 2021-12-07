using System;
using Zapper.Core.KeyboardMouse;

namespace Zapper.Core.Remote;

public class RemoteButton
{
    public long Id { get; set; }
    
    public string Name { get; set; }
        
    public string Action { get; set; }
        
    public EventCode Code { get; set; }
        
    // TODO - Update this to reference a device
    public Guid DeviceId { get; set; }
}