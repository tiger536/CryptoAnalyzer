using System.Collections.Generic;

namespace CryptoAnalyzer.Models
{
	public class CoinViewModel
	{
		public Coin Coin { get; set; }
		public List<ChartDataPoint> PriceSeries { get; set; }
		public List<ChartDataPoint> VolumeSeries { get; set; }
		public List<ChartDataPoint> HitsSeries { get; set; }
		public List<ChartDataPoint> PriceSeriesYesterday { get; set; }
		public List<ChartDataPoint> VolumeSeriesYesterday { get; set; }
		public List<ChartDataPoint> RSI { get; set; }
		public List<ChartDataPoint> OBV { get; set; }
		public CoinRecap CoinRecap { get; set; }
	}
}
