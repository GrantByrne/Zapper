using Zapper.Core.KeyboardMouse;

namespace Zapper.Web.Data
{
    public class RemoteButton
    {
        public string Name { get; set; }
        
        public string Action { get; set; }
        
        public EventCode Code { get; set; }
    }
}