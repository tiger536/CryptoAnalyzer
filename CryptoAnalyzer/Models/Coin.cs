﻿using Dapper;
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
        public DateTimeOffset? LastTalkedAbout { get; set; }
        public bool UnderSpotlight { get; set; }
        public bool Ignore { get; set; }

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
SET UnderSpotlight = @spotlight
WHERE Id = @coinID", new { spotlight, coinID });
			}
		}

        public static async Task SetIgnored(bool ignored, int coinID)
        {
            using (var conn = Context.OpenDatabaseConnection())
            {
                await conn.ExecuteAsync(@"
UPDATE dbo.CryptoCurrency
SET Ignore = @ignored
WHERE Id = @coinID", new { ignored, coinID });
            }
        }

        public static async Task SetTalkedAbout(HashSet<string> keywords)
        {
            using (var conn = Context.OpenDatabaseConnection())
            {
                await conn.ExecuteAsync(@"
DECLARE @Id TABLE(CoinID INT)

INSERT INTO @id (CoinID)
SELECT
	Id
FROM
	dbo.CryptoCurrency WITH (NOLOCK)
WHERE 
	Code IN @words
	OR Symbol IN @words

UPDATE
	C
SET
	LastTalkedAbout = SYSDATETIMEOFFSET()
FROM
	dbo.CryptoCurrency C
	INNER JOIN @Id I ON I.CoinID = C.Id",
                new 
                { 
                    words = keywords.Select(x => new DbString()
                    {
                        Value = x,
                        IsAnsi = true,
                        Length = 25
                    }) 
                });
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
    UnderSpotlight,
    LastTalkedAbout,
    Ignore
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
    Ignore
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
    Ignore
FROM
    dbo.CryptoCurrency
WHERE
    Ignore = 0
    AND (DateAdded >= @from
    OR UnderSpotlight = 1
    OR LastTalkedAbout >= @from)", new { from })).AsList();
            }
        }

        public static async Task InsertAsync(Coin coin)
        {
            using (var conn = Context.OpenDatabaseConnection())
            {
                await conn.ExecuteAsync(@"
INSERT INTO dbo.CryptoCurrency(Code, Symbol, Name, MarketCapRank, DateAdded, LastTalkedAbout, Ignore)
VALUES
(@Code, @Symbol, @Name, @MarketCapRank, SYSDATETIMEOFFSET(), NULL, 0)", new
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
