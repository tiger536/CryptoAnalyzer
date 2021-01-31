using Microsoft.Extensions.Hosting;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoAnalyzer.CoinGecko
{
	public class CoinGeckoApi : IHostedService
	{
		private static HttpClient _httpClient;
		public CoinGeckoApi(IHttpClientFactory clientFactory)
		{
			_httpClient = clientFactory.CreateClient("CoinGecko");
		}
		public Task StartAsync(CancellationToken cancellationToken)
		{
			var grabber = new CoinGeckoGrabber(_httpClient);
			Task.Run(() => grabber.GrabAsync());

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
