using System;
using Zapper.Core.WebOs;

namespace Zapper.Web.Data
{
    public class DeviceModel
    {
        public Guid Id { get; set; }
        
        public WebOsStatus Status { get; set; }
        
        public string DeviceType { get; set; }
        
        public string Name { get; set; }
    }
}