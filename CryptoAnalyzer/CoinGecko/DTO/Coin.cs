using Dapper;
using Newtonsoft.Json;
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

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Code, Symbol, Name);
        }

        public override bool Equals(object obj)
        {
            Coin coin = obj as Coin;

            if (coin == null)
            {
                return false;
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
    Name
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
    Name
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
    Name
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
                    coin.MarketCapRank
                });
            }
        }
    }
}
