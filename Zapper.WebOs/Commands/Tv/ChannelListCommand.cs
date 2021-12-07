namespace Zapper.WebOs.Commands.Tv
{
    public class ChannelListCommand : NoPayloadCommandBase
    {
        public override string Uri => "ssap://tv/getChannelList";
    }
}
