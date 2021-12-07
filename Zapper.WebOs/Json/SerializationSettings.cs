using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Zapper.WebOs.Json
{
    public static class SerializationSettings
    {
        public static JsonSerializerSettings Default { get; }

        static SerializationSettings()
        {
            Default = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }
    }
}
