using CryptoAnalyzer.Defi;
using Dapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoAnalyzer.Models
{
	public class Pair
	{
		public int Id { get; set; }
		public DefiExchange ExchangeId { get; set; }
		public string PairId { get; set; }
		public string Token0Id { get; set; }
		public string Token0Code { get; set; }
        public string Token0Symbol { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
        public string Url => $"{(ExchangeId == DefiExchange.UniSwap ? Context.DefiConfiguration.UniSwapBaseUrl : Context.DefiConfiguration.PancakeSwapBaseUrl)}{Token0Id}";

		public static async Task<List<Pair>>GetPairsAsync()
		{
            using (var conn = Context.OpenDatabaseConnection())
            {
                return (await conn.QueryAsync<Pair>(@"
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
SELECT
    Id,
    ExchangeId,
    PairId,
    Token0Id,
    Token0Code,
    Token0Symbol,
    CreatedAt
FROM
    dbo.Pairs")).AsList();
            }
        }

        public static async Task<List<Pair>> GetLatest(DateTimeOffset from)
        {
            using (var conn = Context.OpenDatabaseConnection())
            {
                return (await conn.QueryAsync<Pair>(@"
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
SELECT
    Id,
    ExchangeId,
    PairId,
    Token0Id,
    Token0Code,
    Token0Symbol,
    CreatedAt
FROM
    dbo.Pairs
WHERE
    CreatedAt >= @from", new { from })).AsList();
            }
        }

        public static async Task<int>GetCloned(string tokenSymbol)
		{
            using (var conn = Context.OpenDatabaseConnection())
            {
                return await conn.ExecuteScalarAsync<int>(@"
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
DECLARE @symbol VARCHAR(50) = @tokenSymbol

SELECT COUNT(*)
FROM
	dbo.Pairs
WHERE
	Token0Symbol = @symbol", new DbString() { IsAnsi = true, Length = 50, Value = tokenSymbol });
            }
        }

        public static async Task StoreNewPairsAsync(List<Pair> pairs)
        {
            using (var conn = Context.OpenDatabaseConnection())
            {
                foreach(var pair in pairs)
				{
                   await conn.ExecuteAsync(@"
INSERT INTO dbo.Pairs(ExchangeId, PairId, Token0Id, Token0Code, CreatedAt, Token0Symbol)
VALUES (@ExchangeId,@PairId,@Token0Id,@Token0Code,@CreatedAt,@Token0Symbol)",
                    new 
                    { 
                        pair.ExchangeId,
                        PairId = new DbString() { IsAnsi = true, Length = 50, Value = pair.PairId },
                        Token0Id = new DbString() { IsAnsi = true, Length = 50, Value = pair.Token0Id },
                        Token0Code = new DbString() { IsAnsi = true, Length = 50, Value = pair.Token0Code },
                        Token0Symbol = new DbString() { IsAnsi = true, Length = 50, Value = pair.Token0Symbol },
                        pair.CreatedAt
                    });
				}
            }
        }
    }
}
