using System;
using Zapper.Core.Devices;

namespace Zapper.Core.WebOs.Abstract
{
    public interface IWebOsActionFactory
    {
        Action Build(string name, Device device);
    }
}