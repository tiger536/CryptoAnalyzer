using CryptoAnalyzer.Service;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoAnalyzer.CoinGecko
{
	public class CoinGeckoApi : IHostedService
	{
		private static IThrottledService _httpClient;
		private static CoinGeckoGrabber _grabber;
		private static CoinListGrabber _coinListGrabber;
		public CoinGeckoApi(IThrottledService client)
		{
			_httpClient = client;
			_grabber = new CoinGeckoGrabber(_httpClient);
			_coinListGrabber = new CoinListGrabber(_httpClient);
		}
		public Task StartAsync(CancellationToken cancellationToken)
		{		
			Task.Run(() => _grabber.GrabAsync());		
			Task.Run(() => _coinListGrabber.GrabAsync());

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			_coinListGrabber.Cancel();
			return Task.CompletedTask;
		}
	}
}
