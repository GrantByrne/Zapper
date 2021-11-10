using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Timers;
using Zapper.Core.WebOs.Abstract;

namespace Zapper.Core.WebOs
{
    public class WebOsStatusManager : IWebOsStatusManager, IDisposable
    {
        private readonly Dictionary<Guid, WebOsInfo> _statusLookup = new();
        private readonly Queue<(Guid id, string ipaddress)> _registrationQueue = new();
        private readonly Queue<Guid> _deRegistrationQueue = new();
        private readonly Timer _timer = new();

        public WebOsStatusManager()
        {
            _timer.Enabled = true;
            _timer.Interval = 1000 * 30; // 30 Seconds
            _timer.Elapsed += (_, _) => { Run(); };
            _timer.Start();
        }

        public void Register(Guid id, string ipAddress)
        {
            var tuple = (idToAdd: id, ipAddress);
            _registrationQueue.Enqueue(tuple);
        }
        
        public void DeRegister(Guid id)
        {
            _deRegistrationQueue.Enqueue(id);
        }

        public WebOsStatus GetStatus(Guid id)
        {
            if (_statusLookup.TryGetValue(id, out var result))
            {
                return result.Status;
            }

            return WebOsStatus.Unknown;
        }

        private void Run()
        {
            RegisterLookups();
            DeRegisterLookups();

            foreach (var statusLookup in _statusLookup.Values)
            {
                var pingSender = new Ping();
                var options = new PingOptions();

                options.DontFragment = true;

                const string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                var buffer = Encoding.ASCII.GetBytes(data);
                const int timeout = 120;
                var ipAddress = statusLookup.IpAddress;
                var reply = pingSender.Send(ipAddress, timeout, buffer, options);
                
                if (reply?.Status == IPStatus.Success)
                {
                    statusLookup.Status = WebOsStatus.On;
                }
                else
                {
                    statusLookup.Status = WebOsStatus.Off;
                }
            }
        }

        private void DeRegisterLookups()
        {
            while (_deRegistrationQueue.TryPeek(out var id))
            {
                _statusLookup.Remove(id);

                _deRegistrationQueue.Dequeue();
            }
        }

        private void RegisterLookups()
        {
            while (_registrationQueue.TryPeek(out var details))
            {
                var info = new WebOsInfo();

                info.Id = details.id;
                info.Status = WebOsStatus.Unknown;
                info.IpAddress = details.ipaddress;

                _statusLookup[details.id] = info;

                _registrationQueue.Dequeue();
            }
        }
        
        private class WebOsInfo
        {
            public Guid Id { get; set; }
            public string IpAddress { get; set; }
            public WebOsStatus Status { get; set; }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}