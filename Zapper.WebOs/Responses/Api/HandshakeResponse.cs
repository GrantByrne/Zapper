using Newtonsoft.Json;

namespace Zapper.WebOs.Responses.Api
{
    public class HandshakeResponse : ResponseBase
    {
        [JsonProperty("client-key")]
        public string Key { get; set; }

        public HandshakeResponse()
        {
            ReturnValue = true;
        }
    }
}
