using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Caliburn.Micro;
using Microsoft.HockeyApp;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Pr0gramm.EventHandlers;
using Pr0gramm.Helpers;
using Pr0gramm.Models;
using Pr0gramm.Services;
using Pr0grammAPI.Feeds;
using Pr0grammAPI.Interfaces;

namespace Pr0gramm.ViewModels
{
    public class FeedViewerViewModelBase : Screen, IHandle<RefreshEvent>, IHandle<SearchFeedItemsEvent>, IHandle<MuteEvent>
    {
        protected readonly ToastNotificationsService ToastNotificationsService;
        private readonly CacheVoteService _cacheVoteService;
        protected readonly IEventAggregator EventAggregator;
        private bool _boolLoadingNewItems;
        private FeedItemViewModel _selectedItem;
        protected IProgrammApi ProgrammApi;

        protected string ActualSearchTags;
        private bool _isMuted;
        private bool _isBusy;

        internal FeedViewerViewModelBase(IProgrammApi programmProgrammApi, IEventAggregator eventAggregator,
            ToastNotificationsService toastNotificationsService, SettingsService settingsService, CacheVoteService cacheVoteService)
        {
            ProgrammApi = programmProgrammApi;
            EventAggregator = eventAggregator;
            ToastNotificationsService = toastNotificationsService;
            _cacheVoteService = cacheVoteService;
            ShowTop = true;
            EventAggregator.Subscribe(this);
            FeedItems = new BindableCollection<FeedItemViewModel>();
            IsMuted = settingsService.IsMuted;
        }

        public bool ShowTop { get; set; }
        public BindableCollection<FeedItemViewModel> FeedItems { get; set; }

        public bool IsMuted
        {
            get { return _isMuted; }
            set => Set(ref _isMuted, value);
        }

        public FeedItemViewModel SelectedItem
        {
            get => _selectedItem;
            set
            {
                Set(ref _selectedItem, value);
                value?.LoadTagAndCommentVotesAsync();
            }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                if (value == _isBusy) return;
                _isBusy = value;
                NotifyOfPropertyChange(() => IsBusy);
            }
        }

        public void Handle(MuteEvent message)
        {
            IsMuted = message.IsMuted;
        }

        private async Task LoadComments(FeedItemViewModel itemViewModel)
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

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            LoadFeedItems();
        }


        private async void LoadFeedItems()
        {
            try
            {
                IsBusy = true;
                var feed = await ProgrammApi.GetFeed(FlagSelectorService.ActualFlag, ShowTop, ActualSearchTags);
                InitializeFeedItemViewModelsAsync(feed);
                
            }
            catch (ApplicationException)
            {
                ToastNotificationsService.ShowToastNotificationWebSocketExeception();
                IsBusy = false;
            }
        }

        public async void LoadOlderItems()
        {
            IsBusy = true;
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
                        InitializeFeedItemViewModelsAsync(feed);
                        _boolLoadingNewItems = false;
                    }
                    catch (ApplicationException)
                    {
                        ToastNotificationsService.ShowToastNotificationWebSocketExeception();
                        _boolLoadingNewItems = false;
                        IsBusy = false;
                    }
                }
            }
        }

        public void SearchTag(TagViewModel tag)
        {
            Handle(new SearchFeedItemsEvent(tag.Tag));
            EventAggregator.PublishOnUIThread(new TagSearchEvent(tag.Tag));
        }

        private void InitializeFeedItemViewModelsAsync(Feed feed)
        {
            var newList = new List<FeedItemViewModel>();
            feed.Items.ForEach(item =>
            {
                newList.Add(new FeedItemViewModel(item, ProgrammApi, ToastNotificationsService, _cacheVoteService, EventAggregator));
            });
            FeedItems.AddRange(newList);
            IsBusy = false;
            for (int i = 0; i < newList.Count; i++)
            {
                LoadComments(newList[i]);
            }
        }

        public async void Handle(SearchFeedItemsEvent message)
        {
            FeedItems.Clear();
            IsBusy = true;
            ActualSearchTags = message.SearchTags;
            try
            {
                var feed = await ProgrammApi.GetFeed(FlagSelectorService.ActualFlag, ShowTop, ActualSearchTags);
                InitializeFeedItemViewModelsAsync(feed);
            }
            catch (ApplicationException)
            {
                IsBusy = false;
            }

        }

        public void ShareSelectedItem()
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            if (DataTransferManager.IsSupported())
            {
                dataTransferManager.DataRequested += DataTransferManager_DataRequested;
                DataTransferManager.ShowShareUI();
            }
        }

        public async void DownloadSelectedItem()
        {
            var savePicker = new FileSavePicker { SuggestedStartLocation = PickerLocationId.PicturesLibrary };
            if (!SelectedItem.IsVideo)
            {
                savePicker.FileTypeChoices.Add("Image", new List<string>() { ".jpg" });
                savePicker.SuggestedFileName = "NewImage";
            }
            else
            {
                savePicker.FileTypeChoices.Add("Video", new List<string>() { ".mp4" });
                savePicker.SuggestedFileName = "NewVideo";
            }
            var savefile = await savePicker.PickSaveFileAsync();
            if (savefile == null)
                return;
            await SaveFileAsync(SelectedItem.FullSizeSource, savefile);
        }

        private async Task SaveFileAsync(Uri fileUri, StorageFile file)
        {
            try
            {
                var backgroundDownloader = new BackgroundDownloader();
                var downloadOperation = backgroundDownloader.CreateDownload(fileUri, file);
                await downloadOperation.StartAsync();
                ToastNotificationsService.ShowToastNotificationDownloadSucceded();
            }
            catch (Exception)
            {
                ToastNotificationsService.ShowToastNotificationDownloadFailed();
            }
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.RequestedOperation = DataPackageOperation.Link;
            request.Data.Properties.Title = "SharePostTitel".GetLocalized();
            request.Data.Properties.Description = "SharePostDescription".GetLocalized();
            request.Data.SetWebLink(SelectedItem.ShareLink);
        }


        // Dirty Hack... not solved yet in xaml. Normally should directly call the Upvote method in the FeedItemViewModel
        public void UpVote()
        {
            SelectedItem.UpVote();
        }

        // Dirty Hack... not solved yet in xaml. Normally should directly call the DownVote method in the FeedItemViewModel
        public void DownVote()
        {
            SelectedItem.DownVote();
        }

        public async void OpenLink(LinkClickedEventArgs e)
        {
            var uri = new Uri(e.Link);
            await Launcher.LaunchUriAsync(uri);
        }

     
    }
}
