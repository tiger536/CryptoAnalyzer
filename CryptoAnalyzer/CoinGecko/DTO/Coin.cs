using Newtonsoft.Json;

namespace CryptoAnalyzer.CoinGecko.DTO
{
	public class Coin
	{
		public int Id { get; set; }
		[JsonProperty("id")]
		public string Code { get; set; }
		[JsonProperty("symbol")]
		public string Symbol { get; set; }
		[JsonProperty("name")]
		public string Name { get; set; }
		[JsonProperty("market_cap_rank")] 
		public int MarketCapRank { get; set; }
	}
}
