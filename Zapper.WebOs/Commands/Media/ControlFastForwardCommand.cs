﻿namespace Zapper.WebOs.Commands.Media
{
    public class ControlFastForwardCommand : NoPayloadCommandBase
    {
        public override string Uri => "ssap://media.controls/fastForward";
    }
}
