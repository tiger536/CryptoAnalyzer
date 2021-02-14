using Newtonsoft.Json;
using StackExchange.Exceptional;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CryptoAnalyzer.Service
{
    public class ThrottledHttpClient
    {
        private readonly HttpClient _httpClient;
        private static readonly SemaphoreSlim _throttler = new SemaphoreSlim(1);
        public ThrottledHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<T> GetAsync<T>(string partialPath)
        {
            var tcs = new TaskCompletionSource<T>();

            _throttler.WaitAsync().ContinueWith(async t =>
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                try
                {
                    var response = await _httpClient.GetAsync(partialPath);
                    if(response.StatusCode == System.Net.HttpStatusCode.TooManyRequests || response.StatusCode == System.Net.HttpStatusCode.GatewayTimeout)
                    {
                        await Task.Delay(7000);
                        response = await _httpClient.GetAsync(partialPath);
                    }
                    response.EnsureSuccessStatusCode();
                    var result = await response.Content.ReadAsStringAsync();

                    tcs.SetResult(JsonConvert.DeserializeObject<T>(result));
                }
                catch(Exception e)
                {
                    await e.LogAsync(null);
                    tcs.SetResult(default);
                }
                stopwatch.Stop();

                if(stopwatch.Elapsed > TimeSpan.FromSeconds(25))
				{
                    //fuck
                    new Exception($"Coingecko banned me ({stopwatch.Elapsed}). {partialPath}").LogNoContext();
				}

                var delay = stopwatch.ElapsedMilliseconds > 1000 ? 0 : 1000 - stopwatch.ElapsedMilliseconds;
                if (delay < 625) delay = 625;
                await Task.Delay((int)delay);

                _throttler.Release();
            });

            return tcs.Task;
        }
    }
}
