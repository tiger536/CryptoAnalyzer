using CryptoAnalyzer.CoinGecko.DTO;
using CryptoAnalyzer.Models;
using CryptoAnalyzer.Service;
using Microsoft.AspNetCore.WebUtilities;
using StackExchange.Exceptional;
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
        private readonly ThrottledService _client;
        private static Dictionary<string, string> _queryStringParam;
        public CoinListGrabber(ThrottledService client)
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
                    var dbCoins = await Coin.GetAllAsync();
                    var allCoins = await _client.GetAsync<List<GeckoCoin>>("coins/list");
                    var newCoins = allCoins.Select(x=> new Coin()
                    {
                        Code = x.Id,
                        Name = x.Name,
                        MarketCapRank = x.MarketCapRank,
                        Symbol = x.Symbol
                    }).Where(x => !dbCoins.Contains(x) && !x.IsUseless()).ToList();

                    foreach (var coin in newCoins)
                    {
                        var coinDetail = await _client.GetAsync<CoinDetail>(QueryHelpers.AddQueryString($"coins/{coin.Code}", _queryStringParam));
                        coin.MarketCapRank = coinDetail.MarketCapRank;
                        await Coin.InsertAsync(coin);                        
                    }
                }
                catch (Exception e)
                {
                    e.LogNoContext();
                }
                await Task.Delay(TimeSpan.FromHours(3));
            }
        }

        public void Cancel()
        {
            _globalCancellation.Cancel();
        }
    }
}
