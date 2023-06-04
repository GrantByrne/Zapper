using System.Collections.Generic;

namespace Zapper.Core.WebOs.Abstract;

public interface IIpAddressManager
{
    List<string> GetLocalNetworkIpAddresses();
}