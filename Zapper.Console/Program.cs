using System.Threading.Tasks;
using Serilog;

namespace Zapper.Console
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
            
            System.Console.ReadLine();
        }
    }
}