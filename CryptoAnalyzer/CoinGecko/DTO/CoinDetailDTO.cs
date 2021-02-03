using System;

namespace CryptoAnalyzer.CoinGecko.DTO
{
	public class CoinDetailDTO
	{
		public DateTimeOffset LogDate { get; set; }
		public decimal Volume { get; set; }
		public decimal Price { get; set; }
		public decimal MarketCap { get; set; }
	}
}
