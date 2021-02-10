using CryptoAnalyzer.CoinGecko.DTO;
using CryptoAnalyzer.DTO.CoinGecko;
using CryptoAnalyzer.Models;
using CryptoAnalyzer.Service;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoAnalyzer.CoinGecko
{
    public class SpotlightHandler
    {
        private CancellationTokenSource _globalCancellation;
        private readonly IThrottledService _client;
        private double UPDATE_FREQUENCY = TimeSpan.FromMinutes(5).TotalMilliseconds;
        public SpotlightHandler(IThrottledService client)
        {
            _globalCancellation = new CancellationTokenSource();
            _client = client;
        }

        public async Task GrabAsync()
        {
            try
            {
                while (!_globalCancellation.Token.IsCancellationRequested)
                {                  
                    var coins = await Coin.GetUnderSpotlight();
                    var waitTime = UPDATE_FREQUENCY / coins.Count;

                    Parallel.ForEach(coins, async (coin, state, index) =>
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(waitTime * index), _globalCancellation.Token);
                        var lastUpdateTime = await CryptoDataPoint.GetLastUpdateDate(coin.Id);
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
                        await CryptoDataPoint.BulkInsert(coin.Id, dataPoints);
                    });

                    await Task.Delay((int)UPDATE_FREQUENCY, _globalCancellation.Token);
                }
            }
            catch(Exception e)
            {

            }
        }

        public void Cancel()
		{
            _globalCancellation.Cancel();
		}

    }
}
