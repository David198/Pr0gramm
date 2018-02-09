using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Pr0gramm.Views.Convertes
{
    internal class BooleanToVisibilityConverterInverse : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var flag = false;
            if (value is bool)
                flag = (bool) value;
            return flag ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
