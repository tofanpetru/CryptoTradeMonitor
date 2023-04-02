# CryptoTradeMonitor
Create a console application in Visual Studio using .NET Core 6.0+.
Choose a specific market type (e.g., SPOT) and implement functionality to select multiple trade pairs (e.g., usdt/btc, eth/btc).
Retrieve a list of trade pairs from a top-10 cryptocurrency exchange (preferably Binance).
Subscribe to trades for the selected trade pairs and store them in a shared in-memory storage (not in a database).
Implement multithreading, with separate threads for each trade pair subscription.
Store the last N trades for each pair (minimum 10,000), allowing N to be set through constants, interactively, or from the command line.
Implement a separate thread to clean old values from memory every minute.
Display the received data in a console table, with separate threads for updating the console, and use color coding for buy and sell trades (green for buy, red for sell).
Use only public API, without any keys or private information.
Minimize the use of external libraries, only use what is necessary for the task.
Follow the specific coding style guidelines provided in the task description.
(Optional Bonus) When built for Windows, the application should not pause when clicking on the console window.

X - async methods, Linq, Entity Framework, and runtime dependency injection.