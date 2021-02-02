using System.Threading.Tasks;

namespace CryptoAnalyzer.Service
{
    public interface IThrottledService
    {
        public Task<T> GetAsync<T>(string partialPath) where T : new();
    }
}
