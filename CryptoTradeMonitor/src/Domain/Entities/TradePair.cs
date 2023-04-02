namespace Domain.Entities
{
    public class TradePair
    {
        public string Symbol { get; set; }
        public string BaseAsset { get; set; }
        public string QuoteAsset { get; set; }
        public string MarketType { get; set; }
    }
}
