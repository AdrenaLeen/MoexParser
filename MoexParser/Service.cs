using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MoexParser
{
    public static class Service
    {
        public static List<Stock> GetDataFromIss(DateTime startDate, DateTime endDate)
        {
            List<Stock> sData = new List<Stock>();
            foreach (string ticker in GetTickersFromIss(startDate, endDate))
            {
                sData.Add(new Stock() { Ticker = ticker, Price = ParseStock(ticker, startDate, endDate) });
            }
            return sData;
        }

        public static List<Stock> GetDataFromIssParallel(DateTime startDate, DateTime endDate)
        {
            List<Stock> sData = new List<Stock>();
            Parallel.ForEach(GetTickersFromIss(startDate, endDate), (ticker) =>
            {
                sData.Add(new Stock() { Ticker = ticker, Price = ParseStock(ticker, startDate, endDate) });
            });
            return sData;
        }

        public static string GetShortName(string ticker)
        {
            WebClient webClient = new WebClient { Encoding = Encoding.UTF8 };
            string baseString = $"https://iss.moex.com/iss/securities/{ticker}.json?iss.meta=off";
            JObject jResponse = JObject.Parse(webClient.DownloadString(baseString));
            return jResponse["description"]["data"][2][2].ToString();
        }

        private static List<string> GetTickersFromIss(DateTime startDate, DateTime endDate)
        {
            WebClient webClient = new WebClient();
            List<int> cursor = new List<int>();
            List<string> tickers = new List<string>();
            string baseString = $"http://iss.moex.com/iss/history/engines/stock/markets/shares/boards/tqbr/securities.json?date={startDate:yyyy-MM-dd}&iss.meta=off&history.columns=SECID";
            string queryString = baseString;
            do
            {
                if (cursor.Count > 0) queryString = $"{baseString}&start={cursor[0] + cursor[2]}";
                JObject jResponse = JObject.Parse(webClient.DownloadString(queryString));
                tickers.AddRange(from j in jResponse["history"]["data"] select j[0].ToString());
                cursor = (from j in jResponse["history.cursor"]["data"][0] select int.Parse(j.ToString())).ToList();
            } while (cursor[0] + cursor[2] < cursor[1]);
            if (tickers.Count == 0)
            {
                startDate = startDate.AddDays(1);
                if (startDate < endDate) return GetTickersFromIss(startDate, endDate);
            }
            return tickers;
        }

        private static List<HistoryPrice> ParseStock(string ticker, DateTime startDate, DateTime endDate)
        {
            WebClient webClient = new WebClient();
            string baseString = $"http://iss.moex.com/iss/history/engines/stock/markets/shares/boards/tqbr/securities/{ticker}.json?from={startDate:yyyy-MM-dd}&till={endDate:yyyy-MM-dd}&iss.meta=off&history.columns=TRADEDATE,CLOSE";
            string queryString = baseString;
            List<HistoryPrice> stockPrices = new List<HistoryPrice>();
            DateTime last = new DateTime();
            int start = 0;
            bool canContinue = true;
            do
            {
                if (last > startDate) queryString = $"{baseString}&start={start}";
                JObject jResponse = new JObject();
                try
                {
                    jResponse = JObject.Parse(webClient.DownloadString(queryString));
                }
                catch (WebException)
                {
                    return ParseStock(ticker, startDate, endDate);
                }
                foreach (JToken j in jResponse["history"]["data"])
                {
                    if (j[1].ToString() == "null" || j[1].ToString() == "") continue;
                    stockPrices.Add(new HistoryPrice { Date = DateTime.Parse(j[0].ToString()), Close = double.Parse(j[1].ToString()) });
                }
                canContinue = (jResponse["history"]["data"].Count() == 100) && DateTime.TryParse(jResponse["history"]["data"].Last[0].ToString(), out last);
                start += jResponse["history"]["data"].Count();
            } while (canContinue && last <= endDate);
            return stockPrices;
        }
    }
}
