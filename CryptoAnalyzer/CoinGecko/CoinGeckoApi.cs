using CryptoAnalyzer.Service;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoAnalyzer.CoinGecko
{
	public class CoinGeckoApi : IHostedService
	{
		private static IThrottledService _httpClient;
		public CoinGeckoApi(IThrottledService client)
		{
			_httpClient = client;
		}
		public Task StartAsync(CancellationToken cancellationToken)
		{
			var grabber = new CoinGeckoGrabber(_httpClient);
			Task.Run(() => grabber.GrabAsync());

			var coinListGrabber = new CoinListGrabber(_httpClient);
			Task.Run(() => coinListGrabber.GrabAsync());

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
