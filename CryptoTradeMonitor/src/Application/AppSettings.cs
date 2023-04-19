using Domain.Configurations;
using Microsoft.Extensions.Configuration;

namespace Application
{
    public static class AppSettings
    {
        private static readonly IConfiguration _configuration;

        public static AppConfiguration Configuration { get; private set; }

        static AppSettings()
        {
            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                .Build();

            Configuration = _configuration.GetSection("AppConfiguration").Get<AppConfiguration>();
        }
    }

}
