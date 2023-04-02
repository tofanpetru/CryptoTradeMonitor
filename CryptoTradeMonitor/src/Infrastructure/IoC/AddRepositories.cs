using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.IoC
{
    public static class AddRepositories
    {
        public static IServiceCollection AddRepositoriesExtension(this IServiceCollection appService)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();

            types.Where(type => type.IsInterface).ToList()
                .ForEach(interfac => types.Where(type => type.GetInterfaces().Contains(interfac)).ToList()
                .ForEach(implementation => appService.AddScoped(interfac, implementation)));

            return appService;
        }
    }
}
