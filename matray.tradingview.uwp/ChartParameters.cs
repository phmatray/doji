using System.Collections.Generic;
using System.Linq;
using matray.tradingview.uwp.Enums;

namespace matray.tradingview.uwp
{
    public class ChartParameters
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public string Symbol { get; set; } = "BITTREX:ETHBTC";
        public string Interval { get; set; } = "D";
        public string Timezone { get; set; } = "Etc/UTC";
        public Theme Theme { get; set; } = Theme.Light;
        public string Style { get; set; } = "1";
        public string Locale { get; set; } = "en";
        public string ToolbarBg { get; set; } = "rgba(255, 255, 255, 1)";
        public bool EnablePublishing { get; set; } = false;
        public bool HideSideToolbar { get; set; } = false;
        public bool AllowSymbolChange { get; set; } = true;
        public bool HideIdeas { get; set; } = true;

        public List<Study> Studies { get; set; }

        public ChartParameters()
        {
            Studies = new List<Study> {Study.StochasticRSI, Study.BB};
        }

        internal string ThemeDisplay => Theme.ToString();
        internal string EnablePublishingDisplay => EnablePublishing.ToString().ToLower();
        internal string HideSideToolbarDisplay => HideSideToolbar.ToString().ToLower();
        internal string AllowSymbolChangeDisplay => AllowSymbolChange.ToString().ToLower();
        internal string HideIdeasDisplay => HideIdeas.ToString().ToLower();

        internal string StudiesDisplay
        {
            get
            {
                if (!Studies.Any())
                    return "";

                return Studies
                    .Select(x => $"\"{x.ToString()}@tv-basicstudies\"")
                    .Aggregate((current, next) => current + ", " + next);
            }
        }
    }
}