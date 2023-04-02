using Application.IoC;
using Infrastructure.IoC;
using Microsoft.Extensions.DependencyInjection;

namespace CryptoTradeMonitor.IoC
{
    public static class AddDependencyService
    {
        public static IServiceCollection AddDependencyServiceExtension(this IServiceCollection services)
        {
            services.AddAppServiceExtension();
            services.AddRepositoriesExtension();

            return services;
        }
    }
}
