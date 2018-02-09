using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Pr0gramm.Views.Convertes
{
    public class ParentCommentMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return new Thickness(System.Convert.ToDouble(value) * 25, 0, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
