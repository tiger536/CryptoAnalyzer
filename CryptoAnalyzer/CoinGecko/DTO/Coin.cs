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
		public int? MarketCapRank { get; set; }
		[JsonProperty("public_interest_score")]
		public double PublicInterestScore { get; set; }

		public override int GetHashCode()
		{
			return System.HashCode.Combine(Code, Symbol, Name);
		}

		public override bool Equals(object obj)
		{
			Coin fooItem = obj as Coin;

			if (fooItem == null)
			{
				return false;
			}

			return fooItem.Code.Equals(this.Code) && fooItem.Name.Equals(this.Name) && fooItem.Symbol.Equals(this.Symbol);
		}
	}
}
