using Application.Interfaces;
using CryptoTradeMonitor.IoC;
using Microsoft.Extensions.DependencyInjection;

var serviceProvider = new ServiceCollection()
                .AddDependencyServiceExtension()
                .BuildServiceProvider();

var app = serviceProvider.GetService<IConsoleOutputManager>();

await app.Run();