namespace Domain.Configurations
{
    public class BinanceConfiguration
    {
        public string WebSocketUri { get; set; }
        public List<HttpClientConfiguration> HttpClients { get; set; }
    }
}
