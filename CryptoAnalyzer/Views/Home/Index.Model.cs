using System.Collections.Generic;

namespace CryptoAnalyzer.Models
{
	public class IndexViewModel
	{
		public int NewCoinThresold { get; set; }
		public List<Coin> Coins { get; set; }
		public List<Coin> InternetBuzzCoins { get; set; }
		public List<Pair> LatestDefiCoins { get; set; }
	}
}
