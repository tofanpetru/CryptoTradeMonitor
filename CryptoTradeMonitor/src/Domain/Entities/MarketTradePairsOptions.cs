using Domain.Enums;

namespace Domain.Entities
{
    public class MarketTradePairsOptions
    {
        public List<string> Symbols { get; set; }
        public List<PermissionType> Permissions { get; set; }
    }
}
