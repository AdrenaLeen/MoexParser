using System;

namespace MoexParser
{
    [Serializable]
    public class HistoryPrice
    {
        public DateTime Date { get; set; }
        public double Close { get; set; }
    }
}
