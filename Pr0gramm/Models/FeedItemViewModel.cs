using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.Core;
using Caliburn.Micro;
using Pr0gramm.Services;
using Pr0grammAPI.Annotations;
using Pr0grammAPI.Feeds;
using Pr0grammAPI.Interfaces;

namespace Pr0gramm.Models
{
    public class FeedItemViewModel : INotifyPropertyChanged

    {
        private readonly IProgrammApi _api;
        private readonly ToastNotificationsService _toastNotificationsService;
        private bool _commentsAndTagsLoaded;
        private FeedItem _feedItem;

        public FeedItemViewModel(FeedItem feedItem, IProgrammApi api,
            ToastNotificationsService toastNotificationsService)
        {
            FeedItem = feedItem;
            _api = api;
            _toastNotificationsService = toastNotificationsService;
            CommentViewModels = new BindableCollection<CommentViewModel>();
            Tags = new BindableCollection<TagItem>();
        }

        public FeedItem FeedItem
        {
            get => _feedItem;
            set
            {
                _feedItem = value;
                OnPropertyChanged(nameof(FeedItem));
            }
        }

        public int ParentCount { get; set; }

        public BindableCollection<CommentViewModel> CommentViewModels { get; set; }

        public BindableCollection<TagItem> Tags { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public async Task LoadCommentsAndTags()
        {
            if (_commentsAndTagsLoaded)
                return;
            _commentsAndTagsLoaded = true;

                CoreDispatcher dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    CommentViewModels.Clear();
                    Tags.Clear();
                    var feedItemCommentItem = await _api.GetFeedItemComments(FeedItem.Id);
                    var tempList = new List<Comment>();
                    var rootNodes = Node<Comment>.CreateTree(feedItemCommentItem.Comments, l => l.Id, l => l.Parent);
                    rootNodes = rootNodes.OrderByDescending(o => o.Value.Confidence);
                    foreach (var node in rootNodes)
                        FlatHierarchy(node, tempList);
                    var temptCommentViewModel = new List<CommentViewModel>();
                    tempList.ForEach(comment =>
                    {
                        var commentViewModel = new CommentViewModel(comment);
                        commentViewModel.CalculateCommentDepth(tempList);
                        temptCommentViewModel.Add(commentViewModel);
                    });

                    CommentViewModels.AddRange(temptCommentViewModel);
                    Tags.AddRange(feedItemCommentItem.Tags.OrderByDescending(tag => tag.Confidence).ToList());
                }
                catch (ApplicationException)
                {
                    _toastNotificationsService.ShowToastNotificationWebSocketExeception();
                }


            });

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
