using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;

namespace CryptoAnalyzer.CoinGecko.DTO
{
	[JsonArray]
	public class DataPoint : ICollection<object>
	{
		private readonly List<object> Points = new List<object>();

		public int Count => Points.Count;

		public bool IsReadOnly => false;

		public void Add(object item)
		{
			Points.Add(item);
		}

		public void Clear()
		{
			Points.Clear();
		}

		public bool Contains(object item)
		{
			return Points.Contains(item);
		}

		public void CopyTo(object[] array, int arrayIndex)
		{
			Points.CopyTo(array, arrayIndex);
		}

		public IEnumerator<object> GetEnumerator()
		{
			return Points.GetEnumerator();
		}

		public bool Remove(object item)
		{
			return Points.Remove(item);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Points.GetEnumerator();
		}

		public decimal PointValue => Convert.ToDecimal(Points[1]);
		public DateTimeOffset Date => DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(Points[0]));
	}
}
