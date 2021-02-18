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
            var coinsInThread = new Dictionary<int, HashSet<string>>();
            var repliesInThread = new Dictionary<int, int>();
            while (!_globalCancellation.Token.IsCancellationRequested)
            {
                try
                {
                    var possibleCoinsNewReplies = new Dictionary<string, int>();
                    var response = await _client.GetAsync(Context.ChanConfiguration.ApiBaseUrl);
                    response.EnsureSuccessStatusCode();
                    var threadList = JsonConvert.DeserializeObject<List<Catalog>>(await response.Content.ReadAsStringAsync())
                        .SelectMany(x => x.threads.Where(x => !string.IsNullOrEmpty(x.sub))).ToList();

                    foreach(var thread in threadList)
					{
                        coinsInThread.TryGetValue(thread.no, out var c);
                        if(c is null)
						{
                            c = new HashSet<string>();
                            foreach (Match ItemMatch in regex.Matches(thread.sub))
                            {
                                c.UnionWith(ItemMatch.Value.Split(" "));
                            }
                            coinsInThread[thread.no] = c;
                        }
                        repliesInThread.TryGetValue(thread.no, out var count);
                        repliesInThread[thread.no] = thread.replies;
                        var newReplies = thread.replies - count;
                        if (newReplies > 0)
                        {
                            foreach (var coinName in c)
                            {
                                var secureCoinName = coinName.Length > 50 ? coinName.Substring(0,50) : coinName;
                                possibleCoinsNewReplies.TryGetValue(secureCoinName, out var currentCount);
                                possibleCoinsNewReplies[secureCoinName] = currentCount + newReplies;
                            }
                        }
					}
                    await Coin.SetTalkedAbout(possibleCoinsNewReplies);
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
