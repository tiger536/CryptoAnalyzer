using CryptoAnalyzer.DTO.CoinGecko;
using CryptoAnalyzer.Service;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoAnalyzer.CoinGecko
{
	public class CoinGeckoGrabber
	{
		private string coin = "bitcoin";
		private CancellationTokenSource _globalCancellation;
		private IThrottledService _client;
		public CoinGeckoGrabber(IThrottledService client)
		{
			_globalCancellation = new CancellationTokenSource();
			_client = client;
		}

		public async Task GrabAsync()
		{
			try
			{
				var query = new Dictionary<string, string>
				{
					["vs_currency"] = "usd",
					["from"] = DateTimeOffset.Now.AddMinutes(-30).ToUnixTimeSeconds().ToString(),
					["to"] = DateTimeOffset.Now.ToUnixTimeSeconds().ToString(),
				};
				var response = await _client.GetAsync<MarketChart>(QueryHelpers.AddQueryString("coins/bitcoin/market_chart/range", query));

			}
			catch(Exception e)
			{

			}
			return;
		}
	}
}
