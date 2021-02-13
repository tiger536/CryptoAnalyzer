﻿using CryptoAnalyzer.DTO.CoinGecko;
using CryptoAnalyzer.Models;
using CryptoAnalyzer.Service;
using Microsoft.AspNetCore.WebUtilities;
using StackExchange.Exceptional;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoAnalyzer.CoinGecko
{
    public class SpotlightHandler
    {
        private readonly CancellationTokenSource _globalCancellation;
        private readonly IThrottledService _client;
        private readonly TelegramBot _telegramBot;
        private readonly double UPDATE_FREQUENCY = TimeSpan.FromMinutes(5).TotalMilliseconds;
        public SpotlightHandler(IThrottledService client, TelegramBot telegramBot)
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
                    var coins = (await Coin.GetImportantCoinsAsync(DateTimeOffset.UtcNow.AddDays(-3))).Where(x => !x.IsUseless()).ToList();

                    var waitTime = UPDATE_FREQUENCY / coins.Count;

                    var task = Task.Run(async () =>
                    {
                        var stopwatch = new Stopwatch();
                        foreach(var coin in coins)
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
                            if (coinDataByInterval is null) return;
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
                    });

                    await Task.WhenAll(task, Task.Delay((int)UPDATE_FREQUENCY));
                }
            }
            catch(Exception e)
            {
                e.LogNoContext();
            }
        }

        private async Task ParseDataAsync(Coin coin)
		{
            var data = await CryptoDataPoint.GetTimeframeAsync(DateTimeOffset.UtcNow.AddDays(-7), DateTimeOffset.UtcNow, coin.Id);
            var today = data.Where(x => x.LogDate >= DateTimeOffset.UtcNow.AddDays(-1)).ToList();
            var yesterday = data.Where(x => x.LogDate >= DateTimeOffset.UtcNow.AddDays(-2) && x.LogDate < DateTimeOffset.UtcNow.AddDays(-1)).ToList();
            var lastPoint = data.Last();

            var recap = CoinRecap.GetRecap(today, yesterday);

            if(recap.LastHourVolumeVariation >= 0.3M || recap.Last3HoursVolumeVariation >= 0.40M || recap.Last9HoursVolumeVariation >= 0.45M)
			{
                await _telegramBot.SendMessageAsync(coin.Code);
			}
        }

        public void Cancel()
		{
            _globalCancellation.Cancel();
		}

    }
}
