namespace Domain.Entities
{
    public class Trade
    {
        public long TradeId { get; set; }
        public string TradePairSymbol { get; set; }
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public bool IsBuyerMaker { get; set; }
        public DateTime TradeTime { get; set; }
    }
}
