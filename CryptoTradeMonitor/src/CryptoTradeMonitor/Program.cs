using CryptoTradeMonitor.IoC;
using Domain.Enums;
using Infrastructure.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;

var serviceProvider = new ServiceCollection()
                .AddDependencyServiceExtension()
                .BuildServiceProvider();

/*try
{*/
var exchangeApi = serviceProvider.GetService<IExchangeApi>();

var test = await exchangeApi.GetMarketTradePairsAsync(MarketType.Spot);
/*
 var trades = await exchangeApi.GetTradesAsync(filteredTradePairs, 10);
 foreach (var trade in trades)
 {
     Console.WriteLine($"Trade: {trade.TradePair} - {trade.Price} - {trade.Quantity}");
 }*/
/*}
catch (Exception ex)
{
    Console.WriteLine($"An error occurred: {ex.Message}");
}*/

Console.WriteLine("Press any key to exit...");
Console.ReadKey();
