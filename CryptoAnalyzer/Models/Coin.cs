using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoAnalyzer.Models
{
	public class Coin
	{
        public int Id { get; set; }
        public string Code { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public int? MarketCapRank { get; set; }
        public DateTimeOffset? DateAdded { get; set; }
        public bool UnderSpotlight { get; set; }

        public bool IsUseless()
        => Code.Contains("token", StringComparison.InvariantCultureIgnoreCase)
            || Code.Contains("crossover", StringComparison.InvariantCultureIgnoreCase)
            || Name.Contains("USD")
            || Name.Contains("BTC")
            || Name.Contains("ETH")
            || Name.Contains("Cash")
            || Name.Contains("yield", StringComparison.InvariantCultureIgnoreCase)
            || Name.Contains("dollar", StringComparison.InvariantCultureIgnoreCase)
            || Code.Contains("coinbase", StringComparison.InvariantCultureIgnoreCase)
            || Code.Contains("adjusted", StringComparison.InvariantCultureIgnoreCase)
            || (Name.Contains("ether", StringComparison.InvariantCultureIgnoreCase) && !Name.Equals("ethereum", StringComparison.InvariantCultureIgnoreCase))
            || (Name.Contains("bitcoin", StringComparison.InvariantCultureIgnoreCase) && !Name.Equals("bitcoin", StringComparison.InvariantCultureIgnoreCase));

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

            return coin.Code.Equals(this.Code) && coin.Name.Equals(this.Name) && coin.Symbol.Equals(this.Symbol);
        }

        public static async Task SetSpotlight(bool spotlight, int coinID)
		{
            using(var conn = Context.OpenDatabaseConnection())
			{
                await conn.ExecuteAsync(@"
UPDATE dbo.CryptoCurrency
SET UnderSpotlight =@spotlight
WHERE Id = @coinID", new { spotlight, coinID });
			}
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
    DateAdded,
    UnderSpotlight
FROM
    dbo.CryptoCurrency
WHERE Code = @code", new { code = new DbString() { IsAnsi = true, Value = code, Length = 50 } });
            }
        }

        public static async Task<HashSet<Coin>> GetAllAsync()
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
    DateAdded,
    UnderSpotlight
FROM
    dbo.CryptoCurrency")).ToHashSet();
            }
        }

        public static async Task<List<Coin>> GetImportantCoinsAsync(DateTimeOffset from)
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
    DateAdded,
    UnderSpotlight
FROM
    dbo.CryptoCurrency
WHERE
    DateAdded > @from
    OR UnderSpotlight = 1", new { from })).AsList();
            }
        }

        public static async Task InsertAsync(Coin coin)
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
