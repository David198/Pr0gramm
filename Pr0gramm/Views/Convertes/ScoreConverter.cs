using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Pr0gramm.Models;
using Pr0grammAPI.Feeds;


namespace Pr0gramm.Views.Convertes
{
    public class ScoreConverter : IValueConverter

    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is FeedItem)
            {
                var feedItem = value as FeedItemViewModel;
                if(feedItem.ScoreIsAwailable)
                  return (feedItem.Up - feedItem.Down).ToString();
                else
                {
                    return "---";
                }
            }
            if (value is Comment)
            {
                var comment = value as CommentViewModel;
                if(comment.ScoreIsAwailable)
                 return (comment.Up - comment.Down).ToString();
                return "---";
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
