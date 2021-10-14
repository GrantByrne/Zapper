using System.Linq;
using NUnit.Framework;
using Zapper.Core.KeyboardMouse;

namespace Zapper.Core.Test
{
    public class DeviceManagerTest
    {
        [Test]
        public void Get_ReadsFile_Success()
        {
            var path = "./devices.txt";
            
            var result = DeviceReader.Get(path);
            
            Assert.IsTrue(result.Any(), "Expected to get some devices back");
        }
    }
}