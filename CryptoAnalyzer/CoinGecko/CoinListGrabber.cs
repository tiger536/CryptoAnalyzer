using CryptoAnalyzer.CoinGecko.DTO;
using CryptoAnalyzer.Service;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoAnalyzer.CoinGecko
{
    public class CoinListGrabber
    {
        private readonly CancellationTokenSource _globalCancellation;
        private readonly IThrottledService _client;
        private static Dictionary<string, string> _queryStringParam;
        public CoinListGrabber(IThrottledService client)
        {
            _globalCancellation = new CancellationTokenSource();
            _client = client;

            _queryStringParam = new Dictionary<string, string>
            {
                ["localization"] = "false",
                ["tickers"] = "false",
                ["market_data"] = "false",
                ["community_data"] = "false",
                ["developer_data"] = "false"
            };
        }

        public async Task GrabAsync()
        {
            while (!_globalCancellation.Token.IsCancellationRequested)
            {
                try
                {
                    var dbCoins = await Coin.GetAll();
                    var allCoins = (await _client.GetAsync<List<Coin>>("coins/list"))
                        .Where(x => !x.Code.Contains("token", StringComparison.InvariantCulture) && !x.Code.Contains("crossover", StringComparison.InvariantCulture));
                    var newCoins = allCoins.Where(x => !dbCoins.Contains(x)).ToList();

                    foreach (var coin in newCoins)
                    {
                        var coinDetail = await _client.GetAsync<CoinDetail>(QueryHelpers.AddQueryString($"coins/{coin.Code}", _queryStringParam));
                        coin.MarketCapRank = coinDetail.MarketCapRank;
                        await Coin.Insert(coin);                        
                    }

                }
                catch (Exception e)
                {

                }
                await Task.Delay(TimeSpan.FromDays(1));
            }
        }

        public void Cancel()
        {
            _globalCancellation.Cancel();
        }
    }
}
