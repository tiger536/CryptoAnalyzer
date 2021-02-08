using CryptoAnalyzer.CoinGecko.DTO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CryptoAnalyzer.DTO.CoinGecko
{
    public class MarketChart
    {
        [JsonProperty("prices")]
        public List<DataPoint> Prices { get; set; }
        [JsonProperty("market_caps")]
        public List<DataPoint> MarketCaps { get; set; }
        [JsonProperty("total_volumes")]
        public List<DataPoint> TotalVolumes { get; set; }

        public bool HasDifferentSizes => Prices.Count != MarketCaps.Count || TotalVolumes.Count != MarketCaps.Count;
    }
}
