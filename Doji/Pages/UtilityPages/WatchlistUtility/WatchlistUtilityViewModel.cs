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
        public CoinInfo Info { get; }
        public CoinFullAggregatedDataDisplay BtcPrice { get; }
        public CoinFullAggregatedDataDisplay EthPrice { get; }
        public string ImgUrl { get; }

        public Summary(CoinInfo coinInfo, CoinFullAggregatedDataDisplay btcPrice, CoinFullAggregatedDataDisplay ethPrice, string coinImgUrl)
        {
            Info = coinInfo;
            BtcPrice = btcPrice;
            EthPrice = ethPrice;
            ImgUrl = coinImgUrl;
        }
    }

    public class MyViewModelBase : ViewModelBase
    {
        protected static BittrexClient BittrexClient { get; } = new BittrexClient();
        protected static CryptoCompareClient CryptoCompareClient { get; } = CryptoCompareClient.Instance;
    }


    public class WatchlistUtilityViewModel : MyViewModelBase
    {
        private bool _isLoading;

        public bool IsLoading
        {
            get { return _isLoading; }
            private set { Set(ref _isLoading, value); }
        }

        public ObservableCollection<Summary> Summaries { get; set; }

        public WatchlistUtilityViewModel()
        {
            Summaries = new ObservableCollection<Summary>();
            InitializeAsync();
        }

        public async Task InitializeAsync()
        {
            IsLoading = true;

            var exchangeListResponse = await CryptoCompareClient.Exchanges.ListAsync();
            var coinListResponse = await CryptoCompareClient.Coins.ListAsync();

            var symbols = exchangeListResponse
                .First(x => x.Key == "BitTrex")
                .Value
                .Where(x => x.Value.Contains("BTC") && x.Value.Contains("ETH"))
                .Select(x => x.Key)
                .ToList();

            var coins = coinListResponse.Coins
                .Where(x => symbols.Contains(x.Key))
                .ToList();

            var priceMultiFullResponse = await CryptoCompareClient.Prices.MultiFullAsync(symbols, new[] { "BTC", "ETH" }, null, "BitTrex");
            var btcPriceMulti = priceMultiFullResponse.Display
                .SelectMany(x => x.Value)
                .Where(x => x.Key == "BTC")
                .Select(x => x.Value)
                .OrderByDescending(x => x.Volume24Hour)
                .ToList();

            var ethPriceMulti = priceMultiFullResponse.Display
                .SelectMany(x => x.Value)
                .Where(x => x.Key == "ETH")
                .Select(x => x.Value)
                .OrderByDescending(x => x.Volume24Hour)
                .ToList();

//
//            var btcMultiFullResponse = await CryptoCompareClient.Prices.MultiFullAsync(new[] { "BTC" }, new[] { "USD", "EUR" });
//            var btcMulti = btcMultiFullResponse.Display.First().Value.Values.ToList();
//            var ethMultiFullResponse = await CryptoCompareClient.Prices.MultiFullAsync(new[] { "ETH" }, new[] { "USD", "EUR" });


            Summaries.Clear();
            foreach (var c in coins)
            {
                try
                {
                    var coin = c.Value;
                    var btcPrice = btcPriceMulti.First(x => x.FromSymbol == c.Key);
                    var ethPrice = ethPriceMulti.First(x => x.FromSymbol == c.Key);

                    // imgUrl
                    var tryGetValue = coinListResponse.Coins.TryGetValue(c.Key, out var coinInfo);
                    var coinImgUrl = tryGetValue ? $"{coinListResponse.BaseImageUrl}{coinInfo.ImageUrl}" : null;

                    var summary = new Summary(coin, btcPrice, ethPrice, coinImgUrl);
                    Summaries.Add(summary);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            IsLoading = false;
        }
    }
}
