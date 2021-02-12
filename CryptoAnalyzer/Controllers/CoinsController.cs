using CryptoAnalyzer.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoAnalyzer.Controllers
{
    public class CoinsController : Controller
    {
        [Route("Coins/{code?}")]
        public async Task<IActionResult> DetailsAsync(string code)
        {
            var coin = await Coin.GetByCode(code);
            if (coin is object)
            {
                var pointsRaw = await CryptoDataPoint.GetTimeframe(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.UtcNow, coin.Id);
                var priceSeries = new List<DataPoint>();
                var volumeSeries = new List<DataPoint>();
                foreach(var point in pointsRaw)
				{
                    priceSeries.Add(new DataPoint(point.LogDate.ToUnixTimeMilliseconds(), point.Price));
                    volumeSeries.Add(new DataPoint(point.LogDate.ToUnixTimeMilliseconds(), point.Volume));
                }
                var pointsRawYesterday = await CryptoDataPoint.GetTimeframe(DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), coin.Id);
                var priceSeriesYesterday = new List<DataPoint>();
                var volumeSeriesYesterday = new List<DataPoint>();
                foreach (var point in pointsRawYesterday)
                {
                    //AddDays is a cheap trick to display all data points on the same scale
                    priceSeriesYesterday.Add(new DataPoint(point.LogDate.AddDays(1).ToUnixTimeMilliseconds(), point.Price));
                    volumeSeriesYesterday.Add(new DataPoint(point.LogDate.AddDays(1).ToUnixTimeMilliseconds(), point.Volume));
                }

                return View("~/Views/Coin/Coin.cshtml",new CoinViewModel()
                {
                    PriceSeries = priceSeries,
                    VolumeSeries = volumeSeries,
                    PriceSeriesYesterday = priceSeriesYesterday,
                    VolumeSeriesYesterday = volumeSeriesYesterday,
                    CoinRecap = CoinRecap.GetRecap(pointsRaw, pointsRawYesterday)
                });
            }
            else
                return NotFound();
        }
    }
}
