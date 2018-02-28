using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using Pr0gramm.EventHandlers;
using Pr0gramm.Models.Enums;
using Pr0gramm.Services;
using Pr0grammAPI.Annotations;
using Pr0grammAPI.Feeds;
using Pr0grammAPI.Interfaces;

namespace Pr0gramm.Models
{
    public class TagViewModel : TagItem, INotifyPropertyChanged
    {
        private readonly IEventAggregator _iEventAggregator;
        private readonly IProgrammApi _iProgrammApi;
        private readonly CacheVoteService _cacheVoteService;

        public TagViewModel(TagItem item, IEventAggregator iEventAggregator, IProgrammApi iProgrammApi, CacheVoteService cacheVoteService) : base(item)
        {
            _iEventAggregator = iEventAggregator;
            _iProgrammApi = iProgrammApi;
            _cacheVoteService = cacheVoteService;
        }

        public Vote VoteState { get; set; }

        public void UpVote()
        {
            VoteState = VoteState == Vote.Up ? Vote.Neutral : Vote.Up;
            OnPropertyChanged(nameof(VoteState));
            _cacheVoteService.SaveVote(CacheVoteType.Tag, VoteState, Id);
            SaveToProApi();
        }

        private void SaveToProApi()
        {
            try
            {
                _iProgrammApi.VoteTag(Id, (int) VoteState);
            }
            catch (ApplicationException)
            {
            }
        }

        public void DownVote()
        {
            VoteState = VoteState == Vote.Down ? Vote.Neutral : Vote.Down;
            OnPropertyChanged(nameof(VoteState));
            _cacheVoteService.SaveVote(CacheVoteType.Tag, VoteState, Id);
            SaveToProApi();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task InitializeVoteState()
        {
            VoteState = Vote.Neutral;
            var cachedVote = await _cacheVoteService.Find(CacheVoteType.Tag, Id);
            if (cachedVote != null)
            {
                VoteState = cachedVote.Vote;
            }
            OnPropertyChanged(nameof(VoteState));
        }
    }
}
