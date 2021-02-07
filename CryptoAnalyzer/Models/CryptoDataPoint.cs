using CryptoAnalyzer.CoinGecko.DTO;
using Dapper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoAnalyzer.Models
{
    public class CryptoDataPoint
    {
        public DateTimeOffset LogDate { get; set; }
        public decimal Volume { get; set; }
        public decimal Price { get; set; }
        public decimal MarketCap { get; set; }

        public static async Task<DateTimeOffset?> GetLastUpdateDate(int coinID)
        {
            using (var conn = Context.OpenDatabaseConnection())
            {
                return await conn.QuerySingleOrDefaultAsync<DateTimeOffset?>(@"
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
SELECT TOP 1 LogDate FROM dbo.CryptoDetails WHERE CoinId = @coinID ORDER BY LogDate DESC", new { coinID });
            }
        }

        public static async Task<List<CryptoDataPoint>> GetTimeframe(DateTimeOffset from, DateTimeOffset to, int coinID)
        {
            using (var conn = Context.OpenDatabaseConnection())
            {
                return (await conn.QueryAsync<CryptoDataPoint>(@"
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
SELECT LogDate, Volume, Price, MarketCap FROM dbo.CryptoDetails
WHERE CoinId = @coinID AND LogDate BETWEEN @from AND @to ORDER BY LogDate ASC", new { from, to, coinID })).AsList();
            }
        }

        public static async Task BulkInsert(int coinID, List<CryptoDataPoint> dapaPoints)
        {
            using (var connection = Context.OpenDatabaseConnection())
            {
                foreach (var data in dapaPoints)
                {
                    await connection.ExecuteAsync(@"
INSERT INTO dbo.CryptoDetails (CoinId, LogDate, Volume, Price, MarketCap)
VALUES
(@CoinId, @LogDate, @Volume, @Price, @MarketCap)", new
                    {
                        CoinId = coinID,
                        data.LogDate,
                        data.Volume,
                        data.Price,
                        data.MarketCap
                    });
                }
            }
        }
    }
}
