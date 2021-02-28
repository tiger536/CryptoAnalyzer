using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace CryptoAnalyzer.Models
{
    [JsonConverter(typeof(FooConverter))]
    public class ChartDataPoint
	{
        public DateTimeOffset X { get; set; }
        public dynamic Y { get; set; }
    }

    public class FooConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var foo = value as ChartDataPoint;
            var obj = new object[] {foo.X.ToUnixTimeMilliseconds(), foo.Y };
            serializer.Serialize(writer, obj);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var arr = ReadArrayObject(reader, serializer);
            return new ChartDataPoint
            {
                X = DateTimeOffset.FromUnixTimeMilliseconds((long)arr[0]),
                Y = (dynamic)arr[1],
            };
        }

        private JArray ReadArrayObject(JsonReader reader, JsonSerializer serializer)
        {
            var arr = serializer.Deserialize<JToken>(reader) as JArray;
            if (arr == null || arr.Count != 2)
                throw new JsonSerializationException("Expected array of length 2");
            return arr;
        }

		public override bool CanConvert(Type objectType)
		{
            return objectType == typeof(ChartDataPoint); ;
		}
	}

}
