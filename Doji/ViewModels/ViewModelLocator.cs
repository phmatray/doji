/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocator xmlns:vm="clr-namespace:Doji.WPF"
                           x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

  You can also use Blend to do all this with the tool's support.
  See http://www.galasoft.ch/mvvm
*/

namespace Doji.ViewModels
{
    using GalaSoft.MvvmLight;
    using GalaSoft.MvvmLight.Ioc;
    using GalaSoft.MvvmLight.Views;
    using Microsoft.Practices.ServiceLocation;
    using Pages.UtilityPages.ExchangeUtility;
    using Pages.UtilityPages.GlossaryUtility;
    using Pages.UtilityPages.WatchlistUtility;
    using Services;

    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// </summary>
    public class ViewModelLocator
    {
        /// <summary>
        /// Initializes a new instance of the ViewModelLocator class.
        /// </summary>
        public ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            var nav = new NavigationService();
            //nav.Configure(SecondPageKey, typeof(SecondPage));
            SimpleIoc.Default.Register<INavigationService>(() => nav);

            SimpleIoc.Default.Register<IDialogService, DialogService>();
            SimpleIoc.Default.Register<IGitHubService, GitHubService>();

            if (ViewModelBase.IsInDesignModeStatic)
            {
                SimpleIoc.Default.Register<IDataService, DesignDataService>();
            }
            else
            {
                SimpleIoc.Default.Register<IDataService, DataService>();
            }

            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<WatchlistUtilityViewModel>();
            SimpleIoc.Default.Register<GlossaryUtilityViewModel>();
            SimpleIoc.Default.Register<ExchangeUtilityViewModel>();
        }

        public MainViewModel Main
            => ServiceLocator.Current.GetInstance<MainViewModel>();

        public WatchlistUtilityViewModel WatchlistUtility
            => ServiceLocator.Current.GetInstance<WatchlistUtilityViewModel>();

        public GlossaryUtilityViewModel GlossaryUtility
            => ServiceLocator.Current.GetInstance<GlossaryUtilityViewModel>();

        public ExchangeUtilityViewModel ExchangeUtility
            => ServiceLocator.Current.GetInstance<ExchangeUtilityViewModel>();

        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}