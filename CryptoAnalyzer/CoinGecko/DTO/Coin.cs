using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        public DateTimeOffset? DateAdded { get; set; }

        public bool IsUseless()
        => Code.Contains("token", StringComparison.InvariantCulture)
                || Code.Contains("crossover", StringComparison.InvariantCulture)
                || Name.Contains("USD")
                || Name.Contains("BTC")
				|| Name.Contains("ETH")
                || Name.Contains("Cash")
                || Name.Contains("yield", StringComparison.InvariantCulture)
                || Code.Contains("coinbase", StringComparison.InvariantCulture)
                || Code.Contains("adjusted", StringComparison.InvariantCulture)
                || (Name.Contains("ethereum", StringComparison.InvariantCulture) && !Name.Equals("ethereum", StringComparison.InvariantCulture))
                || (Name.Contains("bitcoin", StringComparison.InvariantCulture) && !Name.Equals("bitcoin", StringComparison.InvariantCulture));
        
        public override int GetHashCode()
        {
            return HashCode.Combine(Code, Symbol, Name);
        }

        public override bool Equals(object obj)
        {
            var coin = obj as Coin;

            if (coin == null)
            {
                return false;
            }
            if(coin.Code.Equals("hedpay"))
			{

			}
            return coin.Code.Equals(this.Code) && coin.Name.Equals(this.Name) && coin.Symbol.Equals(this.Symbol);
        }

        public static async Task<Coin> GetByCode(string code)
        {
            using (var conn = Context.OpenDatabaseConnection())
            {
                return await conn.QueryFirstOrDefaultAsync<Coin>(@"
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
SELECT
    Id,
    Code,
    Symbol,
    Name,
    DateAdded
FROM
    dbo.CryptoCurrency
WHERE Code = @code", new { code = new DbString() { IsAnsi = true, Value = code, Length = 50 } });
            }
        }

        public static async Task<HashSet<Coin>> GetAll()
        {
            using (var conn = Context.OpenDatabaseConnection())
            {
                return (await conn.QueryAsync<Coin>(@"
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
SELECT
    Id,
    Code,
    Symbol,
    Name,
    DateAdded
FROM
    dbo.CryptoCurrency")).ToHashSet();
            }
        }

        public static async Task<List<Coin>> GetUnderSpotlight()
        {
            using (var conn = Context.OpenDatabaseConnection())
            {
                return (await conn.QueryAsync<Coin>(@"
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
SELECT
    Id,
    Code,
    Symbol,
    Name,
    DateAdded
FROM
    dbo.CryptoCurrency
WHERE
    UnderSpotlight = 1")).AsList();
            }
        }

        public static async Task Insert(Coin coin)
        {
            using (var conn = Context.OpenDatabaseConnection())
            {
                await conn.ExecuteAsync(@"
INSERT INTO dbo.CryptoCurrency(Code, Symbol, Name, MarketCapRank, DateAdded)
VALUES
(@Code, @Symbol, @Name, @MarketCapRank, SYSDATETIMEOFFSET())", new
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
                        IsAnsi = false,
                        Length = 50
                    },
                    Name = new DbString()
                    {
                        Value = coin.Name,
                        IsAnsi = true,
                        Length = 100
                    },
                    coin.MarketCapRank
                });
            }
        }
    }
}
