using Windows.UI.Xaml.Controls;

namespace Doji.Pages.UtilityPages.ExchangeUtility
{
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using GalaSoft.MvvmLight.Command;
    using Models;
    using Services;
    using ViewModels;

    public class ExchangeUtilityViewModel : MyViewModelBase
    {
        private readonly IDataService _dataService;

        public ExchangeUtilityViewModel(IDataService dataService)
        {
            this._dataService = dataService;

            this.Exchanges = new ObservableCollection<Exchange>();
            this.GotoExchangeDetailsPage = new RelayCommand<Exchange>(async args =>
                await Shell.Current.NavigateToPattern("WatchlistUtility", args));

            this.InitializeAsync();
        }

        public ObservableCollection<Exchange> Exchanges { get; }
        public RelayCommand<Exchange> GotoExchangeDetailsPage { get; }

        private async Task InitializeAsync()
        {
            this.IsLoading = true;

            var exchanges = await this._dataService.GetExchangesAsync();

            this.Exchanges.Clear();
            foreach (var e in exchanges)
            {
                this.Exchanges.Add(e);
            }

            this.IsLoading = false;
        }
    }
}
