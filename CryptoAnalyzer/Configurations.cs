namespace CryptoAnalyzer
{
	public class TelegramConfiguration
	{
		public string APIKey { get; set; }
		public long ConversationID { get; set; }
		public long NewDefiCoinChatID { get; set; }
	}

	public class DefiConfiguration
	{
		public string UniSwapBaseUrl { get; set; }
		public string PancakeSwapBaseUrl { get; set; }
		public string UniSwapGraphQLUrl { get; set; }
		public string PancakeSwapGraphQLUrl { get; set; }
	}

	public class CoinMarketCapConfiguration
	{
		public string ApiBaseUrl { get; set; }
		public string ApiKey { get; set; }
	}

	public class CoinGeckoConfiguration
	{
		public string ApiBaseUrl { get; set; }
	}

	public class ChanConfiguration
	{
		public string ApiBaseUrl { get; set; }
	}
}
