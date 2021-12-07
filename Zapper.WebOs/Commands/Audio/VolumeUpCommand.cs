namespace Zapper.WebOs.Commands.Audio
{
    public class VolumeUpCommand : NoPayloadCommandBase
    {
        public override string Uri => "ssap://audio/volumeUp";
    }
}