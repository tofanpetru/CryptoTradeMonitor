using Domain.Enums;

namespace Domain.Entities
{
    public class BinanceTrade
    {
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public TradeDirection Direction { get; set; }
        public bool IsBuyerMaker { get; set; }
        public TradePair TradePair { get; set; }

        public static Trade ToTrade(BinanceTrade binanceTrade)
        {
            return new Trade
            {
                Price = binanceTrade.Price,
                Quantity = binanceTrade.Quantity,
                Direction = binanceTrade.Direction,
            };
        }
    }

}
