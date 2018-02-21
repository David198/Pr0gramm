using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Core;
using Caliburn.Micro;
using Pr0gramm.Helpers;
using Pr0gramm.Services;
using Pr0grammAPI.Annotations;
using Pr0grammAPI.Feeds;
using Pr0grammAPI.Interfaces;

namespace Pr0gramm.Models
{
    public class FeedItemViewModel : FeedItem, INotifyPropertyChanged

    {
        private readonly IProgrammApi _api;
        private readonly ToastNotificationsService _toastNotificationsService;
        private bool _commentsAndTagsLoaded;

        public FeedItemViewModel(FeedItem feedItem, IProgrammApi api,
            ToastNotificationsService toastNotificationsService) : base(feedItem)
        {
            _api = api;
            _toastNotificationsService = toastNotificationsService;
            CommentViewModels = new BindableCollection<CommentViewModel>();
            Tags = new BindableCollection<TagViewModel>();
        }


        public int ParentCount { get; set; }

        public BindableCollection<CommentViewModel> CommentViewModels { get; set; }

        public BindableCollection<TagViewModel> Tags { get; set; }

        public Uri ShareLink
        {
            get
            {
                var link = "https://pr0gramm.com/";
                if (Promoted != 0)
                    link = link + "top/" + Id;
                else
                {
                    link = link + "new/" + Id;
                }
                return new Uri(link);
            }
        }

        public bool ScoreIsAwailable
        {
            get
            {
                if ((DateTime.Now - Created).TotalHours > 1)
                    return true;
                return false;
            }
        }

        public bool IsRepost
        {
            get { return Tags.Any(tag => tag.Tag.ToLower().Equals("repost")); }
        }

        public string CreatedString => DateTimeUtlis.MakeCreatedString(Created);

        public event PropertyChangedEventHandler PropertyChanged;

        public async Task LoadCommentsAndTags()
        {
            if (_commentsAndTagsLoaded)
                return;
            _commentsAndTagsLoaded = true;
            try
            {
                CommentViewModels.Clear();
                Tags.Clear();
                var feedItemCommentItem = await _api.GetFeedItemComments(Id);
                var tempList = new List<Comment>();
                var rootNodes = Node<Comment>.CreateTree(feedItemCommentItem.Comments, l => l.Id, l => l.Parent);
                rootNodes = rootNodes.OrderByDescending(o => o.Value.Confidence);
                foreach (var node in rootNodes)
                    FlatHierarchy(node, tempList);
                var temptCommentViewModel = new List<CommentViewModel>();
                tempList.ForEach(comment =>
                {
                    var commentViewModel = new CommentViewModel(comment, this);
                    commentViewModel.CalculateCommentDepth(tempList);
                    temptCommentViewModel.Add(commentViewModel);
                });

                CommentViewModels.AddRange(temptCommentViewModel);
                feedItemCommentItem.Tags.OrderByDescending(tag => tag.Confidence).ToList()
                    .ForEach(item => Tags.Add(new TagViewModel(item)));
                OnPropertyChanged(nameof(IsRepost));
            }
            catch (ApplicationException)
            {
                _toastNotificationsService.ShowToastNotificationWebSocketExeception();
                _commentsAndTagsLoaded = false;
            }
        }

        public void FlatHierarchy(Node<Comment> node, List<Comment> tempList)
        {
            if (node == null) return;
            if (!tempList.Contains(node.Value))
                tempList.Add(node.Value);
            if (!node.Children.Any())
                return;
            foreach (var child in node.Children.OrderByDescending(ch => ch.Value.Confidence))
            {
                tempList.Add(child.Value);
                FlatHierarchy(child, tempList);
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
