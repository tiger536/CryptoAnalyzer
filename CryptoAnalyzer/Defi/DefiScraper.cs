using CryptoAnalyzer.Service;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using StackExchange.Exceptional;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoAnalyzer.Defi
{
    public class DefiScraper
    {
        private readonly CancellationTokenSource _globalCancellation;
        private readonly TelegramBot _telegramBot;
        private readonly DefiQLClient _defiQLClient;

        public DefiScraper(TelegramBot telegramBot, DefiQLClient defiQLClient)
        {
            _globalCancellation = new CancellationTokenSource();
            _telegramBot = telegramBot;
            _defiQLClient = defiQLClient;
        }

        public async Task ScrapAsync()
        {
            while (!_globalCancellation.Token.IsCancellationRequested)
            {
                try
                {
                    var allDbPairs = await Models.Pair.GetPairsAsync();

                    var newPairs = (await _defiQLClient.GetNewestPairs()).Where(x => !allDbPairs.Any(y => y.Token0Id.Equals(x.Token0Id))).ToList();
                    if (newPairs.Any())
                    {
                        newPairs.ForEach(async x =>
                        {
                            var clones = await Models.Pair.GetCloned(x.Token0Symbol);
                            await _telegramBot.SendMessageAsync(Context.TelegramBotConfiguration.NewDefiCoinChatID,
                            $"New coin on {x.ExchangeId} ({x.Token0Code}).\n{x.Url}\nhttps://etherscan.io/token/{x.Token0Id}\nDuplicates: {clones}", clones == 0);
                        });
                        await Models.Pair.StoreNewPairsAsync(newPairs);
                    }                  
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
