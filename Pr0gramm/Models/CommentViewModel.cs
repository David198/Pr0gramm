using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Caliburn.Micro;
using Pr0gramm.EventHandlers;
using Pr0gramm.Helpers;
using Pr0gramm.Models.Enums;
using Pr0gramm.Services;
using Pr0grammAPI.Annotations;
using Pr0grammAPI.Feeds;
using Pr0grammAPI.Interfaces;

namespace Pr0gramm.Models
{
    public class CommentViewModel : Comment, INotifyPropertyChanged
    {
        private readonly FeedItem _parentFeedItem;
        private readonly CacheVoteService _cacheVoteService;
        private readonly IProgrammApi _iProgrammApi;
        private int _parentDepth;

        public CommentViewModel(Comment comment, FeedItem parentFeedItem, CacheVoteService cacheVoteService,
            IProgrammApi iProgrammApi) : base(comment)
        {
            _parentFeedItem = parentFeedItem;
            _cacheVoteService = cacheVoteService;
            _iProgrammApi = iProgrammApi;
            if (comment.Parent == 0) comment.Parent = null;
            ParentDepthList = new BindableCollection<int>();
        }

        public Vote VoteState { get; set; }

        public BindableCollection<int> ParentDepthList { get; set; }

        public bool IsCreatedByOP => Name.Equals(_parentFeedItem.User);

        public int ParentDepth
        {
            get => _parentDepth;
            set
            {
                if (value == _parentDepth) return;
                _parentDepth = value;
                for (var i = 1; i < value; i++)
                    ParentDepthList.Add(0);
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

        public string CreatedString => DateTimeUtlis.MakeCreatedString(Created);

        public event PropertyChangedEventHandler PropertyChanged;

        public void CalculateCommentDepth(List<Comment> comments)
        {
            if (Parent == null) return;
            var currentComment = (Comment) this;
            var depth = 0;

            while (true)
            {
                depth++;
                currentComment = comments.FirstOrDefault(comment => comment.Id.Equals(currentComment.Parent));
                if (currentComment == null) break;
            }

            ParentDepth = depth;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void UpVote()
        {
            var oldState = VoteState;
            VoteState = VoteState == Vote.Up ? Vote.Neutral : Vote.Up;
            OnPropertyChanged(nameof(VoteState));
            _cacheVoteService.SaveVote(CacheVoteType.Item, VoteState, Id);
            AdjustScore(oldState, VoteState);
            SaveToProApi();
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
            if(oldState == Vote.Up && newState == Vote.Down)
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

        public void DownVote()
        {
            var oldState = VoteState;
            VoteState = VoteState == Vote.Down ? Vote.Neutral : Vote.Down;
            OnPropertyChanged(nameof(VoteState));
            _cacheVoteService.SaveVote(CacheVoteType.Item, VoteState, Id);
            AdjustScore(oldState, VoteState);
            SaveToProApi();
        }

        private void SaveToProApi()
        {
            try
            {
                _iProgrammApi.VoteComment(Id, (int) VoteState);
            }
            catch (ApplicationException)
            {
            }
        }

        public async Task InitializeVoteState()
        {
            VoteState = Vote.Neutral;
            var cachedVote = await _cacheVoteService.Find(CacheVoteType.Comment, Id);
            if (cachedVote != null)
            {
                VoteState = cachedVote.Vote;
            }

            OnPropertyChanged(nameof(VoteState));
        }
    }
}
