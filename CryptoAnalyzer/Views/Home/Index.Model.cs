using CryptoAnalyzer.CoinGecko.DTO;
using System.Collections.Generic;

namespace CryptoAnalyzer.Models
{
	public class IndexViewModel
	{
		public List<Coin> Spotlight { get; set; }
		public List<Coin> Newest { get; set; }
	}
}
