using CryptoAnalyzer.DTO.CoinGecko;
using CryptoAnalyzer.Models;
using CryptoAnalyzer.Service;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using StackExchange.Exceptional;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoAnalyzer.CoinMarketCap
{
    public class FastRefreshHandler
    {
        private readonly CancellationTokenSource _globalCancellation;
        private readonly HttpClient _client;
        private readonly TelegramBot _telegramBot;
        private readonly HashSet<int> notificationsCoins = new HashSet<int>();
        private readonly double UPDATE_FREQUENCY = TimeSpan.FromSeconds(65).TotalMilliseconds;

        public FastRefreshHandler(HttpClient client, TelegramBot telegramBot)
        {
            _globalCancellation = new CancellationTokenSource();
            _client = client;
            _telegramBot = telegramBot;          
        }

        public async Task GrabAsync()
        {
            try
            {
                var stopwatch = new Stopwatch();
                while (!_globalCancellation.Token.IsCancellationRequested)
                {
                    stopwatch.Start();
                    var fastCoin = await Coin.GetFastRefreshCoinsAsync();
                    if (fastCoin.Any())
                    {
                        var querystringParam = new Dictionary<string, string>
                        {
                            ["symbol"] = string.Join(",", fastCoin.Select(x => x.Symbol)),
                            ["skip_invalid"] = "true"
                        };

                        var resp = await _client.GetAsync(QueryHelpers.AddQueryString($"v1/cryptocurrency/quotes/latest", querystringParam));
                        resp.EnsureSuccessStatusCode();
                        var quotes = JsonConvert.DeserializeObject<LatestQuotes>(await resp.Content.ReadAsStringAsync());

                        foreach (var coin in fastCoin)
                        {
                            var lastUpdateTime = await CryptoDataPoint.GetLastUpdateDateAsync(coin.Id);
                            if (lastUpdateTime > DateTimeOffset.UtcNow.AddMinutes(-30))
                            {
                                if (quotes.Data.ContainsKey(coin.Symbol.ToUpper()))
                                {
                                    var quote = quotes.Data[coin.Symbol.ToUpper()]?.Quote["USD"];
                                    if (lastUpdateTime < new DateTimeOffset(quote.LastUpdated) && quote is not null)
                                    {
                                        var dataPoints = new List<CryptoDataPoint>()
                                    {
                                        new CryptoDataPoint()
                                        {
                                            LogDate = new DateTimeOffset(quote.LastUpdated),
                                            Volume = (decimal)quote.Volume24h,
                                            Price = (decimal)quote.Price,
                                            MarketCap = (decimal)quote.MarketCap
                                        }
                                    };
                                        await CryptoDataPoint.BulkInsertAsync(coin.Id, dataPoints);
                                    }
                                }
                            }
                        }
                    }
                    stopwatch.Stop();

                    if(stopwatch.ElapsedMilliseconds < UPDATE_FREQUENCY)
					{
                        await Task.Delay(TimeSpan.FromMilliseconds(UPDATE_FREQUENCY - stopwatch.ElapsedMilliseconds), _globalCancellation.Token);
                    }
                    stopwatch.Reset();                  
                }
            }
            catch(Exception e)
            {
                e.LogNoContext();
            }
        }

        public void Cancel()
		{
            _globalCancellation.Cancel();
		}
    }
}
