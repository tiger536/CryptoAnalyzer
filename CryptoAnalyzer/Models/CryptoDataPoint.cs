using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using TicTacTec.TA.Library;

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

        public static (List<DataPoint> priceSeries, List<DataPoint> volumeSeries, List<DataPoint> hitsSeries) GetSeries(List<CryptoDataPoint> points, int daysToAdd = 0)
		{
            var priceSeries = new List<DataPoint>();
            var volumeSeries = new List<DataPoint>();
            var hitsSeries = new List<DataPoint>();
            foreach (var point in points)
            {
                priceSeries.Add(new DataPoint(point.LogDate.AddDays(daysToAdd).ToUnixTimeMilliseconds(), point.Price));
                volumeSeries.Add(new DataPoint(point.LogDate.AddDays(daysToAdd).ToUnixTimeMilliseconds(), point.Volume));
                if (point.Hits > 0)
                    hitsSeries.Add(new DataPoint(point.LogDate.AddDays(daysToAdd).ToUnixTimeMilliseconds(), point.Hits));
            }

            return (priceSeries, volumeSeries, hitsSeries);
        }

        public static List<DataPoint> GetRSI(List<CryptoDataPoint> points, int timeWindow = 14)
		{
            var rsi = new List<DataPoint>();
            double[] outReal = new double[points.Count - timeWindow];
            Core.Rsi(0, points.Count - 1, points.Select(x => (float)x.Price).ToArray(), timeWindow, out _, out int outNBElement, outReal);

            for (int i = 0; i < outNBElement; i++)
            {
                rsi.Add(new DataPoint(points[i + timeWindow].LogDate.ToUnixTimeMilliseconds(), outReal[i]));
            }

            return rsi;
        }

        public static List<DataPoint> GetOBV(List<CryptoDataPoint> points)
        {
            var obv = new List<DataPoint>();
            double[] outReal = new double[points.Count];
            Core.Obv(0, points.Count - 1, points.Select(x => (float)x.Price).ToArray(), 
                points.Select(x => (float)x.Volume).ToArray(), out _, out int outNBElement, outReal);

            for (int i = 0; i < outNBElement; i++)
            {
                obv.Add(new DataPoint(points[i].LogDate.ToUnixTimeMilliseconds(), outReal[i]));
            }

            return obv;
        }
    }
}
