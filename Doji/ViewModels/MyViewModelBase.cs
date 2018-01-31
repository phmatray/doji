namespace Doji.ViewModels
{
    using Bittrex.Net;
    using CryptoCompare;
    using GalaSoft.MvvmLight;

    public class MyViewModelBase : ViewModelBase
    {
        protected bool _isLoading;

        public bool IsLoading
        {
            get => this._isLoading;
            set => Set(ref this._isLoading, value);
        }

        protected static BittrexClient BittrexClient { get; } = new BittrexClient();
        protected static CryptoCompareClient CryptoCompareClient { get; } = CryptoCompareClient.Instance;
    }
}