using Newtonsoft.Json;

namespace CryptoAnalyzer.CoinGecko.DTO
{
    public class GeckoCoin
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("symbol")]
        public string Symbol { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("market_cap_rank")] 
        public int? MarketCapRank { get; set; }
        [JsonProperty("public_interest_score")]
        public double PublicInterestScore { get; set; }
    }
}
