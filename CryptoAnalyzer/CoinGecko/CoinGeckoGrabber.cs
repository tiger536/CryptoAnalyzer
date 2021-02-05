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
    public class CoinGeckoGrabber
    {
        private string COIN_EXAMPLE = "bitcoin";
        private CancellationTokenSource _globalCancellation;
        private readonly IThrottledService _client;
        public CoinGeckoGrabber(IThrottledService client)
        {
            _globalCancellation = new CancellationTokenSource();
            _client = client;
        }

        public async Task GrabAsync()
        {
            try
            {
                var coin = await Coin.GetByCode(COIN_EXAMPLE);

                var lastUpdateTime = await CryptoDataPoint.GetLastUpdateDate(coin.Id);
                if (lastUpdateTime == null || (DateTimeOffset.UtcNow - lastUpdateTime > TimeSpan.FromDays(1)))
                    lastUpdateTime = DateTimeOffset.Now.AddMinutes(5).AddDays(-1);

                var querystringParam = new Dictionary<string, string>
                {
                    ["vs_currency"] = "usd",
                    ["from"] = lastUpdateTime.Value.ToUnixTimeSeconds().ToString(),
                    ["to"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
                };
                var coinDataByInterval = await _client.GetAsync<MarketChart>(QueryHelpers.AddQueryString($"coins/{COIN_EXAMPLE}/market_chart/range", querystringParam));
                var dataPoints = new List<CryptoDataPoint>();
                for(int i = 0; i < coinDataByInterval.TotalVolumes.Count; i++)
                {
                    var volume = coinDataByInterval.TotalVolumes[i];
                    var marketCap = coinDataByInterval.MarketCaps[i];
                    var price = coinDataByInterval.Prices[i];
                    if((volume.Date != marketCap.Date) || (marketCap.Date != price.Date))
                    {
                        throw new Exception("Data was not taken at the same time");
                    }

                    dataPoints.Add(new CryptoDataPoint()
                    {
                        LogDate = volume.Date,
                        Volume = volume.PointValue,
                        Price = price.PointValue,
                        MarketCap = marketCap.PointValue
                    });
                }
                await CryptoDataPoint.BulkInsert(coin.Id, dataPoints);
            }
            catch(Exception e)
            {

            }
            return;
        }

    }
}
