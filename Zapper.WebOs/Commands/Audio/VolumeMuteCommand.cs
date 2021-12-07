using Newtonsoft.Json;

namespace Zapper.WebOs.Commands.Audio
{
    public class VolumeMuteCommand : CommandBase
    {
        public override string Uri => "ssap://audio/setMute";

        [JsonIgnore]
        public bool Mute { get; set; }

        [JsonProperty("mute")]
        internal int MuteInternal => Mute ? 1 : 0;
    }
}
