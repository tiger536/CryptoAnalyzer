using CryptoAnalyzer.CoinGecko.DTO;
using CryptoAnalyzer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StackExchange.Exceptional;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace CryptoAnalyzer.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var coins = (await Coin.GetImportantCoins(DateTimeOffset.UtcNow.AddDays(-3))).Where(x => !x.IsUseless()).ToList();
            return View(new IndexViewModel()
            {
                Coins = coins
            });
        }
        public async Task Exceptions() => await ExceptionalMiddleware.HandleRequestAsync(HttpContext).ConfigureAwait(false);

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
