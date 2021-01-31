using CryptoAnalyzer.CoinGecko;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace CryptoAnalyzer
{	
	public static class Context
	{
		public static CoinGeckoConfiguration CoinGeckoConfiguration { get; private set; }

		public static void Initialize(IConfiguration configuration, IWebHostEnvironment environment)
		{
			CoinGeckoConfiguration = configuration.GetSection("CoinGecko").Get<CoinGeckoConfiguration>();
		}
	}
}
