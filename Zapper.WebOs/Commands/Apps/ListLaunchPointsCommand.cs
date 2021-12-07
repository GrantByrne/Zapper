namespace Zapper.WebOs.Commands.Apps
{
    public class ListLaunchPointsCommand : NoPayloadCommandBase
    {
        public override string Uri => "ssap://com.webos.applicationManager/listLaunchPoints";
    }
}
