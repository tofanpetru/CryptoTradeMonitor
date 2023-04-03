using Application.Interfaces;
using CryptoTradeMonitor.IoC;
using Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

var serviceProvider = new ServiceCollection()
                .AddDependencyServiceExtension()
                .BuildServiceProvider();

var app = serviceProvider.GetService<IConsoleOutputManager>();

app.Run();