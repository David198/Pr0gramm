using System;
using Windows.UI.Xaml.Data;

namespace Pr0gramm.Views.Convertes
{
    public class UserTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!value.GetType().Equals(typeof(string))) return null;
            return new Uri("http://pr0gramm.com/user/" + value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
