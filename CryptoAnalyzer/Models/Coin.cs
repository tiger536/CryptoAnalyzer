using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
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
        public DateTimeOffset DateAdded { get; set; }
        public DateTimeOffset? LastTalkedAbout { get; set; }
        public bool UnderSpotlight { get; set; }
        public bool Ignore { get; set; }
        public bool FastRefresh { get; set; }
        public int Hits { get; set; }
        public string Details { get; set; }

        public bool IsUseless()
        => Code.Contains("token", StringComparison.InvariantCultureIgnoreCase)
            || Code.Contains("crossover", StringComparison.InvariantCultureIgnoreCase)
            || Name.Contains("USD")
            || Name.Contains("BTC")
            || Name.Contains("ETH")
            || Name.Contains("[Old")
            || Name.Contains("(Old")
            || Name.Contains("Cash")
            || Name.Contains("erc20", StringComparison.InvariantCultureIgnoreCase)         
            || Name.Contains("yield", StringComparison.InvariantCultureIgnoreCase)
            || Name.Contains("synthetic", StringComparison.InvariantCultureIgnoreCase)
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

        public static async Task SetSpotlightAsync(bool spotlight, int coinID)
		{
            using(var conn = Context.OpenDatabaseConnection())
			{
                await conn.ExecuteAsync(@"
UPDATE dbo.CryptoCurrency
SET UnderSpotlight = @spotlight
WHERE Id = @coinID", new { spotlight, coinID });
			}
		}

        public static async Task SetIgnoredAsync(bool ignored, int coinID)
        {
            using (var conn = Context.OpenDatabaseConnection())
            {
                await conn.ExecuteAsync(@"
UPDATE dbo.CryptoCurrency
SET Ignore = @ignored
WHERE Id = @coinID", new { ignored, coinID });
            }
        }

        public static async Task SetFastRefreshAsync(bool fastRefresh, int coinID)
        {
            using (var conn = Context.OpenDatabaseConnection())
            {
                await conn.ExecuteAsync(@"
UPDATE dbo.CryptoCurrency
SET FastRefresh = @fastRefresh
WHERE Id = @coinID", new { fastRefresh, coinID });
            }
        }

        public static async Task SetNotesAsync(string notes, int coinID)
        {
            using (var conn = Context.OpenDatabaseConnection())
            {
                await conn.ExecuteAsync(@"
UPDATE dbo.CryptoCurrency
SET Details = @data
WHERE Id = @coinID", new { coinID, data = new DbString() {IsAnsi = false ,Value = notes} });
            }
        }

        public static async Task SetTalkedAboutAsync(Dictionary<string,int> possibleCoinsNewReplies)
        {
            var dt = new DataTable();
            dt.Columns.Add("Code", typeof(string));
            dt.Columns.Add("NewReplies", typeof(int));
            foreach(var val in possibleCoinsNewReplies)
			{
                var row = dt.NewRow();
                row["Code"] = val.Key;
                row["NewReplies"] = val.Value;
                dt.Rows.Add(row);
            }

            using (var conn = Context.OpenDatabaseConnection())
            {
                await conn.ExecuteAsync(@"
DECLARE @Id TABLE (CoinID INT, Replies INT)

INSERT INTO @id (CoinID, Replies)
SELECT
	C.Id,
    P.NewReplies
FROM
	dbo.CryptoCurrency C WITH (NOLOCK)
    INNER JOIN @possibleCoins P ON P.Code = C.Code

INSERT INTO @id (CoinID, Replies)
SELECT
	C.Id,
    P.NewReplies
FROM
	dbo.CryptoCurrency C WITH (NOLOCK)
    INNER JOIN @possibleCoins P ON P.Code = C.Symbol
    LEFT OUTER JOIN @id I ON I.CoinID = C.Id
WHERE
    I.CoinID IS NULL

UPDATE
	C
SET
	C.LastTalkedAbout = SYSDATETIMEOFFSET(),
    C.Hits = C.Hits + I.Replies
FROM
	dbo.CryptoCurrency C
	INNER JOIN @Id I ON I.CoinID = C.Id",
                new 
                { 
                    possibleCoins = dt.AsTableValuedParameter("dbo.TVP_PossibleCoins")
                });
            }
            dt.Dispose();
        }

        public static async Task<Coin> GetByCodeAsync(string code)
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
    UnderSpotlight,
    LastTalkedAbout,
    Ignore,
    Hits,
    FastRefresh,
    Details
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
    UnderSpotlight,
    LastTalkedAbout,
    Ignore,
    Hits,
    FastRefresh,
    Details
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
    UnderSpotlight,
    LastTalkedAbout,
    Ignore,
    Hits,
    FastRefresh,
    Details
FROM
    dbo.CryptoCurrency
WHERE
    Ignore = 0
    AND (DateAdded >= @from
    OR UnderSpotlight = 1)", new { from })).AsList();
            }
        }

        public static async Task<List<Coin>> GetFastRefreshCoinsAsync()
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
    UnderSpotlight,
    LastTalkedAbout,
    Ignore,
    Hits,
    FastRefresh,
    Details
FROM
    dbo.CryptoCurrency
WHERE
    FastRefresh = 1")).AsList();
            }
        }

        public static async Task<List<Coin>> GetInternetBuzzCoinsAsync(DateTimeOffset from)
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
    UnderSpotlight,
    LastTalkedAbout,
    Ignore,
    Hits,
    FastRefresh
    Details
FROM
    dbo.CryptoCurrency
WHERE
    Ignore = 0
    AND UnderSpotlight = 0
    AND LastTalkedAbout >= @from
    AND DateAdded <= @from", new { from })).AsList();
            }
        }

        public static async Task InsertAsync(Coin coin)
        {
            using (var conn = Context.OpenDatabaseConnection())
            {
                await conn.ExecuteAsync(@"
INSERT INTO dbo.CryptoCurrency(Code, Symbol, Name, MarketCapRank, DateAdded, LastTalkedAbout, Ignore, Hits, FastRefresh, Details)
VALUES
(@Code, @Symbol, @Name, @MarketCapRank, SYSDATETIMEOFFSET(), NULL, 0, 0, 0, NULL)", new
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
