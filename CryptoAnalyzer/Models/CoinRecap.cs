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
		public decimal Last9HoursAvgVolume { get; set; }
		public bool HugeVolSpikeLastHour { get; set; }
		public decimal LastHourVolumeVariation => LastHourAvgVolume > 0 ? ((LastVolume - LastHourAvgVolume) / LastHourAvgVolume) : default;
		public decimal Last3HoursVolumeVariation => Last3HoursAvgVolume > 0 ? ((LastVolume - Last3HoursAvgVolume) / Last3HoursAvgVolume) : default;
		public decimal Last9HoursVolumeVariation => Last9HoursAvgVolume > 0 ? ((LastVolume - Last9HoursAvgVolume) / Last9HoursAvgVolume) : default;

		public static CoinRecap GetRecap(List<CryptoDataPoint> dataPointsToday, List<CryptoDataPoint> dataPointsYesterday)
		{
			var lastDayMeanVolume = dataPointsYesterday.Any() ? dataPointsYesterday.Average(x => x.Volume) : default;
			var lastHourHalfPoint = dataPointsToday.Where(x => x.LogDate >= DateTimeOffset.UtcNow.AddMinutes(-90));
			var lastHourPoint = dataPointsToday.Where(x => x.LogDate >= DateTimeOffset.UtcNow.AddMinutes(-70) && x.LogDate <= DateTimeOffset.UtcNow.AddMinutes(-10));
			var lastHourAvgVolume = lastHourPoint.Any() ? lastHourPoint.Average(x => x.Volume) : default;
			bool hugeVolVar = false;
			if(lastHourHalfPoint.Any())
			{
				var order = lastHourHalfPoint.OrderBy(i => i.Volume).ToList();
				var min = order.FirstOrDefault();
				var max = order.LastOrDefault();
				if(max.LogDate > min.LogDate && ((max.Price - min.Price)/min.Price) > 0.35M)
				{
					hugeVolVar = true;
				}
			}
			var last3HoursPoint = dataPointsToday.Where(x => x.LogDate >= DateTimeOffset.UtcNow.AddHours(-4) && x.LogDate <= DateTimeOffset.UtcNow.AddMinutes(-30));
			var last3HoursAvgVolume = last3HoursPoint.Any() ? last3HoursPoint.Average(x => x.Volume) : default;

			var last9HoursPoint = dataPointsToday.Where(x => x.LogDate >= DateTimeOffset.UtcNow.AddHours(-10) && x.LogDate <= DateTimeOffset.UtcNow.AddHours(-1));
			var last9HoursAvgVolume = last9HoursPoint.Any() ? last9HoursPoint.Average(x => x.Volume) : default;

			var last = dataPointsToday.LastOrDefault() ?? new CryptoDataPoint();
			var first = dataPointsToday.FirstOrDefault() ?? new CryptoDataPoint();
			var h1Ago = dataPointsToday.FirstOrDefault(x => x.LogDate >= last.LogDate.AddHours(-1)) ?? new CryptoDataPoint();
			
			var recap = new CoinRecap
			{
				LastLog = last.LogDate,
				LastPrice = last.Price,
				LastVolume = last.Volume,
				LastMarketCap = last.MarketCap,
				PriceVariation24h = first.Price > 0 ? ((last.Price - first.Price) / first.Price) :0,
				VoumeVariation24h = first.Volume > 0 ? ((last.Volume - first.Volume) / first.Volume) : 0,
				MarketCapVariation24h = first.MarketCap > 0 ? ((last.MarketCap - first.MarketCap) / first.MarketCap) : 0,
				PriceVariation1h = h1Ago.Price > 0 ? ((last.Price - h1Ago.Price) / h1Ago.Price) : 0,
				YesterdayAvgVolume = lastDayMeanVolume,
				LastHourAvgVolume = lastHourAvgVolume,
				Last3HoursAvgVolume = last3HoursAvgVolume,
				Last9HoursAvgVolume = last9HoursAvgVolume,
				HugeVolSpikeLastHour = hugeVolVar
			};
			return recap;
		}
	}
}
