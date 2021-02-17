using CryptoAnalyzer.Models;
using Newtonsoft.Json;
using StackExchange.Exceptional;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoAnalyzer.Chan
{
    public class ThreadScraper
    {
        private readonly CancellationTokenSource _globalCancellation;
        private readonly HttpClient _client;
        private readonly Regex regex = new Regex("(\\b[A-Z]{2,}(?:\\s+[A-Z]+)*\\b)|(^([\\w\\-]+))"); //capital words or first word
        
        public ThreadScraper(HttpClient client)
        {
            _globalCancellation = new CancellationTokenSource();
            _client = client;
        }

        public async Task ScrapAsync()
        {
            while (!_globalCancellation.Token.IsCancellationRequested)
            {
                try
                {
                    var possibleCoins = new HashSet<string>();
                    var response = await _client.GetAsync(Context.ChanConfiguration.ApiBaseUrl);
                    response.EnsureSuccessStatusCode();
                    var threadList = JsonConvert.DeserializeObject<List<Catalog>>(await response.Content.ReadAsStringAsync());
                    var threadNames = threadList.SelectMany(x => x.threads.Where(x => !string.IsNullOrEmpty(x.sub)).Select(x => x.sub)).ToList();
                    foreach (var threadName in threadNames)
                    {
                        foreach (Match ItemMatch in regex.Matches(threadName))
                        {
                            possibleCoins.UnionWith(ItemMatch.Value.Split(" "));
                        }
                    }
                    await Coin.SetTalkedAbout(possibleCoins);
                }
                catch (Exception e)
                {
                    e.LogNoContext();
                }
                await Task.Delay(TimeSpan.FromMinutes(30));
            }
        }

        public void Cancel()
        {
            _globalCancellation.Cancel();
        }
    }
}
