using Common.Configuration;
using Common.Helpers;
using Common.Logging;
using Domain.Configurations;
using Domain.Entities;
using Infrastructure.Data.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace Infrastructure.Data.Executors
{
    public class BinanceSocketApiRequestExecutor : IBinanceSocketApiRequestExecutor, IDisposable
    {
        private readonly ConcurrentDictionary<string, string> _cachedResponses;
        private readonly ConcurrentDictionary<string, Action<string>> _eventCallbacks;
        private Uri _uri;
        private ClientWebSocket _clientWebSocket;
        private CancellationTokenSource _cancellationTokenSource;

        public BinanceSocketApiRequestExecutor()
        {
            _cachedResponses = new ConcurrentDictionary<string, string>();
            _eventCallbacks = new ConcurrentDictionary<string, Action<string>>();
        }

        #region Async Methods
        public async Task ConnectAsync()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _clientWebSocket = new ClientWebSocket();
            await _clientWebSocket.ConnectAsync(_uri, _cancellationTokenSource.Token);
        }

        public async Task SendAsync(string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            await _clientWebSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);
        }

        public async Task<string> ReceiveAsync()
        {
            var buffer = new byte[4096];
            var result = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

            return message;
        }

        public async Task<string> ReceiveAsync(string subscriptionId, TimeSpan timeout)
        {
            var cacheKey = $"SubId:{subscriptionId}";
            if (_cachedResponses.TryGetValue(cacheKey, out var cachedResponse))
            {
                _cachedResponses.TryRemove(cacheKey, out _);
                return cachedResponse;
            }

            using var timeoutCancellationTokenSource = new CancellationTokenSource(timeout);
            using var linkedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(_cancellationTokenSource.Token, timeoutCancellationTokenSource.Token);

            var buffer = new byte[4096];
            var stringBuilder = new StringBuilder();

            while (true)
            {
                var receiveResult = await _clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), linkedCancellationTokenSource.Token);

                if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    await _clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                    return null;
                }

                stringBuilder.Append(Encoding.UTF8.GetString(buffer, 0, receiveResult.Count));

                if (receiveResult.EndOfMessage)
                {
                    var message = stringBuilder.ToString();
                    _cachedResponses.TryAdd(cacheKey, message);
                    return message;
                }
            }
        }
        #endregion

        #region Sync Methods
        public void Connect()
        {
            AsyncHelper.RunSync(ConnectAsync);
        }

        public void Send(string message)
        {
            AsyncHelper.RunSync(() => SendAsync(message));
        }

        public string Receive()
        {
            return AsyncHelper.RunSync(ReceiveAsync);
        }

        public string Receive(string subscriptionId, TimeSpan timeout)
        {
            return AsyncHelper.RunSync(() => ReceiveAsync(subscriptionId, timeout));
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _clientWebSocket.Dispose();
            GC.SuppressFinalize(this);
        }

        public bool Subscribe(string symbol, string eventType, Action<string> callback)
        {
            _uri = new Uri(AppSettings<BinanceConfiguration>.Instance.WebSocketUri + symbol.ToLowerInvariant() + "@" + eventType.ToLowerInvariant());
            _eventCallbacks[eventType] = callback;

            Console.WriteLine($"Registered callback for {symbol}@{eventType}");

            Connect();
            Send($"{{\"method\":\"SUBSCRIBE\",\"params\":[\"{symbol.ToLowerInvariant()}@{eventType.ToLowerInvariant()}\"],\"id\":1}}");

            return true;
        }

        public void StartReceiveLoop(CancellationToken cancellationToken)
        {
            const int maxRetryCount = 3;
            int retryCount = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (_clientWebSocket.State != WebSocketState.Open)
                    {
                        Console.WriteLine($"WebSocket state is {_clientWebSocket.State}, cannot receive message.");
                        if (retryCount >= maxRetryCount)
                        {
                            Console.WriteLine($"WebSocket connection failed after {maxRetryCount} attempts. Exiting receive loop.");
                            return;
                        }
                        Console.WriteLine($"WebSocket connection failed. Retrying in 5 seconds...");
                        Thread.Sleep(TimeSpan.FromSeconds(5));
                        Connect();
                        retryCount++;
                        continue;
                    }
                    var response = Receive();
                    var logger = new ConsoleLogger();

                    PrintResponse(response, logger);

                    if (!string.IsNullOrEmpty(response))
                    {
                        var payload = JObject.Parse(response);

                        // Check if payload is a ping message and respond with a pong
                        if (payload["e"]?.ToString() == "PING")
                        {
                            var pongPayload = new JObject { ["pong"] = payload["ping"] };
                            var pongMessage = pongPayload.ToString();

                            Send(pongMessage);
                        }
                        else
                        {
                            var eventType = payload["e"]?.ToString();
                            var symbol = payload["s"]?.ToString();

                            if (eventType != null && symbol != null)
                            {
                                var eventKey = $"{eventType}@{symbol.ToLowerInvariant()}";
                                if (_eventCallbacks.TryGetValue(eventKey, out var callback))
                                {
                                    callback(payload.ToString());
                                }
                            }
                        }
                    }
                }
                catch (WebSocketException ex)
                {
                    Console.WriteLine($"WebSocket exception in receive loop: {ex}");
                    if (retryCount >= maxRetryCount)
                    {
                        Console.WriteLine($"WebSocket connection failed after {maxRetryCount} attempts. Exiting receive loop.");
                        return;
                    }
                    Console.WriteLine($"WebSocket connection failed. Retrying in 5 seconds...");
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                    Connect();
                    retryCount++;
                    continue;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in receive loop: {ex}");
                }
            }
        }

        private static void PrintResponse(string response, ConsoleLogger logger)
        {
            var output = JsonConvert.DeserializeObject<BinanceTrade>(response);
            if (output != null)
            {
                var color = output.IsBuyerMaker ? ConsoleColor.Green : ConsoleColor.Red;
                logger.Log($"{output.TradePair} - {output.TradeTime}: {output.Price} {output.Quantity}", color);
            }
        }
        #endregion
    }
}
