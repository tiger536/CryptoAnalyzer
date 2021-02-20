using CryptoAnalyzer.Models;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Exceptional;
using System;
using System.Threading.Tasks;

namespace CryptoAnalyzer.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {         
            return View(new IndexViewModel()
            {
                NewCoinThresold = Context.COIN_DAYS,
                Coins = await Coin.GetImportantCoinsAsync(DateTimeOffset.UtcNow.AddDays(Context.COIN_DAYS)),
                InternetBuzzCoins = await Coin.GetInternetBuzzCoinsAsync(DateTimeOffset.UtcNow.AddDays(Context.COIN_DAYS)),
                LatestDefiCoins = await Pair.GetLatest(DateTimeOffset.UtcNow.AddDays(-2))
            });
        }

        public async Task Exceptions() => await ExceptionalMiddleware.HandleRequestAsync(HttpContext).ConfigureAwait(false);
    }
}
