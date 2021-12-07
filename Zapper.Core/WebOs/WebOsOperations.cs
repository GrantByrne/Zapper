using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Zapper.Core.Devices.Abstract;
using Zapper.Core.WebOs.Abstract;
using Zapper.WebOs.Services;

namespace Zapper.Core.WebOs
{
    public class WebOsOperations
    {
        private readonly IDeviceManager _deviceManager;
        private readonly ILogger<WebOsOperations> _logger;
        private readonly IWebOsConnectionFactory _webOsConnectionFactory;

        public WebOsOperations(
            IDeviceManager deviceManager,
            ILogger<WebOsOperations> logger,
            IWebOsConnectionFactory webOsConnectionFactory)
        {
            _deviceManager = deviceManager;
            _logger = logger;
            _webOsConnectionFactory = webOsConnectionFactory;
        }
        
        public async Task Mute(Guid deviceId)
        {
            var d = _deviceManager.Get(deviceId);
            var connection = await _webOsConnectionFactory.Get(d.IpAddress);
            await connection.Audio.MuteAsync();
        }

        public async Task Unmute(Guid deviceId)
        {
            var d = _deviceManager.Get(deviceId);
            var connection = await _webOsConnectionFactory.Get(d.IpAddress);
            await connection.Audio.UnmuteAsync();
        }

        public async Task VolumeUp(Guid deviceId)
        {
            var d = _deviceManager.Get(deviceId);
            var connection = await _webOsConnectionFactory.Get(d.IpAddress);
            await connection.Audio.VolumeUpAsync();
        }

        public async Task VolumeDown(Guid deviceId)
        {
            var d = _deviceManager.Get(deviceId);
            var connection = await _webOsConnectionFactory.Get(d.IpAddress);
            await connection.Audio.VolumeDownAsync();
        }

        public async Task ChannelDown(Guid deviceId)
        {
            var d = _deviceManager.Get(deviceId);
            var connection = await _webOsConnectionFactory.Get(d.IpAddress);
            await connection.Tv.ChannelDownAsync();
        }

        public async Task ChannelUp(Guid deviceId)
        {
            var d = _deviceManager.Get(deviceId);
            var connection = await _webOsConnectionFactory.Get(d.IpAddress);
            await connection.Tv.ChannelUpAsync();
        }

        public async Task TurnOn3d(Guid deviceId)
        {
            var d = _deviceManager.Get(deviceId);
            var connection = await _webOsConnectionFactory.Get(d.IpAddress);
            await connection.Tv.TurnOn3dAsync();
        }

        public async Task TurnOff3d(Guid deviceId)
        {
            var d = _deviceManager.Get(deviceId);
            var connection = await _webOsConnectionFactory.Get(d.IpAddress);
            await connection.Tv.TurnOff3dAsync();
        }

        public async Task Home(Guid deviceId)
        {
            var d = _deviceManager.Get(deviceId);
            var connection = await _webOsConnectionFactory.Get(d.IpAddress);
            await connection.Control.SendIntentAsync(ControlService.ControlIntent.Home);
        }

        public async Task Back(Guid deviceId)
        {
            var d = _deviceManager.Get(deviceId);
            var connection = await _webOsConnectionFactory.Get(d.IpAddress);
            await connection.Control.SendIntentAsync(ControlService.ControlIntent.Back);
        }

        public async Task Up(Guid deviceId)
        {
            var d = _deviceManager.Get(deviceId);
            var connection = await _webOsConnectionFactory.Get(d.IpAddress);
            await connection.Audio.VolumeDownAsync();
        }

        public async Task Down(Guid deviceId)
        {
            var d = _deviceManager.Get(deviceId);
            var connection = await _webOsConnectionFactory.Get(d.IpAddress);
            await connection.Audio.VolumeDownAsync();
        }

        public async Task Left(Guid deviceId)
        {
            var d = _deviceManager.Get(deviceId);
            var connection = await _webOsConnectionFactory.Get(d.IpAddress);
            await connection.Audio.VolumeDownAsync();
        }

        public async Task Right(Guid deviceId)
        {
            var d = _deviceManager.Get(deviceId);
            var connection = await _webOsConnectionFactory.Get(d.IpAddress);
            await connection.Audio.VolumeDownAsync();
        }

        public async Task Red(Guid deviceId)
        {
            var d = _deviceManager.Get(deviceId);
            var connection = await _webOsConnectionFactory.Get(d.IpAddress);
            await connection.Audio.VolumeDownAsync();
        }

        public async Task Blue(Guid deviceId)
        {
            var d = _deviceManager.Get(deviceId);
            var connection = await _webOsConnectionFactory.Get(d.IpAddress);
            await connection.Audio.VolumeDownAsync();
        }

        public async Task Yellow(Guid deviceId)
        {
            var d = _deviceManager.Get(deviceId);
            var connection = await _webOsConnectionFactory.Get(d.IpAddress);
            await connection.Audio.VolumeDownAsync();
        }

        public async Task Green(Guid deviceId)
        {
            var d = _deviceManager.Get(deviceId);
            var connection = await _webOsConnectionFactory.Get(d.IpAddress);
            await connection.Audio.VolumeDownAsync();
        }

        public async Task FastForward(Guid deviceId)
        {
            var d = _deviceManager.Get(deviceId);
            var connection = await _webOsConnectionFactory.Get(d.IpAddress);
            await connection.Audio.VolumeDownAsync();
        }

        public async Task Pause(Guid deviceId)
        {
            var d = _deviceManager.Get(deviceId);
            var connection = await _webOsConnectionFactory.Get(d.IpAddress);
            await connection.Audio.VolumeDownAsync();
        }

        public async Task Play(Guid deviceId)
        {
            var d = _deviceManager.Get(deviceId);
            var connection = await _webOsConnectionFactory.Get(d.IpAddress);
            await connection.Audio.VolumeDownAsync();
        }

        public async Task Rewind(Guid deviceId)
        {
            var d = _deviceManager.Get(deviceId);
            var connection = await _webOsConnectionFactory.Get(d.IpAddress);
            await connection.Audio.VolumeDownAsync();
        }

        public async Task Stop(Guid deviceId)
        {
            var d = _deviceManager.Get(deviceId);
            var connection = await _webOsConnectionFactory.Get(d.IpAddress);
            await connection.Audio.VolumeDownAsync();
        }

        public async Task PowerOff(Guid deviceId)
        {
            var d = _deviceManager.Get(deviceId);
            var connection = await _webOsConnectionFactory.Get(d.IpAddress);
            await connection.Audio.VolumeDownAsync();
        }

        public async Task PowerOn(Guid deviceId)
        {
            var d = _deviceManager.Get(deviceId);
            var connection = await _webOsConnectionFactory.Get(d.IpAddress);
            await connection.Audio.VolumeDownAsync();
        }

        public async Task ToggleOnOff(Guid deviceId)
        {
            var d = _deviceManager.Get(deviceId);
            var connection = await _webOsConnectionFactory.Get(d.IpAddress);
            await connection.Audio.VolumeDownAsync();
        }
    }
}