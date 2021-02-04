using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace CryptoAnalyzer.CoinGecko.DTO
{
    public class Description
    {
        public string en { get; set; }
    }

    public class ReposUrl
    {
        public List<string> github { get; set; }
        public List<object> bitbucket { get; set; }
    }

    public class Links
    {
        public List<string> homepage { get; set; }
        public List<string> blockchain_site { get; set; }
        public List<string> official_forum_url { get; set; }
        public List<string> chat_url { get; set; }
        public List<string> announcement_url { get; set; }
        public string twitter_screen_name { get; set; }
        public string facebook_username { get; set; }
        public object bitcointalk_thread_identifier { get; set; }
        public string telegram_channel_identifier { get; set; }
        public string subreddit_url { get; set; }
        public ReposUrl repos_url { get; set; }
    }

    public class Image
    {
        public string thumb { get; set; }
        public string small { get; set; }
        public string large { get; set; }
    }

    public class PublicInterestStats
    {
        public int? alexa_rank { get; set; }
        public object bing_matches { get; set; }
    }

    public class CoinDetail
    {
        public string id { get; set; }
        public string symbol { get; set; }
        public string name { get; set; }
        public object asset_platform_id { get; set; }
        public int? block_time_in_minutes { get; set; }
        public string hashing_algorithm { get; set; }
        public List<string> categories { get; set; }
        public object public_notice { get; set; }
        public List<object> additional_notices { get; set; }
        public Description description { get; set; }
        public Links links { get; set; }
        public Image image { get; set; }
        public string country_origin { get; set; }
        public string genesis_date { get; set; }
        public double? sentiment_votes_up_percentage { get; set; }
        public double? sentiment_votes_down_percentage { get; set; }
        [JsonProperty("market_cap_rank")]
        public int? MarketCapRank { get; set; }
        public int? coingecko_rank { get; set; }
        public double? coingecko_score { get; set; }
        public double? developer_score { get; set; }
        public double? community_score { get; set; }
        public double? liquidity_score { get; set; }      
        [JsonProperty("public_interest_score")]
        public double? PublicInterestScore { get; set; }
        public PublicInterestStats public_interest_stats { get; set; }
        public List<object> status_updates { get; set; }
        public DateTime? last_updated { get; set; }
    }
}
