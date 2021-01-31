using CryptoAnalyzer.CoinGecko.DTO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CryptoAnalyzer.DTO.CoinGecko
{
	public class MarketChart
	{
		[JsonProperty("prices")]
		public List<DataPoint> Prices { get; set; }
	}
}
