using System;
using Windows.Media.Core;
using Windows.UI.Xaml.Data;

namespace Pr0gramm.Views.Convertes
{
    public class MediaSourceFromUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value.ToString().Contains(".mp4"))
                return MediaSource.CreateFromUri(new Uri(value.ToString()));
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
