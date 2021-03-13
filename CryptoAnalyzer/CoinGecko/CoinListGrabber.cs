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
        private readonly ThrottledHttpClient _client;
        private readonly TelegramBot _telegramBot;
        private static Dictionary<string, string> _queryStringParam;
        public CoinListGrabber(ThrottledHttpClient client, TelegramBot telegramBot)
        {
            _globalCancellation = new CancellationTokenSource();
            _client = client;
            _telegramBot = telegramBot;
            _queryStringParam = new Dictionary<string, string>
            {
                ["localization"] = "false",
                ["tickers"] = "true",
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
                    }).Where(x => !dbCoins.Contains(x) && !x.IsUseless() && !string.IsNullOrEmpty(x.Code)).ToList();

                    foreach (var coin in newCoins)
                    {
                        var coinDetail = await _client.GetAsync<CoinDetail>(QueryHelpers.AddQueryString($"coins/{coin.Code}", _queryStringParam));
                        coin.MarketCapRank = coinDetail?.MarketCapRank;
                        await Coin.InsertAsync(coin);
                        //await _telegramBot.SendMessageAsync(Context.TelegramBotConfiguration.ConversationID,
                        //    $"New Coin: {coin.Code}({coin.Name})\n Available at: {string.Join(", ",coinDetail.tickers.Select(x => x.market.name))}");
                    }
                }
                catch (Exception e)
                {
                    e.LogNoContext();
                }
                await Task.Delay(TimeSpan.FromMinutes(10));
            }
        }

        public void Cancel()
        {
            _globalCancellation.Cancel();
        }
    }
}
