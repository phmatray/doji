// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

namespace Doji.PatternPages
{
    using matray.tradingview.uwp;
    using Windows.UI.Xaml.Controls;

    public sealed partial class AbandonedBabyBottomPatternPage : Page
    {
        public AbandonedBabyBottomPatternPage()
        {
            this.InitializeComponent();
            this.Loaded += AbandonedBabyBottomPatternPage_Loaded;
        }

        private void AbandonedBabyBottomPatternPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            TradingViewChartControl.Load(ref mywebview);
        }
    }
}
