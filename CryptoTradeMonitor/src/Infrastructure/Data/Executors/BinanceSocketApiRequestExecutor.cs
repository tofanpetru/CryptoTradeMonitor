using Infrastructure.Data.Interfaces;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace Infrastructure.Data.Executors
{
    public class BinanceSocketApiRequestExecutor : IBinanceSocketApiRequestExecutor, IDisposable
    {
        private ClientWebSocket _clientWebSocket;
        private CancellationTokenSource _cancellationTokenSource;
        private Uri _uri;
        private ConcurrentDictionary<string, string> _cachedResponses;

        public BinanceSocketApiRequestExecutor(Uri uri)
        {
            _uri = uri;
            _cachedResponses = new ConcurrentDictionary<string, string>();
        }

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

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _clientWebSocket.Dispose();
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

        public async Task<bool> SubscribeAsync(string message)
        {
            var retries = 0;
            var success = false;

            while (retries < 5)
            {
                await SendAsync(message);

                var response = await ReceiveAsync();
                if (!string.IsNullOrEmpty(response))
                {
                    success = true;
                    break;
                }

                retries++;
            }

            return success;
        }
    }
}
