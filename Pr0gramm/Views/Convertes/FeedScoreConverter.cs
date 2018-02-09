using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Pr0grammAPI.Feeds;


namespace Pr0gramm.Views.Convertes
{
    public class FeedScoreConverter : IValueConverter

    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is FeedItem)
            {
                var feedItem = value as FeedItem;
                return (feedItem.Up - feedItem.Down).ToString();
            }
            if (value is Comment)
            {
                var Comment = value as Comment;
                return (Comment.Up - Comment.Down).ToString();
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
