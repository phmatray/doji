namespace Doji.Models
{
    using CryptoCompare;

    public class Summary
    {
        public Summary(CoinInfo coinInfo, CoinFullAggregatedDataDisplay btcPrice, CoinFullAggregatedDataDisplay ethPrice, string coinImgUrl)
        {
            this.Info = coinInfo;
            this.BtcPrice = btcPrice;
            this.EthPrice = ethPrice;
            this.ImgUrl = coinImgUrl;
        }

        public CoinInfo Info { get; }
        public CoinFullAggregatedDataDisplay BtcPrice { get; }
        public CoinFullAggregatedDataDisplay EthPrice { get; }
        public string ImgUrl { get; }
    }
}