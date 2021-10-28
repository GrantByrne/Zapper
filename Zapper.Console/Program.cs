using Zapper.Core.KeyboardMouse;

namespace Zapper.Console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            using var aggHandler = new AggregateInputReader();

            aggHandler.OnKeyPress += (e) => { System.Console.WriteLine($"Code:{e.Code} State:{e.State}"); };
            
            System.Console.ReadLine();
        }
    }
}