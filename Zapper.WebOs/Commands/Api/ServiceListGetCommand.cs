﻿namespace Zapper.WebOs.Commands.Api
{
    public class ServiceListGetCommand : NoPayloadCommandBase
    {
        public override string Uri => "ssap://api/getServiceList";
    }
}
