using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Zapper.Core.WebOs;

public class LgTvScanner
{
    public class TvData
    {
        public string Uuid { get; set; }
        public string TvName { get; set; }
        public string Address { get; set; }
    }

    public List<TvData> LgTvScan()
    {
        var request = Encoding.ASCII.GetBytes(
            "M-SEARCH * HTTP/1.1\r\n" +
            "HOST: 239.255.255.250:1900\r\n" +
            "MAN: \"ssdp:discover\"\r\n" +
            "MX: 2\r\n" +
            "ST: urn:schemas-upnp-org:device:MediaRenderer:1\r\n\r\n"
        );

        var sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        sock.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 10000);

        var addresses = new List<TvData>();
        var attempts = 4;
        for (var i = 0; i < attempts; i++)
        {
            sock.SendTo(request, new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900));
            string uuid = null;
            string tvName = null;
            string address = null;
            var responseBuffer = new byte[512];
            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            var bytesRead = sock.ReceiveFrom(responseBuffer, ref remoteEP);
            var response = Encoding.ASCII.GetString(responseBuffer, 0, bytesRead);
            foreach (var line in response.Split(new[] { '\n' }))
            {
                if (line.StartsWith("USN"))
                {
                    try
                    {
                        uuid = Regex.Match(line, @"uuid:(.*?):").Groups[1].Value;
                    }
                    catch
                    {
                        // Handle exception or ignore
                        continue;
                    }
                }
                if (line.StartsWith("DLNADeviceName.lge.com"))
                {
                    tvName = HttpUtility.UrlDecode(line.Split(':')[1].Trim());
                }
            }
            var data = new TvData
            {
                Uuid = uuid,
                TvName = tvName,
                Address = ((IPEndPoint)remoteEP).Address.ToString()
            };

            if (response.Contains("LG"))
            {
                addresses.Add(data);
            }
            else
            {
                Console.WriteLine("Unknown device");
                Console.WriteLine(response);
                Console.WriteLine(remoteEP);
            }

            System.Threading.Thread.Sleep(2000);
        }

        sock.Close();
        var addressDict = new Dictionary<string, TvData>();
        foreach (var tvData in addresses)
        {
            addressDict[tvData.Address] = tvData;
        }
        addresses = new List<TvData>(addressDict.Values);
        return addresses;
    }
}