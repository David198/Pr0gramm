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
    public class FeedViewerViewModelBase : Screen, IHandle<RefreshEvent>
    {
        private readonly ToastNotificationsService _toastNotificationsService;
        protected readonly IEventAggregator EventAggregator;
        private bool _boolLoadingNewItems;
        private FeedItemViewModel _selectedItem;
        protected IProgrammApi Api;

        internal FeedViewerViewModelBase(IProgrammApi programmApi, IEventAggregator eventAggregator,
            ToastNotificationsService toastNotificationsService)
        {
            Api = programmApi;
            EventAggregator = eventAggregator;
            _toastNotificationsService = toastNotificationsService;
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
                if(value!=null)
                 LoadComments(value);
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

        private async void InitializeTheme()
        {
            await ThemeSelectorService.InitializeAsync();
            ThemeSelectorService.SetRequestedTheme();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            InitializeTheme();
            EventAggregator.Subscribe(this);
            FeedItems = new BindableCollection<FeedItemViewModel>();
            LoadFeedItems();
        }

        private async void LoadFeedItems()
        {
            try
            {
                var feed = await Api.GetFeed(FeedFlags.SFW, ShowTop);
                InitializeFeedItemViewModels(feed);
            }
            catch (ApplicationException)
            {
                _toastNotificationsService.ShowToastNotificationWebSocketExeception();
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
                        ? FeedItems[FeedItems.Count - 1].FeedItem.Promoted
                        : FeedItems[FeedItems.Count - 1].FeedItem.Id;
                    try
                    {
                        var feed = await Api.GetOlderFeed(id, FeedFlags.SFW, ShowTop);
                        InitializeFeedItemViewModels(feed);
                        _boolLoadingNewItems = false;
                    }
                    catch (ApplicationException)
                    {
                        _toastNotificationsService.ShowToastNotificationWebSocketExeception();
                    }
                }
            }
        }

        private void InitializeFeedItemViewModels(Feed feed)
        {
            feed.Items.ForEach(item =>
            {
                FeedItems.Add(new FeedItemViewModel(item, Api, _toastNotificationsService));
            });
        }

        public async void OpenLink(LinkClickedEventArgs e)
        {
            var uri = new Uri(e.Link);
            var success = await Launcher.LaunchUriAsync(uri);
            if (!success)
            {
// URI launched
            }
        }
    }
}
