using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Zapper.WebOs.Json;

namespace Zapper.WebOs.Commands
{
    public abstract class CommandBase
    {
        [JsonIgnore]
        public abstract string Uri { get; }

        [JsonIgnore]
        public string CustomId { get; set; }

        [JsonIgnore]
        public string CustomType { get; set; }

        public virtual JObject ToJObject()
        {
            return JObject.FromObject(this, JsonSerializer.CreateDefault(SerializationSettings.Default));
        }
    }
}
