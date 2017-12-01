using System;
using Windows.UI.Xaml.Data;

namespace Doji.Pages.Converters
{
    public class PriceFormatConverter
        : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return $"{(decimal) value:F8}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}