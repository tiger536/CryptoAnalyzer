using System;
using System.Runtime.Serialization;

namespace CryptoAnalyzer.Models
{
    //DataContract for Serializing Data - required to serve in JSON format
    [DataContract]
    public class DataPoint
    {
        public DataPoint(long x, dynamic y)
        {
            this.X = x;
            this.Y = y;
        }

        [DataMember(Name = "x")]
        public long X;

        [DataMember(Name = "y")]
        public dynamic Y = null;
    }
}
