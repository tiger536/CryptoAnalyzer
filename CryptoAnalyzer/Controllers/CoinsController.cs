using CryptoAnalyzer.Models;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Exceptional;
using System;
using System.Net;
using System.Threading.Tasks;

namespace CryptoAnalyzer.Controllers
{
    public class CoinsController : Controller
    {
        [Route("Coins/{code?}")]
        public async Task<IActionResult> DetailsAsync(string code)
        {
            var coin = await Coin.GetByCodeAsync(code);
            if (coin is object)
            {
                var pointsRaw = await CryptoDataPoint.GetTimeframeAsync(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.UtcNow, coin.Id);
                var todaySeries = CryptoDataPoint.GetSeries(pointsRaw);

                var pointsRawYesterday = await CryptoDataPoint.GetTimeframeAsync(DateTimeOffset.UtcNow.AddDays(-2), DateTimeOffset.UtcNow.AddDays(-1), coin.Id);
                var yesterdaySeries = CryptoDataPoint.GetSeries(pointsRawYesterday, 1);

                return View("~/Views/Coin/Coin.cshtml",new CoinViewModel()
                {
                    Coin = coin,
                    PriceSeries = todaySeries.priceSeries,
                    VolumeSeries = todaySeries.volumeSeries,
                    HitsSeries = todaySeries.hitsSeries,
                    RSI = CryptoDataPoint.GetRSI(pointsRaw),
                    OBV = CryptoDataPoint.GetOBV(pointsRaw),
                    PriceSeriesYesterday = yesterdaySeries.priceSeries,
                    VolumeSeriesYesterday = yesterdaySeries.volumeSeries,
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
                await Coin.SetSpotlightAsync(important, coinID);
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
                await Coin.SetIgnoredAsync(ignore, coinID);
                return Ok();
            }
            catch (Exception e)
            {
                await e.LogAsync(HttpContext);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        [Route("Coins/FastUpdate")]
        public async Task<IActionResult> FastRefresh(bool fastUpdate, int coinID)
        {
            try
            {
                await Coin.SetFastRefreshAsync(fastUpdate, coinID);
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
                await Coin.SetNotesAsync(notes, coinID);
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
