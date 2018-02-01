using System;
using Windows.UI.Xaml.Controls;

namespace matray.tradingview.uwp
{
    public class TradingViewChartControl
    {
        public static void Load(ref WebView webView)
        {
            var parameters = new ChartParameters
            {
                Width = Convert.ToInt32(webView.ActualWidth) - 20,
                Height = Convert.ToInt32(webView.ActualHeight) - 20
            };

            Load(ref webView, parameters);
        }

        private static void Load(ref WebView webView, ChartParameters cp)
        {
            var html = $@"
                <!DOCTYPE html>
                <html lang=""{cp.Locale}"" xmlns=""http://www.w3.org/1999/xhtml"">
                <head>
                    <meta charset=""utf-8""/>
                    <title></title>
                </head>
                <body>
                    <!--TradingView Widget BEGIN-->
                    <script type=""text/javascript"" src=""https://s3.tradingview.com/tv.js""></script>
                    <script type=""text/javascript"">
                        new TradingView.widget({{
                            ""width"": {cp.Width},
                            ""height"": {cp.Height},
                            ""symbol"": ""{cp.Symbol}"",
                            ""interval"": ""{cp.Interval}"",
                            ""timezone"": ""{cp.Timezone}"",
                            ""theme"": ""{cp.ThemeDisplay}"",
                            ""style"": ""{cp.Style}"",
                            ""locale"": ""{cp.Locale}"",
                            ""toolbar_bg"": ""{cp.ToolbarBg}"",
                            ""enable_publishing"": {cp.EnablePublishingDisplay},
                            ""hide_side_toolbar"": {cp.HideSideToolbarDisplay},
                            ""allow_symbol_change"": {cp.AllowSymbolChangeDisplay},
                            ""hideideas"": {cp.HideIdeasDisplay},
                            ""studies"": [{cp.StudiesDisplay}]
                        }});
                    </script>
                    <!--TradingView Widget END-->
                </body>
                </html>";

            webView.NavigateToString(html);
        }
    }
}