using CryptoAnalyzer.Chan;
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
        private static CoinListGrabber _coinListGrabber;
        private static ThreadScraper _threadScraper;
        public BackgroundTaskService(ThrottledHttpClient throttledhttpClient, TelegramBot telegramApi, HttpClient httpClient)
        {
            _spotlighter = new SpotlightHandler(throttledhttpClient, telegramApi);
            _coinListGrabber = new CoinListGrabber(throttledhttpClient);
            _threadScraper = new ThreadScraper(httpClient);
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() => _spotlighter.GrabAsync(), cancellationToken);
            Task.Run(() => _coinListGrabber.GrabAsync(), cancellationToken);
            Task.Run(() => _threadScraper.ScrapAsync(), cancellationToken);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _coinListGrabber.Cancel();
            _spotlighter.Cancel();
            _threadScraper.Cancel();

            return Task.CompletedTask;
        }
    }
}
