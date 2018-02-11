﻿using System;
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
        protected IProgrammApi ProgrammApi;

        internal FeedViewerViewModelBase(IProgrammApi programmProgrammApi, IEventAggregator eventAggregator,
            ToastNotificationsService toastNotificationsService)
        {
            ProgrammApi = programmProgrammApi;
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

        private async void InitializeTheme()
        {
            await ThemeSelectorService.InitializeAsync();
            ThemeSelectorService.SetRequestedTheme();
        }

        protected override async void OnInitialize()
        {
            base.OnInitialize();
            InitializeTheme();
            EventAggregator.Subscribe(this);
            FeedItems = new BindableCollection<FeedItemViewModel>();
            var credentials = UserLoginService.GetCredentialFromLocker();
            if (credentials != null && !UserLoginService.IsLoggedIn)
            {
                credentials.RetrievePassword();
                var user = await ProgrammApi.Login(credentials.UserName, credentials.Password);
                if (user != null)
                {
                    EventAggregator.PublishOnUIThread(new UserLoggedInEvent(credentials.UserName));
                    UserLoginService.IsLoggedIn = true;
                }
                    
            }
            LoadFeedItems();
        }

        private async void LoadFeedItems()
        {
            try
            {
                var feed = await ProgrammApi.GetFeed(FlagSelectorService.ActualFlag, ShowTop);
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
                        var feed = await ProgrammApi.GetOlderFeed(id, FlagSelectorService.ActualFlag, ShowTop);
                        InitializeFeedItemViewModels(feed);
                        _boolLoadingNewItems = false;
                    }
                    catch (ApplicationException)
                    {
                        _toastNotificationsService.ShowToastNotificationWebSocketExeception();
                        _boolLoadingNewItems = false;
                    }
                }
            }
        }

        private void InitializeFeedItemViewModels(Feed feed)
        {
            feed.Items.ForEach(item =>
            {
                FeedItems.Add(new FeedItemViewModel(item, ProgrammApi, _toastNotificationsService));
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
