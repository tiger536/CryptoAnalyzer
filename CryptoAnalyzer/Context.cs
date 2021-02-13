using CryptoAnalyzer.CoinGecko;
using CryptoAnalyzer.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Data.Common;
using System.Data.SqlClient;

namespace CryptoAnalyzer
{    
    public static class Context
    {
        public static CoinGeckoConfiguration CoinGeckoConfiguration { get; private set; }
        public static TelegramConfiguration TelegramBotConfiguration { get; private set; }
        private static string _sqlConnectionString = default!;

        public static void Initialize(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _sqlConnectionString = configuration.GetConnectionString("DefaultConnection");
            CoinGeckoConfiguration = configuration.GetSection("CoinGecko").Get<CoinGeckoConfiguration>();
            TelegramBotConfiguration = configuration.GetSection("Telegram").Get<TelegramConfiguration>();
        }

        public static DbConnection OpenDatabaseConnection()
        {
            return new SqlConnection(_sqlConnectionString);
        }
    }
}
