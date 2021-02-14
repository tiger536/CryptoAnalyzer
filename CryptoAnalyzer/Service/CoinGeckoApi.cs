using CryptoAnalyzer.Service;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoAnalyzer.CoinGecko
{
    public class CoinGeckoApi : IHostedService
    {
        private static ThrottledHttpClient _httpClient;
        private static TelegramBot _telegramBot;
        private static SpotlightHandler _spotlighter;
        private static CoinListGrabber _coinListGrabber;
        public CoinGeckoApi(ThrottledHttpClient client, TelegramBot telegramApi)
        {
            _httpClient = client;
            _telegramBot = telegramApi;
            _spotlighter = new SpotlightHandler(_httpClient, _telegramBot);
            _coinListGrabber = new CoinListGrabber(_httpClient);
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() => _spotlighter.GrabAsync());
            Task.Run(() => _coinListGrabber.GrabAsync());

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _coinListGrabber.Cancel();
            _spotlighter.Cancel();

            return Task.CompletedTask;
        }
    }
}
