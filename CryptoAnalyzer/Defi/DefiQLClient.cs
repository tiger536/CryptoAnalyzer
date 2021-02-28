using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoAnalyzer.Defi
{
	public class DefiQLClient
	{
		private readonly Dictionary<DefiExchange,GraphQLHttpClient> Exchanges;
		public DefiQLClient()
		{
			Exchanges = new Dictionary<DefiExchange, GraphQLHttpClient>()
			{
				{ DefiExchange.UniSwap, new GraphQLHttpClient(Context.DefiConfiguration.UniSwapGraphQLUrl, new NewtonsoftJsonSerializer()) }//,
				//{ DefiExchange.PancakeSwap, new GraphQLHttpClient(Context.DefiConfiguration.PancakeSwapGraphQLUrl, new NewtonsoftJsonSerializer()) }
			};
		}

		public async Task<List<Models.Pair>> GetNewestPairs()
		{
			var newPairs = new List<Models.Pair>();
			foreach (var client in Exchanges)
			{
				var graphQLResponse = await client.Value.SendQueryAsync<UniSwapData>(new GraphQLRequest { Query = @"
{
	pairs(first:100, orderBy:createdAtTimestamp, orderDirection:desc) {
		id
		token0Price
		token1Price
		createdAtTimestamp
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
	}
}" });
				if (graphQLResponse.Errors is null)
				{
					var onlyEthPair = graphQLResponse.Data.pairs.Where(x => x.token1.symbol.Equals("WETH") || x.token1.symbol.Equals("WBNB")).ToList();
					newPairs.AddRange(onlyEthPair.Select(x => new Models.Pair()
					{
						ExchangeId = client.Key,
						PairId = x.id,
						Token0Id = x.token0.id,
						Token0Code = x.token0.name,
						CreatedAt = DateTimeOffset.FromUnixTimeSeconds(x.createdAtTimestamp)
					}));
				}
			}
			return newPairs;
		}
	}
}
