using CryptoAnalyzer.DTO.CoinGecko;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoAnalyzer.CoinGecko
{
	public class CoinGeckoGrabber
	{
		private string coin = "bitcoin";
		private CancellationTokenSource _globalCancellation;
		private HttpClient _client;
		public CoinGeckoGrabber(HttpClient client)
		{
			_globalCancellation = new CancellationTokenSource();
			_client = client;
		}

		public async Task GrabAsync()
		{

			try
			{
				// using Microsoft.AspNetCore.WebUtilities;
				var query = new Dictionary<string, string>
				{
					["vs_currency"] = "usd",
					["from"] = DateTimeOffset.Now.AddMinutes(-30).ToUnixTimeSeconds().ToString(),
					["to"] = DateTimeOffset.Now.ToUnixTimeSeconds().ToString(),
				};
				using (var response = await _client.GetAsync(QueryHelpers.AddQueryString("coins/bitcoin/market_chart/range", query)))
				{
					response.EnsureSuccessStatusCode();
					var ciao = await response.Content.ReadAsStringAsync();
					var responseBody = JsonConvert.DeserializeObject<MarketChart>(ciao);
				}
			}
			catch(Exception e)
			{

			}
			return;
		}
	}
}
