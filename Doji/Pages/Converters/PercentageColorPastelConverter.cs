using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Doji.Pages.Converters
{
    public class PercentageColorPastelConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var d = (decimal) value;
            SolidColorBrush result;
            if (d > 0)
            {
                result = new SolidColorBrush(Color.FromArgb(15, 0, 255, 0));
            }
            else if (d < 0)
            {
                result = new SolidColorBrush(Color.FromArgb(15, 255, 0, 0));
            }
            else
            {
                result = new SolidColorBrush(Colors.White);
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}