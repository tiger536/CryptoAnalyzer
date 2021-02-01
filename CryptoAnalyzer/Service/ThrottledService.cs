using Newtonsoft.Json;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoAnalyzer.Service
{
    public class ThrottledService : IThrottledService
    {
        private readonly HttpClient _httpClient;
        private static readonly SemaphoreSlim _throttler = new SemaphoreSlim(1);
        public ThrottledService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<T> GetAsync<T>(string partialPath)
        {
            var tcs = new TaskCompletionSource<T>();

            _throttler.WaitAsync().ContinueWith(async t =>
            {
                var stopwatch = new Stopwatch();
                tcs.SetResult(JsonConvert.DeserializeObject<T>(await _httpClient.GetStringAsync(partialPath)));
                stopwatch.Stop();
                return stopwatch.ElapsedMilliseconds;
            }).Unwrap().ContinueWith(async antecedent =>
            {
                var delay = antecedent.Result > 50 ? 0 : 50 - antecedent.Result;
                if (delay > 0)
                    await Task.Delay((int)delay);

                _throttler.Release();
            });
            return tcs.Task;
        }
    }
}
