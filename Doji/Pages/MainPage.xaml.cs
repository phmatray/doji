using Doji.ViewModels;

namespace Doji.Pages
{
    public sealed partial class MainPage
    {
        public MainViewModel VM => (MainViewModel)DataContext;

        public MainPage()
        {
            this.InitializeComponent();
        }
    }
}
