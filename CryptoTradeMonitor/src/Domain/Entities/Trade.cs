using Domain.Enums;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using JsonIgnoreAttribute = Newtonsoft.Json.JsonIgnoreAttribute;

namespace Domain.Entities
{
    public class Trade
    {
        [JsonProperty("p")]
        public decimal Price { get; set; }

        [JsonProperty("q")]
        public decimal Quantity { get; set; }

        [JsonProperty("T")]
        public long TradeTimeUnix { get; set; }

        [JsonProperty("t")]
        public long TradeId { get; set; }

        [JsonProperty("m")]
        public bool IsBuyerMaker { get; set; }

        [JsonIgnore]
        public bool IsBuyer => !IsBuyerMaker;

        [JsonIgnore]
        public DateTime TradeTime => DateTimeOffset.FromUnixTimeMilliseconds(TradeTimeUnix).UtcDateTime;

    }
}
