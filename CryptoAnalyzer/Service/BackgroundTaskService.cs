using CryptoAnalyzer.Chan;
using CryptoAnalyzer.Defi;
using CryptoAnalyzer.Service;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
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
        private static DefiScraper _defiScraper;
        public BackgroundTaskService(ThrottledHttpClient throttledhttpClient, TelegramBot telegramApi, HttpClient httpClient)
        {
            _spotlighter = new SpotlightHandler(throttledhttpClient, telegramApi);
            _coinListGrabber = new CoinListGrabber(throttledhttpClient, telegramApi);
            _threadScraper = new ThreadScraper(httpClient);
            _defiScraper = new DefiScraper(telegramApi);
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() => _spotlighter.GrabAsync(), cancellationToken);
            Task.Run(() => _coinListGrabber.GrabAsync(), cancellationToken);
            Task.Run(() => _threadScraper.ScrapAsync(), cancellationToken);
            Task.Run(() => _defiScraper.ScrapAsync(), cancellationToken);

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
