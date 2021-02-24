using CryptoAnalyzer.DTO.CoinGecko;
using CryptoAnalyzer.Models;
using CryptoAnalyzer.Service;
using Microsoft.AspNetCore.WebUtilities;
using StackExchange.Exceptional;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoAnalyzer.CoinGecko
{
    public class SpotlightHandler
    {
        private readonly CancellationTokenSource _globalCancellation;
        private readonly ThrottledHttpClient _client;
        private readonly TelegramBot _telegramBot;
        private readonly HashSet<int> notificationsCoins = new HashSet<int>();
        private readonly Dictionary<int, DateTimeOffset> noDataCoins = new Dictionary<int, DateTimeOffset>();
        private static Dictionary<int, DateTimeOffset> buzzCoinsLastGrab = new Dictionary<int, DateTimeOffset>();
        private readonly double UPDATE_FREQUENCY = TimeSpan.FromMinutes(6).TotalMilliseconds;

        public SpotlightHandler(ThrottledHttpClient client, TelegramBot telegramBot)
        {
            _globalCancellation = new CancellationTokenSource();
            _client = client;
            _telegramBot = telegramBot;          
        }

        public async Task GrabAsync()
        {
            try
            {
                while (!_globalCancellation.Token.IsCancellationRequested)
                {
                    var tryAgainCoins = noDataCoins.Where(x => DateTimeOffset.UtcNow >= x.Value).Select(x => x.Key).ToHashSet();
                    var importantCoins = (await Coin.GetImportantCoinsAsync(DateTimeOffset.UtcNow.AddDays(Context.COIN_DAYS))).Where(x=> !x.FastRefresh).ToList();
                    var internetBuzzCoins = GetNewBuzzToGrab(await Coin.GetInternetBuzzCoinsAsync(DateTimeOffset.UtcNow.AddDays(Context.COIN_DAYS)), Context.MAX_CONCURRENT_COINS - importantCoins.Count);
                    importantCoins.AddRange(internetBuzzCoins);

                    var coins = importantCoins.Where(x => (!x.IsUseless() || x.UnderSpotlight)
                                    && (!noDataCoins.ContainsKey(x.Id) || tryAgainCoins.Contains(x.Id))).ToList();

                    foreach(var coinID in tryAgainCoins)
					{
                        noDataCoins.Remove(coinID);
                    }
                    
                    var waitTime = UPDATE_FREQUENCY / coins.Count;
                    var stopwatch = new Stopwatch();
                    foreach (var coin in coins)
                    {
                        stopwatch.Start();
                        var lastUpdateTime = await CryptoDataPoint.GetLastUpdateDateAsync(coin.Id);
                        if (lastUpdateTime == null || (DateTimeOffset.UtcNow - lastUpdateTime > TimeSpan.FromDays(1)))
                            lastUpdateTime = DateTimeOffset.Now.AddMinutes(5).AddDays(-1);

                        var querystringParam = new Dictionary<string, string>
                        {
                            ["vs_currency"] = "usd",
                            ["from"] = lastUpdateTime.Value.ToUnixTimeSeconds().ToString(),
                            ["to"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                        };
                        var coinDataByInterval = await _client.GetAsync<MarketChart>(QueryHelpers.AddQueryString($"coins/{coin.Code}/market_chart/range", querystringParam));
                        if (coinDataByInterval is null) continue;
                        if (coinDataByInterval.HasDifferentSizes)
                        {
                            throw new Exception("Data was not taken at the same time");
                        }

                        var dataPoints = new List<CryptoDataPoint>();
                        for (int i = 0; i < coinDataByInterval.TotalVolumes.Count; i++)
                        {
                            var volume = coinDataByInterval.TotalVolumes[i];
                            var marketCap = coinDataByInterval.MarketCaps[i];
                            var price = coinDataByInterval.Prices[i];

                            dataPoints.Add(new CryptoDataPoint()
                            {
                                LogDate = volume.Date,
                                Volume = volume.PointValue,
                                Price = price.PointValue,
                                MarketCap = marketCap.PointValue
                            });
                        }
                        await CryptoDataPoint.BulkInsertAsync(coin.Id, dataPoints);
                        await ParseDataAsync(coin);

                        stopwatch.Stop();
                        if (stopwatch.ElapsedMilliseconds < waitTime)
                        {
                            await Task.Delay(TimeSpan.FromMilliseconds(waitTime - stopwatch.ElapsedMilliseconds), _globalCancellation.Token);
                        }
                        stopwatch.Reset();
                    }
                }
            }
            catch(Exception e)
            {
                e.LogNoContext();
            }
        }

        private async Task ParseDataAsync(Coin coin)
		{
            try
            {
                var data = await CryptoDataPoint.GetTimeframeAsync(DateTimeOffset.UtcNow.AddDays(-7), DateTimeOffset.UtcNow, coin.Id);
                var today = data.Where(x => x.LogDate >= DateTimeOffset.UtcNow.AddDays(-1)).ToList();
                var yesterday = data.Where(x => x.LogDate >= DateTimeOffset.UtcNow.AddDays(-2) && x.LogDate < DateTimeOffset.UtcNow.AddDays(-1)).ToList();
                var lastPoint = data.LastOrDefault();
              
                if (today.Any())
                {
                    var recap = CoinRecap.GetRecap(today, yesterday);
                    if (recap.LastHourVolumeVariation >= 0.3M || recap.Last3HoursVolumeVariation >= 0.40M || recap.Last9HoursVolumeVariation >= 0.45M)
                    {                     
                        if (!notificationsCoins.Contains(coin.Id))
                        {
                            await _telegramBot.SendMessageAsync(Context.TelegramBotConfiguration.ConversationID, CreateMessage(recap,coin));
                            notificationsCoins.Add(coin.Id);
                        }
                    }
                    else if(notificationsCoins.Contains(coin.Id))
					{
                        await _telegramBot.SendMessageAsync(Context.TelegramBotConfiguration.ConversationID, $"{coin.Code} ({coin.Name}) is now bad");
                        notificationsCoins.Remove(coin.Id);
					}
                }
                else if(!noDataCoins.ContainsKey(coin.Id))
				{
                    new Exception($"No data point today for {coin.Code} ({coin.Name})").LogNoContext();
                    noDataCoins.Add(coin.Id, DateTimeOffset.UtcNow.AddHours(1)); //don't grab data for the next hour
				}
            }
            catch(Exception e)
			{
                e.LogNoContext();
            }
        }

		private string CreateMessage(CoinRecap recap, Coin coin)
		{
            var sb = new StringBuilder($"{coin.Code} ({coin.Name}) \n");
            if (recap.LastHourVolumeVariation >= 0.3M)
                sb.AppendLine($"LastHourVolumeVariation: {Math.Round(recap.LastHourVolumeVariation, 2)}");
            if (recap.Last3HoursVolumeVariation >= 0.4M)
                sb.AppendLine($"Last3HoursVolumeVariation: {Math.Round(recap.Last3HoursVolumeVariation, 2)}");
            if (recap.Last9HoursVolumeVariation >= 0.45M)
                sb.AppendLine($"Last9HoursVolumeVariation: {Math.Round(recap.Last9HoursVolumeVariation, 2)}");
            if (recap.HugeVolSpikeLastHour)
                sb.AppendLine($"HugeVolSpikeLastHour!!");

            return sb.ToString();
        }

		private static List<Coin> GetNewBuzzToGrab(List<Coin> coins, int num)
		{
            var newBuzz = coins.Where(x => !buzzCoinsLastGrab.ContainsKey(x.Id)).Take(num).ToList();
            newBuzz.ForEach(x => buzzCoinsLastGrab.Add(x.Id, DateTimeOffset.UtcNow));

            if (newBuzz.Count != num && newBuzz.Count != coins.Count)
			{
                var oldestCoinID = buzzCoinsLastGrab.OrderBy(x => x.Value).Select(x => x.Key).ToHashSet();
                var toGrab = coins.Where(x => oldestCoinID.Contains(x.Id)).Take(num - newBuzz.Count).ToList();
                newBuzz.AddRange(toGrab);
                toGrab.ForEach(x => buzzCoinsLastGrab[x.Id] = DateTimeOffset.UtcNow);
            }

            return newBuzz;
        }

        public void Cancel()
		{
            _globalCancellation.Cancel();
		}
    }
}
