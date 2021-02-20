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
		public DateTimeOffset CreatedAt { get; set; }
        public string Url => $"{Context.DefiConfiguration.UniswapBaseUrl}{Token0Id}";

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
    CreatedAt
FROM
    dbo.Pairs
WHERE
    CreatedAt >= @from", new { from })).AsList();
            }
        }

        public static async Task StoreNewPairsAsync(List<Pair> pairs)
        {
            using (var conn = Context.OpenDatabaseConnection())
            {
                foreach(var pair in pairs)
				{
                   await conn.ExecuteAsync(@"
INSERT INTO dbo.Pairs(ExchangeId,PairId,Token0Id,Token0Code,CreatedAt)
VALUES (@ExchangeId,@PairId,@Token0Id,@Token0Code,@CreatedAt)", new { pair.ExchangeId,pair.PairId,pair.Token0Id,pair.Token0Code,pair.CreatedAt });
				}
            }
        }
    }
}
