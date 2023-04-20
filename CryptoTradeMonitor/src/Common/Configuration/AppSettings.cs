using Microsoft.Extensions.Configuration;

namespace Common.Configuration
{
    public static class AppSettings<T> where T : class, new()
    {
        private static readonly Lazy<T> _instance = new(() =>
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                .Build();

            var config = new T();
            configuration.GetSection(typeof(T).Name).Bind(config);
            return config;
        });

        public static T Instance => _instance.Value;
    }
}