using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using WebOsTv.Net;
using WebOsTv.Net.Services;
using Zapper.Core.Devices;
using Zapper.Core.WebOs;
using Zapper.Core.WebOs.Abstract;

namespace Zapper.Core.Test.WebOs
{
    [TestFixture]
    public class WebOsActionFactoryTest
    {
        private readonly IWebOsConnectionFactory _webOsConnectionFactory = Substitute.For<IWebOsConnectionFactory>();
        private readonly IWebOsStatusManager _webOsStatusManager = Substitute.For<IWebOsStatusManager>();
        private readonly IWakeOnLanManager _wakeOnLanManager = Substitute.For<IWakeOnLanManager>();
        private readonly ILogger<WebOsActionFactory> _logger = Substitute.For<ILogger<WebOsActionFactory>>();
        private readonly IAudioService _audioService = Substitute.For<IAudioService>();
        private readonly IService _service = Substitute.For<IService>();
        
        [Test]
        public async Task Mute_Test()
        {
            Test(WebOsActionKey.Mute);

            await _service.Audio.Received().MuteAsync();
        }
        
        [Test]
        public async Task Unmute_Test()
        {
            Test(WebOsActionKey.Unmute);

            await _service.Audio.Received().UnmuteAsync();
        }
        
        [Test]
        public async Task VolumeUp_Test()
        {
            Test(WebOsActionKey.VolumeUp);

            await _service.Audio.Received().VolumeUpAsync();
        }
        
        [Test]
        public async Task VolumeDown_Test()
        {
            Test(WebOsActionKey.VolumeDown);

            await _service.Audio.Received().VolumeDownAsync();
        }
        
        [Test]
        public async Task ChannelDown_Test()
        {
            Test(WebOsActionKey.ChannelDown);

            await _service.Tv.Received().ChannelDownAsync();
        }
        
        [Test]
        public async Task ChannelUp_Test()
        {
            Test(WebOsActionKey.ChannelUp);

            await _service.Tv.Received().ChannelUpAsync();
        }
        
        [Test]
        public async Task TurnOn3d_Test()
        {
            Test(WebOsActionKey.TurnOn3d);

            await _service.Tv.Received().TurnOn3dAsync();
        }
        
        [Test]
        public async Task TurnOff3d_Test()
        {
            Test(WebOsActionKey.TurnOff3d);

            await _service.Tv.Received().TurnOff3dAsync();
        }
        
        [Test]
        public async Task Home_Test()
        {
            Test(WebOsActionKey.Home);

            await _service.Control.Received().SendIntentAsync(ControlService.ControlIntent.Home);
        }
        
        [Test]
        public async Task Back_Test()
        {
            Test(WebOsActionKey.Back);

            await _service.Control.Received().SendIntentAsync(ControlService.ControlIntent.Back);
        }
        
        [Test]
        public async Task Up_Test()
        {
            Test(WebOsActionKey.Up);

            await _service.Control.Received().SendIntentAsync(ControlService.ControlIntent.Up);
        }
        
        [Test]
        public async Task Down_Test()
        {
            Test(WebOsActionKey.Down);

            await _service.Control.Received().SendIntentAsync(ControlService.ControlIntent.Down);
        }
        
        [Test]
        public async Task Left_Test()
        {
            Test(WebOsActionKey.Left);

            await _service.Control.Received().SendIntentAsync(ControlService.ControlIntent.Left);
        }
        
        [Test]
        public async Task Right_Test()
        {
            Test(WebOsActionKey.Right);

            await _service.Control.Received().SendIntentAsync(ControlService.ControlIntent.Right);
        }
        
        [Test]
        public async Task Red_Test()
        {
            Test(WebOsActionKey.Red);

            await _service.Control.Received().SendIntentAsync(ControlService.ControlIntent.Red);
        }
        
        [Test]
        public async Task Blue_Test()
        {
            Test(WebOsActionKey.Blue);

            await _service.Control.Received().SendIntentAsync(ControlService.ControlIntent.Blue);
        }
        
