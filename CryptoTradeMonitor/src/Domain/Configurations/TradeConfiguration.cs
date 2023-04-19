namespace Domain.Configurations
{
    public class TradeConfiguration
    {
        public string EventType { get; set; }
        public int MaxTradeCount { get; set; }
        public int ClearOldTradesIntervalSeconds { get; set; }
    }
}
