namespace Domain.Entities
{
    public class BinanceExchangeInfo
    {
        public List<BinanceSymbol> Symbols { get; set; }
        public string[] Permissions { get; set; }
    }
}
