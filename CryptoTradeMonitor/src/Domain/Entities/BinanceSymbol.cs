using Domain.Enums;

namespace Domain.Entities
{
    public class BinanceSymbol
    {
        public string Symbol { get; set; }
        public string BaseAsset { get; set; }
        public string QuoteAsset { get; set; }
        public string Type { get; set; }
        public bool IsSpotTradingAllowed { get; set; }
        public bool IsTradingAllowed { get; set; }
        public MarketType MarketType { get; set; }
    }
}
