using System;
using System.Collections.Generic;

namespace MoexParser
{
    [Serializable]
    public class Stock
    {
        public string Ticker { get; set; }
        public List<HistoryPrice> Price { get; set; }
    }
}
