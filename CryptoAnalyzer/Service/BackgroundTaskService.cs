using CryptoAnalyzer.Chan;
using CryptoAnalyzer.CoinMarketCap;
using CryptoAnalyzer.Defi;
using CryptoAnalyzer.Service;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoAnalyzer.CoinGecko
{
    public class BackgroundTaskService : IHostedService
    {
        private static SpotlightHandler _spotlighter;
        private static FastRefreshHandler _fastRefreshHandler;
        private static CoinListGrabber _coinListGrabber;
        private static ThreadScraper _threadScraper;
        private static DefiScraper _defiScraper;
        public BackgroundTaskService(ThrottledHttpClient throttledhttpClient, TelegramBot telegramApi, HttpClient httpClient, DefiQLClient defiQLClient, IHttpClientFactory factory)
        {
            _spotlighter = new SpotlightHandler(throttledhttpClient, telegramApi);
            _fastRefreshHandler = new FastRefreshHandler(factory.CreateClient("CoinMarketCap"), telegramApi);
            _coinListGrabber = new CoinListGrabber(throttledhttpClient, telegramApi);
            _threadScraper = new ThreadScraper(httpClient);
            _defiScraper = new DefiScraper(telegramApi, defiQLClient);
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() => _spotlighter.GrabAsync(), cancellationToken);
            Task.Run(() => _coinListGrabber.GrabAsync(), cancellationToken);
            Task.Run(() => _threadScraper.ScrapAsync(), cancellationToken);
            Task.Run(() => _defiScraper.ScrapAsync(), cancellationToken);
            Task.Run(() => _fastRefreshHandler.GrabAsync(), cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _coinListGrabber.Cancel();
            _spotlighter.Cancel();
            _threadScraper.Cancel();
            _defiScraper.Cancel();
            _fastRefreshHandler.Cancel();

            return Task.CompletedTask;
        }
    }
}
