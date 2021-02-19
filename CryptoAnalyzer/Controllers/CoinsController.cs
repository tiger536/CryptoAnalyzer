using CryptoAnalyzer.Models;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Exceptional;
using System;
using System.Collections.Generic;
using System.Net;
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
                var pointsRaw = await CryptoDataPoint.GetTimeframeAsync(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.UtcNow, coin.Id);
                var priceSeries = new List<DataPoint>();
                var volumeSeries = new List<DataPoint>();
                var hitsSeries = new List<DataPoint>();
                foreach (var point in pointsRaw)
				{
                    priceSeries.Add(new DataPoint(point.LogDate.ToUnixTimeMilliseconds(), point.Price));
                    volumeSeries.Add(new DataPoint(point.LogDate.ToUnixTimeMilliseconds(), point.Volume));
                    hitsSeries.Add(new DataPoint(point.LogDate.ToUnixTimeMilliseconds(), point.Hits));
                }
                var pointsRawYesterday = await CryptoDataPoint.GetTimeframeAsync(DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), coin.Id);
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
                    Coin = coin,
                    PriceSeries = priceSeries,
                    VolumeSeries = volumeSeries,
                    HitsSeries = hitsSeries,
                    PriceSeriesYesterday = priceSeriesYesterday,
                    VolumeSeriesYesterday = volumeSeriesYesterday,
                    CoinRecap = CoinRecap.GetRecap(pointsRaw, pointsRawYesterday)
                });
            }
            else
                return NotFound();
        }

        [HttpPost]
        [Route("Coins/Spotlight")]
        public async Task<IActionResult> Spotlight(bool important, int coinID)
		{
            try
			{
                await Coin.SetSpotlight(important, coinID);
                return Ok();
			}
            catch(Exception e)
			{
                await e.LogAsync(HttpContext);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }          
		}

        [HttpPost]
        [Route("Coins/Ignore")]
        public async Task<IActionResult> Ignore(bool ignore, int coinID)
        {
            try
            {
                await Coin.SetIgnored(ignore, coinID);
                return Ok();
            }
            catch (Exception e)
            {
                await e.LogAsync(HttpContext);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        [Route("Coins/Notes")]
        public async Task<IActionResult> SetNotes(string notes, int coinID)
        {
            try
            {
                await Coin.SetNotes(notes, coinID);
                return Ok();
            }
            catch (Exception e)
            {
                await e.LogAsync(HttpContext);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }
    }
}
