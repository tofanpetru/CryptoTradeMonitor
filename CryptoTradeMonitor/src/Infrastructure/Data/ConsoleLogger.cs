using System.Collections.Concurrent;

namespace Infrastructure.Data
{
    public class ConsoleLogger
    {
        private readonly ConcurrentQueue<(string Message, ConsoleColor Color)> _messageQueue;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public ConsoleLogger()
        {
            _messageQueue = new ConcurrentQueue<(string, ConsoleColor)>();
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(() => ProcessLogMessages(), TaskCreationOptions.LongRunning);
        }

        public void Log(string message, ConsoleColor color)
        {
            _messageQueue.Enqueue((message, color));
        }

        private void ProcessLogMessages()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (_messageQueue.TryDequeue(out var message))
                {
                    Console.ForegroundColor = message.Color;
                    Console.WriteLine(message.Message);
                    Console.ResetColor();
                }
                else
                {
                    Thread.Sleep(50);
                }
            }
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
