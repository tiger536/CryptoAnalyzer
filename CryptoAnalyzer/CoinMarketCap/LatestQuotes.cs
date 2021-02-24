using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CryptoAnalyzer.CoinMarketCap
{
    public class MarketData
    {
        [JsonProperty("price")]
        public double Price { get; set; }
        [JsonProperty("volume_24h")]
        public double Volume24h { get; set; }
        public double percent_change_1h { get; set; }
        public double percent_change_24h { get; set; }
        public double percent_change_7d { get; set; }
        public double percent_change_30d { get; set; }
        [JsonProperty("market_cap")]
        public double MarketCap { get; set; }
        [JsonProperty("last_updated")]
        public DateTime LastUpdated { get; set; }
    }

    public class Data
    {
        public int id { get; set; }
        public string name { get; set; }
        public string symbol { get; set; }
        public string slug { get; set; }
        public int is_active { get; set; }
        public int is_fiat { get; set; }
        public int circulating_supply { get; set; }
        public int total_supply { get; set; }
        public int max_supply { get; set; }
        public DateTime date_added { get; set; }
        public int num_market_pairs { get; set; }
        public int cmc_rank { get; set; }
        public DateTime last_updated { get; set; }
        public List<string> tags { get; set; }
        public object platform { get; set; }
        [JsonProperty("quote")]
        public Dictionary<string, MarketData> Quote {get;set;}
    }

    public class Status
    {
        public DateTime timestamp { get; set; }
        public int error_code { get; set; }
        public string error_message { get; set; }
        public int elapsed { get; set; }
        public int credit_count { get; set; }
    }

    public class LatestQuotes
    {
        [JsonProperty("data")]
        public Dictionary<string,Data> Data { get; set; }
        [JsonProperty("status")]
        public Status Status { get; set; }
    }
}
