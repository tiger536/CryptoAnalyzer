using CryptoAnalyzer.CoinGecko.DTO;
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
                var pointsRaw = await CryptoDataPoint.GetTimeframe(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now, coin.Id);
                var priceSeries = new List<Models.DataPoint>();
                var volumeSeries = new List<Models.DataPoint>();
                foreach(var point in pointsRaw)
				{
                    priceSeries.Add(new Models.DataPoint(point.LogDate.ToUnixTimeMilliseconds(), point.Price));
                    volumeSeries.Add(new Models.DataPoint(point.LogDate.ToUnixTimeSeconds(), point.Volume));
                }

                return View("~/Views/Coin/Coin.cshtml",new CoinViewModel()
                {
                    PriceSeries = priceSeries,
                    VolumeSeries = volumeSeries
                });
            }
            return View();
        }
    }
}