        [Test]
        public async Task Yellow_Test()
        {
            Test(WebOsActionKey.Yellow);

            await _service.Control.Received().SendIntentAsync(ControlService.ControlIntent.Yellow);
        }
        
        [Test]
        public async Task Green_Test()
        {
            Test(WebOsActionKey.Green);

            await _service.Control.Received().SendIntentAsync(ControlService.ControlIntent.Green);
        }
        
        [Test]
        public async Task FastForward_Test()
        {
            Test(WebOsActionKey.FastForward);

            await _service.Control.Received().SendIntentAsync(ControlService.ControlIntent.FastForward);
        }
        
        [Test]
        public async Task Pause_Test()
        {
            Test(WebOsActionKey.Pause);

            await _service.Control.Received().SendIntentAsync(ControlService.ControlIntent.Pause);
        }
        
        [Test]
        public async Task Play_Test()
        {
            Test(WebOsActionKey.Play);

            await _service.Control.Received().SendIntentAsync(ControlService.ControlIntent.Play);
        }
        
        [Test]
        public async Task Rewind_Test()
        {
            Test(WebOsActionKey.Rewind);

            await _service.Control.Received().SendIntentAsync(ControlService.ControlIntent.Rewind);
        }
        
        [Test]
        public async Task Stop_Test()
        {
            Test(WebOsActionKey.Stop);

            await _service.Control.Received().SendIntentAsync(ControlService.ControlIntent.Stop);
        }
        
        [Test]
        public async Task PowerOff_Test()
        {
            Test(WebOsActionKey.PowerOff);

            await _service.Control.Received().SendIntentAsync(ControlService.ControlIntent.PowerOff);
        }
        
        [Test]
        public void PowerOn_Test()
        {
            var device = MakeDevice();

            var sut = Build(device);

            var action = sut.Build(WebOsActionKey.PowerOn, device);

            action();
            
            _wakeOnLanManager.Received().Send(device.IpAddress, device.MacAddress);
        }

        [TestCase(WebOsStatus.Unknown)]
        [TestCase(WebOsStatus.Off)]
        public void ToggleOnOff_NotOn_SendsWakeOnLan(WebOsStatus status)
        {
            var device = MakeDevice();

            var sut = Build(device);

            var action = sut.Build(WebOsActionKey.ToggleOnOff, device);

            _webOsStatusManager.GetStatus(device.Id)
                .Returns(status);

            action();
            
            _wakeOnLanManager.Received().Send(device.IpAddress, device.MacAddress);
        }
        
        [Test]
        public void ToggleOnOff_DeviceOn_TurnOff()
        {
            var device = MakeDevice();

            var sut = Build(device);

            var action = sut.Build(WebOsActionKey.ToggleOnOff, device);

            _webOsStatusManager.GetStatus(device.Id)
                .Returns(WebOsStatus.On);

            action();

            _service.Control.Received().SendIntentAsync(ControlService.ControlIntent.PowerOff);
        }

        private void Test(string actionKey)
        {
            var device = MakeDevice();

            var sut = Build(device);   
            
            var action = sut.Build(actionKey, device);
            
            action();
        }

        private WebOsActionFactory Build(Device device)
        {
            var sut = new WebOsActionFactory(
                _webOsConnectionFactory,
                _webOsStatusManager,
                _wakeOnLanManager,
                _logger);
            
            _service.Audio.Returns(_audioService);
            
            var serviceTask = Task.FromResult(_service);
            _webOsConnectionFactory.Get(device.IpAddress)
                .Returns(serviceTask);
            
            return sut;
        }

        private static Device MakeDevice()
        {
            var device = new Device();

            device.Id = Guid.NewGuid();
            device.Name = "WebOS TV";
            device.IpAddress = "192.168.1.1";
            device.MacAddress = "00-11-22-33-44-55";
            
            return device;
        }
    }
}