using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CryptoAnalyzer.Models
{
    public class CryptoDataPoint
    {
        public DateTimeOffset LogDate { get; set; }
        public decimal Volume { get; set; }
        public decimal Price { get; set; }
        public decimal MarketCap { get; set; }
        public int? Hits { get; set; }

        public static async Task<DateTimeOffset?> GetLastUpdateDateAsync(int coinID)
        {
            using (var conn = Context.OpenDatabaseConnection())
            {
                return await conn.QuerySingleOrDefaultAsync<DateTimeOffset?>(@"
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
SELECT TOP 1 LogDate FROM dbo.CryptoDetails WHERE CoinId = @coinID ORDER BY LogDate DESC", new { coinID });
            }
        }

        public static async Task<List<CryptoDataPoint>> GetTimeframeAsync(DateTimeOffset from, DateTimeOffset to, int coinID)
        {
            using (var conn = Context.OpenDatabaseConnection())
            {
                return (await conn.QueryAsync<CryptoDataPoint>(@"
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
SELECT LogDate, Volume, Price, MarketCap, Hits FROM dbo.CryptoDetails
WHERE CoinId = @coinID AND LogDate BETWEEN @from AND @to ORDER BY LogDate ASC", new { from, to, coinID })).AsList();
            }
        }

        public static async Task BulkInsertAsync(int coinID, List<CryptoDataPoint> dapaPoints)
        {
            var dt = new DataTable();
            dt.Columns.Add("LogDate", typeof(DateTimeOffset));
            dt.Columns.Add("Volume", typeof(decimal));
            dt.Columns.Add("Price", typeof(decimal));
            dt.Columns.Add("MarketCap", typeof(decimal));
            foreach (var val in dapaPoints)
            {
                var row = dt.NewRow();
                row["LogDate"] = val.LogDate;
                row["Volume"] = val.Volume;
                row["Price"] = val.Price;
                row["MarketCap"] = val.MarketCap;
                dt.Rows.Add(row);
            }

            using (var connection = Context.OpenDatabaseConnection())
            {
                await connection.ExecuteAsync(@"
DECLARE @vCoinID INT = @CoinID
        ,@LatestHits INT

SELECT @LatestHits = Hits FROM dbo.CryptoCurrency WITH(NOLOCK) WHERE Id = @vCoinID

INSERT INTO dbo.CryptoDetails (CoinId, LogDate, Volume, Price, MarketCap, Hits)
SELECT
    @vCoinID,
    LogDate,
    Volume,
    Price,
    MarketCap,
    @LatestHits
FROM
    @data", new { CoinId = coinID, data = dt.AsTableValuedParameter("dbo.TVP_CryptoData") });
            }

            dt.Dispose();
        }
    }
}
