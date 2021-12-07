using Newtonsoft.Json.Linq;

namespace Zapper.WebOs.Commands
{
    public abstract class NoPayloadCommandBase : CommandBase
    {
        public override JObject ToJObject()
        {
            return null;
        }
    }
}
