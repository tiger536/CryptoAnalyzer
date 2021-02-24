using System.Collections.Generic;

namespace CryptoAnalyzer.Models
{
	public class CoinViewModel
	{
		public Coin Coin { get; set; }
		public List<DataPoint> PriceSeries { get; set; }
		public List<DataPoint> VolumeSeries { get; set; }
		public List<DataPoint> HitsSeries { get; set; }
		public List<DataPoint> PriceSeriesYesterday { get; set; }
		public List<DataPoint> VolumeSeriesYesterday { get; set; }
		public List<DataPoint> RSI { get; set; }
		public List<DataPoint> OBV { get; set; }
		public CoinRecap CoinRecap { get; set; }
	}
}
