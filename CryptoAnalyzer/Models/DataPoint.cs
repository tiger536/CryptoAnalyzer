using System;
using System.Runtime.Serialization;

namespace CryptoAnalyzer.Models
{
    //DataContract for Serializing Data - required to serve in JSON format
    [DataContract]
    public class DataPoint
    {
        public DataPoint(double x, dynamic y)
        {
            this.X = x;
            this.Y = y;
        }

        [DataMember(Name = "x")]
        public Nullable<double> X = null;

        [DataMember(Name = "y")]
        public dynamic Y = null;
    }
}
