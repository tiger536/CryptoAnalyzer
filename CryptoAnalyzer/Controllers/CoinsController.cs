using Microsoft.AspNetCore.Mvc;

namespace CryptoAnalyzer.Controllers
{
    public class CoinsController : Controller
    {
        [Route("Coins/{code?}")]
        public IActionResult Details(string code)
        {

            return View();
        }
    }
}
