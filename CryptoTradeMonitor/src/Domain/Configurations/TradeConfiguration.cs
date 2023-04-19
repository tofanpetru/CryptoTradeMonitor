using Domain.Enums;

namespace Domain.Configurations
{
    public class TradeConfiguration
    {
        public PermissionType PermissionType { get; set; } = PermissionType.SPOT;
        public string EventType { get; set; }
        public int MaxTradeCount { get; set; }
        public int ClearOldTradesIntervalSeconds { get; set; }
    }
}
