namespace Zapper.WebOs.Commands.Tv
{
    public class ThreeDimensionStatusCommand : NoPayloadCommandBase
    {
        public override string Uri => "ssap://com.webos.service.tv.display/get3DStatus";
    }
}
