using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoAnalyzer.Models
{
	public class CoinRecap
	{
		public string Name { get; set; }
		public DateTimeOffset LastLog { get; set; }
		public decimal LastPrice { get; set; }
		public decimal LastVolume { get; set; }
		public decimal LastMarketCap { get; set; }
		public decimal PriceVariation24h { get; set; }
		public decimal VoumeVariation24h { get; set; }
		public decimal MarketCapVariation24h { get; set; }
		public decimal PriceVariation1h { get; set; }
		public decimal? YesterdayAvgVolume { get; set; }
		public decimal LastHourAvgVolume { get; set; }
		public decimal Last3HoursAvgVolume { get; set; }

		public static CoinRecap GetRecap(List<CryptoDataPoint> dataPointsToday, List<CryptoDataPoint> dataPointsYesterday)
		{
			var lastDayMeanVolume = dataPointsYesterday.Any() ? dataPointsYesterday.Average(x => x.Volume) : default;
			var lastHourAvgVolume = dataPointsToday.Where(x => x.LogDate >= DateTimeOffset.UtcNow.AddMinutes(-70) && x.LogDate <= DateTimeOffset.UtcNow.AddMinutes(-10)).Average(x => x.Volume);
			var last3HoursAvgVolume = dataPointsToday.Where(x => x.LogDate >= DateTimeOffset.UtcNow.AddHours(-4) && x.LogDate <= DateTimeOffset.UtcNow.AddHours(-1)).Average(x => x.Volume);
			var last = dataPointsToday.Last();
			var first = dataPointsToday.First();
			var h1Ago = dataPointsToday.First(x => x.LogDate >= last.LogDate.AddHours(-1));
			
			var recap = new CoinRecap
			{
				LastLog = last.LogDate,
				LastPrice = last.Price,
				LastVolume = last.Volume,
				LastMarketCap = last.MarketCap,
				PriceVariation24h = (last.Price - first.Price) / first.Price,
				VoumeVariation24h = (last.Volume - first.Volume) / first.Volume,
				MarketCapVariation24h = first.MarketCap > 0 ? ((last.MarketCap - first.MarketCap) / first.MarketCap) : 0,
				PriceVariation1h = (last.Price - h1Ago.Price) / h1Ago.Price,
				YesterdayAvgVolume = lastDayMeanVolume,
				LastHourAvgVolume = lastHourAvgVolume,
				Last3HoursAvgVolume = last3HoursAvgVolume
			};
			return recap;
		}
	}
}
