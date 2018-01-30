using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bittrex.Net;
using Bittrex.Net.Objects;
using CryptoCompare;
using GalaSoft.MvvmLight;

namespace Doji.Pages.UtilityPages.WatchlistUtility
{
    public class Summary
    {
        public BittrexMarketSummary MarketSummary { get; }
        public BittrexCurrency Currency { get; }
        public string ImgUrl { get; }

        public decimal PercentageChange
        {
            get
            {
                var last = MarketSummary.Last;
                var prevDay = MarketSummary.PrevDay;

                var percentage = (last - prevDay) / prevDay * 100;
                var result = Math.Round(percentage, 1);
                return result;
            }
        }

        public Summary(BittrexMarketSummary marketSummary, BittrexCurrency currency, string imgUrl)
        {
            MarketSummary = marketSummary;
            Currency = currency;
            ImgUrl = imgUrl;
        }
    }

    public class MyViewModelBase : ViewModelBase
    {
        protected static BittrexClient BittrexClient { get; } = new BittrexClient();
        protected static CryptoCompareClient CryptoCompareClient { get; } = CryptoCompareClient.Instance;
    }


    public class WatchlistUtilityViewModel : MyViewModelBase
    {
        private BittrexClient _client;

        public string Hello { get; } = "Hello Watchlist";
        public ObservableCollection<Summary> Summaries { get; set; }

        public WatchlistUtilityViewModel()
        {
            _client = new BittrexClient();

            Summaries = new ObservableCollection<Summary>();
            InitializeAsync();

        }

        public async Task InitializeAsync()
        {
            var coinList = await CryptoCompareClient.Coins.ListAsync();
            var currencies = (await BittrexClient.GetCurrenciesAsync()).Result;
            var marketSummaries = (await BittrexClient.GetMarketSummariesAsync())
                .Result
                .Where(ms => ms.MarketName.StartsWith("BTC-"))
                .OrderByDescending(ms => ms.BaseVolume);

            Summaries.Clear();
            foreach (var ms in marketSummaries)
            {
                try
                {
                    var symbol = ms.MarketName.Split('-').Last();
                    var bittrexCurrency = currencies.First(x => x.Currency == symbol);

                    var tryGetValue = coinList.Coins.TryGetValue(symbol, out var coinInfo);
                    var imgUrl = tryGetValue ? $"{coinList.BaseImageUrl}{coinInfo.ImageUrl}" : null;

                    var summary = new Summary(ms, bittrexCurrency, imgUrl);
                    Summaries.Add(summary);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}
