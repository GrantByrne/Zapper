using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Zapper.Core.WebOs.Abstract;

namespace Zapper.Core.WebOs;

public class IpAddressManager : IIpAddressManager
{
    public List<string> GetLocalNetworkIpAddresses()
    {
        var ipAddresses = new List<string>();

        var localIpAddress = GetLocalIpAddress();
        var localNetworkPrefix = GetNetworkPrefix(localIpAddress);

        for (var i = 1; i < 255; i++)
        {
            ipAddresses.Add($"{localNetworkPrefix}.{i}");
        }

        return ipAddresses;
    }
    
    private static string GetLocalIpAddress()
    {
        var hostName = Dns.GetHostName();
        var addresses = Dns.GetHostAddresses(hostName);

        foreach (var address in addresses)
        {
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                return address.ToString();
            }
        }

        return null;
    }
    
    private static string GetNetworkPrefix(string ipAddress)
    {
        var ipParts = ipAddress.Split('.');
        var networkPrefix = $"{ipParts[0]}.{ipParts[1]}.{ipParts[2]}";

        return networkPrefix;
    }
}