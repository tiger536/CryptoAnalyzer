using CryptoAnalyzer.CoinGecko.DTO;
using CryptoAnalyzer.Service;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoAnalyzer.CoinGecko
{
	public class CoinListGrabber
	{
		private CancellationTokenSource _globalCancellation;
		private IThrottledService _client;
		public CoinListGrabber(IThrottledService client)
		{
			_globalCancellation = new CancellationTokenSource();
			_client = client;
		}

		public async Task GrabAsync()
		{
			try
			{
				var response = await _client.GetAsync<List<Coin>>("coins/list");
			}
			catch (Exception e)
			{

			}
			return;
		}
	}
}
