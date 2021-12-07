namespace Zapper.WebOs.Commands.Tv
{
    public class GetChannelProgramInfoCommand : NoPayloadCommandBase
    {
        public override string Uri => "ssap://tv/getChannelProgramInfo";
    }
}
