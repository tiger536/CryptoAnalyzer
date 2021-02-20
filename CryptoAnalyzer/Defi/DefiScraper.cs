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
        private readonly GraphQLHttpClient _uniswapQLClient;
        private readonly TelegramBot _telegramBot;
        private GraphQLRequest pairs = new GraphQLRequest { Query = @"
{

pairs(first:50 orderBy:createdAtTimestamp, orderDirection:desc) {
  id
  token0Price
  token1Price
  token0 {
    name
    id
    symbol
    txCount
  }
  token1 {
    name
    id
    symbol
    txCount
  }
  createdAtTimestamp
 }
}" };
        public DefiScraper(TelegramBot telegramBot)
        {
            _globalCancellation = new CancellationTokenSource();
            _telegramBot = telegramBot;
            _uniswapQLClient = new GraphQLHttpClient(Context.DefiConfiguration.UniswapGraphQLUrl, new NewtonsoftJsonSerializer());
        }

        public async Task ScrapAsync()
        {
            while (!_globalCancellation.Token.IsCancellationRequested)
            {
                try
                {
                    var allDbPairs = await Models.Pair.GetPairsAsync();
                    var graphQLResponse = await _uniswapQLClient.SendQueryAsync<UniswapData>(pairs);
                    if (graphQLResponse.Errors is null)
                    {
                        var onlyEthPair = graphQLResponse.Data.pairs.Where(x => x.token1.symbol.Equals("WETH")).ToList();
                        var newPairs = onlyEthPair.Where(x => !allDbPairs.Any(y => y.Token0Id.Equals(x.token0.id))).Select(x=> new Models.Pair()
                        {
                            ExchangeId = DefiExchange.UniSwap,
                            PairId = x.id,
                            Token0Id = x.token0.id,
                            Token0Code = x.token0.name,
                            CreatedAt = DateTimeOffset.FromUnixTimeSeconds(x.createdAtTimestamp)
                        }).ToList();
                        if(newPairs.Any())
                        {
                            newPairs.ForEach(async x => await _telegramBot.SendMessageAsync(Context.TelegramBotConfiguration.NewDefiCoinChatID,
                                $"New coin on Uniswap ({x.Token0Code}).\n{Context.DefiConfiguration.UniswapBaseUrl}{x.Token0Id}"));
                            await Models.Pair.StoreNewPairsAsync(newPairs);
                        }                      
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
