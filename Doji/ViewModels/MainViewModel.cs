using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Bittrex.Net.Objects;
using GalaSoft.MvvmLight;
using Newtonsoft.Json;

namespace Doji.ViewModels
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
                LoadAsync();
                //LoadDesignData();
            }
            else
            {
                // Code runs "for real"
                LoadAsync();
            }
        }

        private string _hello;
        public string Hello
        {
            get { return _hello; }
            set { Set(ref _hello, value); }
        }

        private List<BittrexCandle> _candles;
        public List<BittrexCandle> Candles
        {
            get { return _candles; }
            set { Set(ref _candles, value); }
        }

        private void LoadDesignData()
        {
            Hello = "Welcome to Doji !!";
            Candles = new List<BittrexCandle>();
        }

        private async Task LoadAsync()
        {
            Uri appUri = new Uri("ms-appx:///Assets/btc-ada.json");
            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(appUri);
            string json = await FileIO.ReadTextAsync(file);
            var candles = JsonConvert.DeserializeObject<BittrexCandle[]>(json)
                .TakeLast(96)
                .Reverse()
                .ToList();

            Hello = "Welcome to Doji !!";
            Candles = candles;
        }
    }
}