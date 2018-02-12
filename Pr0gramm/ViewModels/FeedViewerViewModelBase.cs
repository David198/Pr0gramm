using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Caliburn.Micro;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Pr0gramm.EventHandlers;
using Pr0gramm.Models;
using Pr0gramm.Services;
using Pr0grammAPI.Feeds;
using Pr0grammAPI.Interfaces;

namespace Pr0gramm.ViewModels
{
    public class FeedViewerViewModelBase : Screen, IHandle<RefreshEvent>, IHandle<SearchFeedItemsEvent>
    {
        protected readonly ToastNotificationsService ToastNotificationsService;
        protected readonly IEventAggregator EventAggregator;
        private bool _boolLoadingNewItems;
        private FeedItemViewModel _selectedItem;
        protected IProgrammApi ProgrammApi;

        protected string ActualSearchTags;

        internal FeedViewerViewModelBase(IProgrammApi programmProgrammApi, IEventAggregator eventAggregator,
            ToastNotificationsService toastNotificationsService)
        {
            ProgrammApi = programmProgrammApi;
            EventAggregator = eventAggregator;
            ToastNotificationsService = toastNotificationsService;
            ShowTop = true;
        }

        public bool ShowTop { get; set; }
        public BindableCollection<FeedItemViewModel> FeedItems { get; set; }

        public FeedItemViewModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                Set(ref _selectedItem, value);
                if (value != null)
                {
                    LoadComments(value);
                    var index = FeedItems.IndexOf(value);
                    if (index < FeedItems.Count - 3)
                    {
                        LoadComments(FeedItems[index + 1]);
                        LoadComments(FeedItems[index + 2]);
                        LoadComments(FeedItems[index + 3]);
                    }    
                }        
            }
        }

        private async void LoadComments(FeedItemViewModel itemViewModel)
        {
            await itemViewModel?.LoadCommentsAndTags();
        }

        public void Handle(RefreshEvent message)
        {
            if (message.CurrentViewType == GetType())
            {
                FeedItems.Clear();
                LoadFeedItems();
            }
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            EventAggregator.Subscribe(this);
            FeedItems = new BindableCollection<FeedItemViewModel>();
            LoadFeedItems();
        }



        private async void LoadFeedItems()
        {
            try
            {
                var feed = await ProgrammApi.GetFeed(FlagSelectorService.ActualFlag, ShowTop, ActualSearchTags);
                InitializeFeedItemViewModels(feed);
            }
            catch (ApplicationException)
            {
                ToastNotificationsService.ShowToastNotificationWebSocketExeception();
            }
        }

        public async void LoadOlderItems()
        {
            if (!_boolLoadingNewItems)
            {
                _boolLoadingNewItems = true;
                if (FeedItems.Count > 0)
                {
                    var id = ShowTop
                        ? FeedItems[FeedItems.Count - 1].Promoted
                        : FeedItems[FeedItems.Count - 1].Id;
                    try
                    {
                        var feed = await ProgrammApi.GetOlderFeed(id, FlagSelectorService.ActualFlag, ShowTop, ActualSearchTags);
                        InitializeFeedItemViewModels(feed);
                        _boolLoadingNewItems = false;
                    }
                    catch (ApplicationException)
                    {
                        ToastNotificationsService.ShowToastNotificationWebSocketExeception();
                        _boolLoadingNewItems = false;
                    }
                }
            }
        }

        public void SearchTag(TagViewModel tag)
        {
            Handle(new SearchFeedItemsEvent(tag.Tag));
            EventAggregator.PublishOnUIThread(new TagSearchEvent(tag.Tag));
        }

        private void InitializeFeedItemViewModels(Feed feed)
        {
            feed.Items.ForEach(item =>
            {
                FeedItems.Add(new FeedItemViewModel(item, ProgrammApi, ToastNotificationsService));
            });
        }

        public async void OpenLink(LinkClickedEventArgs e)
        {
            var uri = new Uri(e.Link);
             await Launcher.LaunchUriAsync(uri);
        }

        public  async void Handle(SearchFeedItemsEvent message)
        {
            FeedItems.Clear();
            ActualSearchTags = message.SearchTags;
            var feed = await ProgrammApi.GetFeed(FlagSelectorService.ActualFlag, ShowTop, ActualSearchTags);
            InitializeFeedItemViewModels(feed);
        }
    }
}
