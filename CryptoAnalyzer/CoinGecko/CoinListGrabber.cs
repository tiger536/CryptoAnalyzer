using CryptoAnalyzer.CoinGecko.DTO;
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
	public class CoinListGrabber
	{
		private CancellationTokenSource _globalCancellation;
		private IThrottledService _client;
		public CoinListGrabber(IThrottledService client)
		{
			_globalCancellation = new CancellationTokenSource();
			_client = client;
		}

		public async Task GrabAsync()
		{
			while (!_globalCancellation.Token.IsCancellationRequested)
			{
				try
				{
					var newCoins = new List<Coin>();
					using (var conn = Context.OpenDatabaseConnection())
					{
						var dbCoins = (await conn.QueryAsync<Coin>(@"
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
SELECT
	Code,
	Symbol,
	Name
FROM
	dbo.CryptoCurrency")).ToHashSet();
						var allCoins = (await _client.GetAsync<List<Coin>>("coins/list")).Where(x => !x.Code.Contains("token", StringComparison.InvariantCulture));
						newCoins = allCoins.Where(x => !dbCoins.Contains(x)).ToList();
					}

					var query = new Dictionary<string, string>
					{
						["localization"] = "false",
						["tickers"] = "false",
						["market_data"] = "false",
						["community_data"] = "false",
						["developer_data"] = "false"
					};

					foreach (var coin in newCoins)
					{
						var coinDetail = await _client.GetAsync<CoinDetail>(QueryHelpers.AddQueryString($"coins/{coin.Code}", query));
						using (var conn = Context.OpenDatabaseConnection())
						{
							await conn.ExecuteAsync(@"
INSERT INTO dbo.CryptoCurrency(Code, Symbol, Name, MarketCapRank)
VALUES
(@Code, @Symbol, @Name, @MarketCapRank)", new
							{
								Code = new DbString()
								{
									Value = coin.Code,
									IsAnsi = true,
									Length = 50
								},
								Symbol = new DbString()
								{
									Value = coin.Symbol,
									IsAnsi = true,
									Length = 10
								},
								Name = new DbString()
								{
									Value = coin.Name,
									IsAnsi = true,
									Length = 100
								},
								coinDetail.MarketCapRank
							});
						}
					}

				}
				catch (Exception e)
				{

				}
				await Task.Delay(TimeSpan.FromDays(1));
			}
		}

		public void Cancel()
		{
			_globalCancellation.Cancel();
		}
	}
}
