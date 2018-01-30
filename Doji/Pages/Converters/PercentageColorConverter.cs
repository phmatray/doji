using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Doji.Pages.Converters
{
    public class PercentageColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var d = (decimal) value;
            SolidColorBrush result;
            if (d > 0)
            {
                result = new SolidColorBrush(Colors.DarkGreen);
            }
            else if (d < 0)
            {
                result = new SolidColorBrush(Colors.DarkRed);
            }
            else
            {
                result = new SolidColorBrush(Colors.Black);
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}