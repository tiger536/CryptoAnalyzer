using CryptoAnalyzer.Models;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Exceptional;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoAnalyzer.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> Index()
        {
            var coins = (await Coin.GetImportantCoinsAsync(DateTimeOffset.UtcNow.AddDays(Context.COIN_DAYS))).Where(x => !x.IsUseless()).ToList();
            return View(new IndexViewModel()
            {
                Coins = coins
            });
        }
        public async Task Exceptions() => await ExceptionalMiddleware.HandleRequestAsync(HttpContext).ConfigureAwait(false);
    }
}
