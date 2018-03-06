using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Caliburn.Micro;
using Pr0gramm.Helpers;
using Pr0gramm.Models.Enums;
using Pr0gramm.Services;
using Pr0grammAPI.Annotations;
using Pr0grammAPI.Feeds;
using Pr0grammAPI.Interfaces;

namespace Pr0gramm.Models
{
    public class FeedItemViewModel : FeedItem, INotifyPropertyChanged

    {
        private readonly IProgrammApi _programmApi;
        private readonly ToastNotificationsService _toastNotificationsService;
        private readonly CacheService _cacheService;

        public FeedItemViewModel(FeedItem feedItem, IProgrammApi api,
            ToastNotificationsService toastNotificationsService, CacheService cacheService) : base(feedItem)
        {
            _programmApi = api;
            _toastNotificationsService = toastNotificationsService;
            _cacheService = cacheService;
            _cacheService.RepostsChanged += (sender, args) => { OnPropertyChanged(nameof(IsRepost)); };
            CommentViewModels = new BindableCollection<CommentViewModel>();
            Tags = new BindableCollection<TagViewModel>();
            VoteState = Vote.Neutral;
        }

        private async void InitializeVoteStateAsync()
        {
            var cachedVote = await _cacheService.FindCachedVote(CacheVoteType.Item, Id);
            if (cachedVote != null)
            {
                VoteState = cachedVote.Vote;
                OnPropertyChanged(nameof(VoteState));
            }  
        }

        public Vote VoteState { get; set; }

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

        public void UpVote()
        {
            var oldState = VoteState;
            VoteState = VoteState == Vote.Up ? Vote.Neutral : Vote.Up;
            OnPropertyChanged(nameof(VoteState));
            _cacheService.SaveVote(CacheVoteType.Item, VoteState, Id);
            AdjustScore(oldState, VoteState);
            SaveToProApi();
        }

        public void DownVote()
        {
            var oldState = VoteState;
            VoteState = VoteState == Vote.Down ? Vote.Neutral : Vote.Down;
            OnPropertyChanged(nameof(VoteState));
            _cacheService.SaveVote(CacheVoteType.Item, VoteState, Id);
            AdjustScore(oldState, VoteState);
            SaveToProApi();
        }

        private void SaveToProApi()
        {
            try
            {
                _programmApi.VoteItem(Id, (int) VoteState);
            }
            catch (ApplicationException)
            {
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

        public string Score
        {
            get
            {
                if (ScoreIsAwailable)
                    return (Up - Down).ToString();
                return "---";
            }
        }

        private void AdjustScore(Vote oldState, Vote newState)
        {
            if (oldState == Vote.Neutral && newState == Vote.Up)
                Up++;
            if (oldState == Vote.Neutral && newState == Vote.Down)
                Down++;
            if (oldState == Vote.Up && newState == Vote.Neutral)
                Up--;
            if (oldState == Vote.Down && newState == Vote.Neutral)
                Down--;
            if (oldState == Vote.Up && newState == Vote.Down)
            {
                Up--;
                Down++;
            }

            if (oldState == Vote.Down && newState == Vote.Up)
            {
                Up++;
                Down--;
            }

            OnPropertyChanged(nameof(Up));
            OnPropertyChanged(nameof(Down));
            OnPropertyChanged(nameof(Score));
        }

        public bool IsRepost
        {
            get { return _cacheService.IsRepost(Id); }
        }

        private bool _loaded;

        public string CreatedString => DateTimeUtlis.MakeCreatedString(Created);

        public event PropertyChangedEventHandler PropertyChanged;

        public async Task LoadCommentsAndTagsAsync()
        {
            try
            {
                if (_loaded) return;
                _loaded = true;
                CommentViewModels.Clear();
                Tags.Clear();
                var feedItemCommentItem = await _programmApi.GetFeedItemComments(Id);
                Debug.WriteLine("Raw Comment Data loaded");
                var tempList = new List<Comment>();
                var rootNodes = Node<Comment>.CreateTree(feedItemCommentItem.Comments, l => l.Id,
                    l => l.Parent);
                rootNodes = rootNodes.OrderByDescending(o => o.Value.Confidence);
                foreach (var node in rootNodes)
                    FlatHierarchy(node, tempList);
                var temptCommentViewModel = new List<CommentViewModel>();
                tempList.ForEach(comment =>
                {
                    var commentViewModel = new CommentViewModel(comment, this, _cacheService, _programmApi);
                    commentViewModel.CalculateCommentDepth(tempList);
                    temptCommentViewModel.Add(commentViewModel);
                });
                CommentViewModels.AddRange(temptCommentViewModel);
                Debug.WriteLine("Comment Data loaded");
                var orderedTags = feedItemCommentItem.Tags.OrderByDescending(tag => tag.Confidence).ToList();
                var tempTagList = new List<TagViewModel>();
                foreach (var tag in orderedTags)
                {
                    tempTagList.Add(new TagViewModel(tag, _programmApi, _cacheService));
                }
                Tags.AddRange(tempTagList);              
                Debug.WriteLine("Tag Data loaded");              
            }
            catch (ApplicationException)
            {
                _loaded = false;
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

        public void LoadTagAndCommentVotesAsync()
        {
            InitializeVoteStateAsync();
            Debug.WriteLine("FeedItem VoteState Loaded");
            foreach (var tagView in Tags)
            {
                  tagView.InitializeVoteStateAsync();
            }
            Debug.WriteLine("tags votestate loaded");
            foreach (var comment in CommentViewModels)
            {
                  comment.InitializeVoteStateAsync();
            }
            Debug.WriteLine("comments votestate loaded");
        }
    }
}
