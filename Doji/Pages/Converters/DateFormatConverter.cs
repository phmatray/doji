using System;
using Windows.UI.Xaml.Data;

namespace Doji.Pages.Converters
{
    public class DateFormatConverter
        : IValueConverter
    {
        #region Implementation of IValueConverter

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return $"{((DateTime) value):MM/dd/yyyy hh:mm tt}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}