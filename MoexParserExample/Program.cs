using MoexParser;
using System;

namespace MoexParserExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Данные за 2018 год:");
            foreach (Stock st in Service.GetDataFromIssParallel(new DateTime(2018, 1, 1), new DateTime(2018, 12, 31)))
            {
                Console.WriteLine(st.Ticker);
                foreach (HistoryPrice price in st.Price) Console.WriteLine($"{price.Date}: {price.Close}");
                Console.WriteLine("----------");
            }
            Console.ReadLine();
        }
    }
}
