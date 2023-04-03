namespace Domain.Entities
{
    public class TradePair
    {
        public string BaseAsset { get; }
        public string QuoteAsset { get; }

        public TradePair(string baseAsset, string quoteAsset)
        {
            BaseAsset = baseAsset;
            QuoteAsset = quoteAsset;
        }
    }
}
