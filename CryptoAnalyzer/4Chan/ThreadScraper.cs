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
            var bizThreadsData = await ThreadData.GetFromDBAsync();
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
                        bizThreadsData.TryGetValue(thread.no, out var threadData);
                        if(threadData is null)
						{
                            threadData = new ThreadData();
                            foreach (Match ItemMatch in regex.Matches(thread.sub))
                            {
                                threadData.PossibleCoins.UnionWith(ItemMatch.Value.Split(" "));
                            }
                            bizThreadsData[thread.no] = threadData;
                        }

                        var lastRepliesCount = threadData.Replies;
                        threadData.LastUpdate = DateTimeOffset.FromUnixTimeSeconds(thread.LastModified);
                        threadData.Replies = thread.replies;
                        var newReplies = thread.replies - lastRepliesCount;
                        if (newReplies > 0)
                        {
                            foreach (var coinName in threadData.PossibleCoins)
                            {
                                var secureCoinName = coinName.Length > 50 ? coinName.Substring(0,50) : coinName;
                                possibleCoinsNewReplies.TryGetValue(secureCoinName, out var currentCount);
                                possibleCoinsNewReplies[secureCoinName] = currentCount + newReplies;
                            }
                        }
					}
                    await Coin.SetTalkedAboutAsync(possibleCoinsNewReplies);
                    await ThreadData.CleanAndStoreAsync(bizThreadsData);
                }
                catch (Exception e)
                {
                    e.LogNoContext();
                }
                await Task.Delay(TimeSpan.FromMinutes(5));
            }
        }

        public void Cancel()
        {
            _globalCancellation.Cancel();
        }
    }
}
