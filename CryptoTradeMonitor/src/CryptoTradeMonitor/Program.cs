using Application.Builders;
using Application.Interfaces;
using Application.Managers;
using Infrastructure.Data.Executors;
using Infrastructure.Data.Interfaces;
using Infrastructure.Data.Repositories;

IBinanceApiRequestExecutor apiRequestExecutor = new BinanceApiRequestExecutor();
IExchangeRepository exchangeRepository = new ExchangeRepository(apiRequestExecutor);

IMarketTradePairsBuilder marketTradePairsBuilder = new MarketTradePairsBuilder(exchangeRepository);
IExchangeManager exchangeManager = new ExchangeManager(exchangeRepository, marketTradePairsBuilder);

IBinanceSocketApiRequestExecutor binanceSocketApiRequestExecutor = new BinanceSocketApiRequestExecutor();
ITradesSubscriptionManager tradesSubscriptionManager = new TradesSubscriptionManager(binanceSocketApiRequestExecutor);

IConsoleOutputManager consoleOutputManager = new ConsoleOutputManager(exchangeManager, tradesSubscriptionManager);
await consoleOutputManager.RunAsync();