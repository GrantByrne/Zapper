namespace Zapper.WebOs.Commands.Tv
{
    public class ChannelUpCommand : NoPayloadCommandBase
    {
        public override string Uri => "ssap://tv/channelUp";
    }
}
