using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data.Interfaces;
using Newtonsoft.Json;

namespace Infrastructure.Data.Repositories
{
    public class ExchangeRepository : IExchangeRepository
    {
        private static readonly Dictionary<MarketType, List<string>> _cachedTradePairs = new();
        private readonly IBinanceApiRequestExecutor _requestExecutor;

        public ExchangeRepository(IBinanceApiRequestExecutor requestExecutor)
        {
            _requestExecutor = requestExecutor;
        }

        #region Public methods
        public async Task<List<string>> GetMarketTradePairsAsync(MarketType marketType)
        {
            if (_cachedTradePairs.TryGetValue(marketType, out var cachedTradePairs))
            {
                return cachedTradePairs;
            }

            var response = await _requestExecutor.ExecuteApiRequestAsync(async client =>
            {
                var uri = new Uri("/api/v3/exchangeInfo", UriKind.Relative);
                return await client.GetAsync(uri);
            });

            if (!response.IsSuccessStatusCode || response.Content is null)
            {
                throw new Exception($"Failed to retrieve trade pairs: {response.ReasonPhrase}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var exchangeInfo = JsonConvert.DeserializeObject<BinanceExchangeInfo>(responseContent);

            var tradePairs = exchangeInfo.Symbols
                .Where(s => s.MarketType == marketType)
                .Select(s => s.Symbol)
                .ToList();

            _cachedTradePairs[marketType] = tradePairs;

            return tradePairs;
        }

        public List<string> ChooseTradePairs(IEnumerable<string> tradePairs)
        {
            if (tradePairs?.Any() != true)
            {
                Console.WriteLine("Please enter the desired trade pairs, separated by a comma or space:");
                tradePairs = Console.ReadLine().Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            }

            return tradePairs.ToList();
        }

        public async Task<List<BinanceTrade>> GetTradesAsync(List<TradePair> tradePairs, int tradeHistoryCount = 1000)
        {
            var tasks = tradePairs.Select(async tradePair =>
            {
                var response = await _requestExecutor.ExecuteApiRequestAsync(async client =>
                {
                    var uri = new Uri($"/api/v3/trades?symbol={tradePair.BaseAsset}{tradePair.QuoteAsset}&limit={tradeHistoryCount}", UriKind.Relative);
                    return await client.GetAsync(uri);
                });

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to retrieve trades for {tradePair}: {response.ReasonPhrase}");
                }

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<List<BinanceTrade>>(content);

                return result.Select(trade => MapTrade(tradePair, trade));
            }).ToList();

            var trades = (await Task.WhenAll(tasks)).SelectMany(x => x).ToList();

            return trades;
        }
        #endregion

        #region Private methods
        private BinanceTrade MapTrade(TradePair tradePair, BinanceTrade trade)
        {
            var mappedTrade = new BinanceTrade
            {
                TradePair = tradePair,
                Price = trade.Price,
                Quantity = trade.Quantity,
                IsBuyerMaker = trade.IsBuyerMaker,
                Direction = trade.Direction
            };

            return mappedTrade;
        }
        #endregion
    }
}