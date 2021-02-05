using System.Collections.Generic;

namespace CryptoAnalyzer.Models
{
	public class CoinViewModel
	{
		public List<DataPoint> PriceSeries { get; set; }
		public List<DataPoint> VolumeSeries { get; set; }
	}
}
