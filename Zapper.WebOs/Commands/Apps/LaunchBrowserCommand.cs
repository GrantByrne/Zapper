using Newtonsoft.Json;

namespace Zapper.WebOs.Commands.Apps
{
    public class LaunchBrowserCommand : CommandBase
    {
        public override string Uri => "ssap://system.launcher/open";

        [JsonProperty("target")]
        public string BrowserUrl { get; set; }
    }
}
