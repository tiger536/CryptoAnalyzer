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
		public decimal PriceVariation24h { get; set; }
		public float PriceVariation1h { get; set; }

		public static CoinRecap GetRecap(List<CryptoDataPoint> dataPoints)
		{
			var last = dataPoints.Last();
			var first = dataPoints.First();
			var h1Ago = dataPoints.First(x => x.LogDate >= last.LogDate.AddHours(-1));
			var recap = new CoinRecap();
			recap.LastLog = last.LogDate;
			recap.LastPrice = last.Price;
			recap.PriceVariation24h = ((last.Price - first.Price) / first.Price);
			recap.PriceVariation1h = (float)((last.Price - h1Ago.Price) / h1Ago.Price);
			return recap;
		}
	}
}
