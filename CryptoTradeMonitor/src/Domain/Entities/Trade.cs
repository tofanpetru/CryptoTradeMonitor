using Domain.Enums;

namespace Domain.Entities
{
    public class Trade
    {
        public decimal Price { get; set; }
        public decimal Quantity { get; set; }
        public TradeDirection Direction { get; set; }
    }
}
