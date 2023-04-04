using Application.Interfaces;
using System.Runtime.InteropServices;

namespace Application.Managers
{
    public class ConsoleOutputManager : IConsoleOutputManager
    {
        private readonly IExchangeManager _exchangeManager;

        public ConsoleOutputManager(IExchangeManager exchangeManager)
        {
            _exchangeManager = exchangeManager;
        }

        public async Task Run()
        {
            Console.WriteLine("Please enter the desired trade pairs, separated by a comma or space:");
            var tradePairsStr = Console.ReadLine();
            var selectedTradePairs = tradePairsStr.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            /*SOON*/

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.ReadLine();
            }
        }
    }
}
