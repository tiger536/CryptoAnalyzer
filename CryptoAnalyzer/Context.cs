﻿using CryptoAnalyzer.Chan;
using CryptoAnalyzer.CoinGecko;
using CryptoAnalyzer.CoinMarketCap;
using CryptoAnalyzer.Defi;
using CryptoAnalyzer.Service;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Data.Common;
using System.Data.SqlClient;

namespace CryptoAnalyzer
{    
    public static class Context
    {
        public static CoinGeckoConfiguration CoinGeckoConfiguration { get; private set; }
        public static CoinMarketCapConfiguration CoinMarketCapConfiguration { get; private set; }
        public static ChanConfiguration ChanConfiguration { get; private set; }
        public static DefiConfiguration DefiConfiguration { get; private set; }
        public static TelegramConfiguration TelegramBotConfiguration { get; private set; }
        private static string _sqlConnectionString = default!;
        public const int COIN_DAYS = -5;
        public const int MAX_CONCURRENT_COINS = 105;

        public static void Initialize(IConfiguration configuration, IWebHostEnvironment environment)
        {
            _sqlConnectionString = configuration.GetConnectionString("DefaultConnection");
            CoinGeckoConfiguration = configuration.GetSection("CoinGecko").Get<CoinGeckoConfiguration>();
            CoinMarketCapConfiguration = configuration.GetSection("CoinMarketCap").Get<CoinMarketCapConfiguration>();
            ChanConfiguration = configuration.GetSection("ChanAPI").Get<ChanConfiguration>();
            DefiConfiguration = configuration.GetSection("Defi").Get<DefiConfiguration>();
            TelegramBotConfiguration = configuration.GetSection("Telegram").Get<TelegramConfiguration>();

            DataMigration();
        }

        private static void DataMigration()
		{
            using (var conn = OpenDatabaseConnection())
            {
                conn.Execute(@"
SELECT Value FROM dbo.Content WHERE Name = 'BizThreadData'

IF @@ROWCOUNT = 0
	INSERT INTO dbo.Content(Name,Value) VALUES ('BizThreadData','{}')");
            }
        }

        public static DbConnection OpenDatabaseConnection()
        {
            return new SqlConnection(_sqlConnectionString);
        }
    }
}
