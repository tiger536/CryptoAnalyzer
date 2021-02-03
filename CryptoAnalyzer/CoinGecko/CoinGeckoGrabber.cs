using CryptoAnalyzer.CoinGecko.DTO;
using CryptoAnalyzer.DTO.CoinGecko;
using CryptoAnalyzer.Service;
using Dapper;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoAnalyzer.CoinGecko
{
	public class CoinGeckoGrabber
	{
		private string COIN_EXAMPLE = "bitcoin";
		private CancellationTokenSource _globalCancellation;
		private IThrottledService _client;
		public CoinGeckoGrabber(IThrottledService client)
		{
			_globalCancellation = new CancellationTokenSource();
			_client = client;
		}

		public async Task GrabAsync()
		{
			try
			{
				Coin coin;
				DateTimeOffset? lastUpdateTime;
				using( var conn = Context.OpenDatabaseConnection())
				{
					coin = await conn.QueryFirstOrDefaultAsync<Coin>(@"
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
SELECT
	Id,
	Code,
	Symbol,
	Name
FROM
	dbo.CryptoCurrency
WHERE Code = @code", new { code = COIN_EXAMPLE });

					lastUpdateTime = await conn.QuerySingleOrDefaultAsync<DateTimeOffset?>(@"
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
SELECT TOP 1 LogDate FROM dbo.CryptoDetails WHERE CoinId = @Id ORDER BY Id DESC", new { coin.Id });
					if (lastUpdateTime == null || (DateTimeOffset.UtcNow - lastUpdateTime > TimeSpan.FromDays(1)))
						lastUpdateTime = DateTimeOffset.Now.AddMinutes(5).AddDays(-1);
				}
				var querystringParam = new Dictionary<string, string>
				{
					["vs_currency"] = "usd",
					["from"] = lastUpdateTime.Value.ToUnixTimeSeconds().ToString(),
					["to"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
				};
				var response = await _client.GetAsync<MarketChart>(QueryHelpers.AddQueryString("coins/bitcoin/market_chart/range", querystringParam));
				var dataPointList = new List<CoinDetailDTO>();
				for(int i = 0; i < response.TotalVolumes.Count; i++)
				{
					var volume = response.TotalVolumes[i];
					var marketCap = response.MarketCaps[i];
					var price = response.Prices[i];
					if((volume.Date != marketCap.Date) && (marketCap.Date != price.Date))
					{
						throw new Exception("data not the same");
					}

					dataPointList.Add(new CoinDetailDTO()
					{
						LogDate = volume.Date,
						Volume = volume.PointValue,
						Price = price.PointValue,
						MarketCap = marketCap.PointValue
					});
				}
				using (var connection = Context.OpenDatabaseConnection())
				{
					foreach(var data in dataPointList)
					{
						connection.Execute(@"
INSERT INTO dbo.CryptoDetails (CoinId, LogDate, Volume, Price, MarketCap)
VALUES
(@CoinId, @LogDate, @Volume, @Price, @MarketCap)", new
						{
							CoinId = coin.Id,
							LogDate = data.LogDate,
							data.Volume,
							data.Price,
							data.MarketCap
						});
					}
				}

			}
			catch(Exception e)
			{

			}
			return;
		}
	}
}
