namespace Zapper.WebOs.Commands.Media
{
    public class ControlRewindCommand : NoPayloadCommandBase
    {
        public override string Uri => "ssap://media.controls/rewind";
    }
}
