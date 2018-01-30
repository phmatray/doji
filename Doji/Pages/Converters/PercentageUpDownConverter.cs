using System;
using Windows.UI.Xaml.Data;

namespace Doji.Pages.Converters
{
    public class PercentageUpDownConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var d = (decimal)value;

            var sign = d > 0 ? "+" : string.Empty;
            var result = $"{sign}{d}%";

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
