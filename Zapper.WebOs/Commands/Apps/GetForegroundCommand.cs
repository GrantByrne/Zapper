namespace Zapper.WebOs.Commands.Apps
{
    public class GetForegroundCommand : NoPayloadCommandBase
    {
        public override string Uri => "ssap://com.webos.applicationManager/getForegroundAppInfo";
    }
}
