using System.Collections.Generic;

namespace CryptoAnalyzer.Defi
{
    public class Token
    {
        public string id { get; set; }
        public string name { get; set; }
        public string symbol { get; set; }
        public string txCount { get; set; }
    }

    public class Pair
    {
        public long createdAtTimestamp { get; set; }
        public string id { get; set; }
        public Token token0 { get; set; }
        public string token0Price { get; set; }
        public Token token1 { get; set; }
        public string token1Price { get; set; }
    }

    public class UniSwapData
    {
        public List<Pair> pairs { get; set; }
    }

}
