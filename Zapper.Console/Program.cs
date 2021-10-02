using System;
using System.Linq;
using System.Threading.Tasks;
using HashtagChris.DotNetBlueZ;
using HashtagChris.DotNetBlueZ.Extensions;

namespace Zapper.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            System.Console.WriteLine("Hello World!");

            var adapter = (await BlueZManager.GetAdaptersAsync()).FirstOrDefault();

            var devices = await adapter.GetDevicesAsync();

            foreach (var d in devices)
            {
                var name = "";

                try
                {
                    name = await d.GetNameAsync();
                }
                catch (Exception)
                {
                }
                
                System.Console.WriteLine($"Found device! {name}");
            }

            System.Console.ReadKey();
        }
    }
}