using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Caliburn.Micro;
using Microsoft.HockeyApp;
using Microsoft.Toolkit.Uwp.UI.Animations;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Pr0gramm.EventHandlers;
using Pr0gramm.Helpers;
using Pr0gramm.Models;
using Pr0gramm.Services;
using Pr0gramm.Views;
using Pr0gramm.Views.Convertes;
using Pr0grammAPI.Annotations;
using RestSharp;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Pr0gramm.Controls
{
    public sealed partial class FeedViewer : UserControl, INotifyPropertyChanged, IHandle<MuteEvent>
    {
        public static readonly DependencyProperty FeedItemsProperty = DependencyProperty.Register(
            "FeedItems", typeof(BindableCollection<FeedItemViewModel>), typeof(FeedViewer),
            new PropertyMetadata(default(BindableCollection<FeedItemViewModel>)));

        public static readonly DependencyProperty SelectedFeedItemProperty = DependencyProperty.Register(
            "PropertyType", typeof(FeedItemViewModel), typeof(FeedViewer),
            new PropertyMetadata(default(FeedItemViewModel)));

        public FeedViewer()
        {
            InitializeComponent();
            IoC.Get<IEventAggregator>().Subscribe(this);
            _settingsService = IoC.Get<SettingsService>();
            _toastNotificationService = IoC.Get<ToastNotificationsService>();
            DataContext = this;
        }


        private bool IsMuted { get; set; }
        private MediaPlayer _mediaPlayer;
        private readonly SettingsService _settingsService;
        private ScrollViewer _flipViewScrollViewer;
        private GridLength _flipViewMainColumnWidth;
        private GridLength _flipViewExtraColumnWidth;
        private ToastNotificationsService _toastNotificationService;

        public GridLength FlipViewExtraColumnGridSplitterWidth { get; set; }

        public GridLength FlipViewExtraColumnWidth
        {
            get { return _flipViewExtraColumnWidth; }
            set
            {
                _flipViewExtraColumnWidth = value;
                OnPropertyChanged(nameof(FlipViewExtraColumnWidth));
            }
        }

        public GridLength FlipViewMainColumnWidth
        {
            get { return _flipViewMainColumnWidth; }
            set
            {
                _flipViewMainColumnWidth = value; 
                OnPropertyChanged(nameof(FlipViewMainColumnWidth));
            }
        }

        public bool ExtraColumnIsActive { get; set; }

        public BindableCollection<FeedItemViewModel> FeedItems
        {
            get => (BindableCollection<FeedItemViewModel>) GetValue(FeedItemsProperty);
            set => SetValue(FeedItemsProperty, value);
        }

        public FeedItemViewModel SelectedFeedItem
        {
            get => (FeedItemViewModel) GetValue(SelectedFeedItemProperty);
            set
            {
                SetValue(SelectedFeedItemProperty, value);
                if (value != null)
                {
                    SetVideoPlayBack(value);
                    FeedItemGridView.ScrollIntoView(value);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler LoadNewItems;

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            RegisterFeedScrollViewUpdate();
            SetMainViewColumnWidth();
            SetExtraCommentColumn();
        }

        private void RegisterFeedScrollViewUpdate()
        {
            var scrollViewer = FeedItemGridView.FindDescendant<ScrollViewer>();
            scrollViewer.ViewChanged += ScrollViewer_OnViewChanged;
        }

        private void SetMainViewColumnWidth()
        {
            FeedItemColumn.Width = new GridLength(_settingsService.FeedViewerRightGridColumnWidth, GridUnitType.Star);
            FeedColumn.Width = new GridLength(_settingsService.FeedViewerLeftGridColumnWidth, GridUnitType.Star);
        }

        private void SetExtraCommentColumn()
        {
         
            ExtraColumnIsActive = _settingsService.FeedViewerExtraColumnVisible;
            if (ExtraColumnIsActive)
            {
                FlipViewMainColumnWidth = new GridLength(_settingsService.FeedViewerExtraLeftGridColumnWidth,
                    GridUnitType.Star);
                FlipViewExtraColumnGridSplitterWidth = new GridLength(15, GridUnitType.Pixel);
                FlipViewExtraColumnWidth = new GridLength(_settingsService.FeedViewerExtraRightGridColumnWidth,
                    GridUnitType.Star);
                FlipView.UpdateLayout();
            }
            else
            {
                FlipViewMainColumnWidth = new GridLength(1, GridUnitType.Star);
                FlipViewExtraColumnGridSplitterWidth = new GridLength(0, GridUnitType.Pixel);
                FlipViewExtraColumnWidth = new GridLength(0, GridUnitType.Pixel);
            }
        }

        private void ScrollViewer_OnViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            var scrollViewer = (ScrollViewer) sender;
            var scrollViewerReachedBottom =
                Math.Abs(scrollViewer.VerticalOffset - scrollViewer.ScrollableHeight) < 0.000000000001f;
            if (scrollViewerReachedBottom)
                OnLoadNewItems(new EventArgs());
        }

        private void OnLoadNewItems(EventArgs e)
        {
            LoadNewItems?.Invoke(this, e);
        }


        private void SetVideoPlayBack(FeedItemViewModel item)
        {
            if (item.IsVideo)
            {
                StopOldMediaPlayer();
                _mediaPlayer = new MediaPlayer
                {
                    IsLoopingEnabled = true,
                    Source = MediaSource.CreateFromUri(item.ImageSource),
                    IsMuted = _settingsService.IsMuted,
                    AutoPlay = true,
                    AudioCategory = MediaPlayerAudioCategory.Media,
                };
                var flipViewItem = FlipView.ContainerFromItem(item);
                if (flipViewItem == null) return;
                var mediaPlayerElement = ViewHelper.FindVisualChild<MediaPlayerElement>(flipViewItem);
                if (mediaPlayerElement != null)
                {
                    mediaPlayerElement.SetMediaPlayer(_mediaPlayer);
                    mediaPlayerElement.Stretch = Stretch.Uniform;
                }
            }
            else
            {
                StopOldMediaPlayer();
            }
        }

        private void StopOldMediaPlayer()
        {
            try
            {
                if (_mediaPlayer != null)
                {
                    _mediaPlayer.Pause();
                    _mediaPlayer.Source = null;
                }
            }
            catch (Exception e)
            {
                HockeyClient.Current.TrackException(e);
            }
            _mediaPlayer = null;
        }


        private void FeedViewer_OnUnloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_mediaPlayer != null)
                    _mediaPlayer.Dispose();
            }
            catch (Exception ex)
            {
                HockeyClient.Current.TrackException(ex);
            }
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Handle(MuteEvent message)
        {
            IsMuted = message.IsMuted;
            try
            {
                if (_mediaPlayer != null)
                    _mediaPlayer.IsMuted = IsMuted;
            }
            catch (Exception e)
            {
                HockeyClient.Current.TrackException(e);
            }
        }



        private async void DownloadImageClick(object sender, RoutedEventArgs e)
        {
             var savePicker = new FileSavePicker {SuggestedStartLocation = PickerLocationId.PicturesLibrary};
            if (!SelectedFeedItem.IsVideo)
            {
                savePicker.FileTypeChoices.Add("Image", new List<string>() {".jpg"});
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
            await  SaveFileAsync(SelectedFeedItem.FullSizeSource, savefile);
        }


        private async Task SaveFileAsync(Uri fileUri, StorageFile file)
        {
            try
            {
                var backgroundDownloader = new BackgroundDownloader();
                var downloadOperation = backgroundDownloader.CreateDownload(fileUri, file);
                await downloadOperation.StartAsync();
                _toastNotificationService.ShowToastNotificationDownloadSucceded();

            }
            catch (Exception exc)
            {
                _toastNotificationService.ShowToastNotificationDownloadFailed();
                HockeyClient.Current.TrackException(exc);
            }
        }

        private void ScrollViewerSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize != e.NewSize)
            {
                var image = ViewHelper.FindVisualChild<ImageEx>(sender as DependencyObject);
                image.MaxWidth = e.NewSize.Width;
                var mediaPlayerElement = ViewHelper.FindVisualChild<MediaPlayerElement>(sender as DependencyObject);
                mediaPlayerElement.MaxWidth = e.NewSize.Width;
                mediaPlayerElement.MaxHeight = FlipView.ActualHeight * 0.75;
            }
        }

        private void ShareButtonClick(object sender, RoutedEventArgs e)
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            if (DataTransferManager.IsSupported())
            {
                dataTransferManager.DataRequested += DataTransferManager_DataRequested;
                DataTransferManager.ShowShareUI();
            }
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.RequestedOperation = DataPackageOperation.Link;
            ;
            request.Data.Properties.Title = "SharePostTitel".GetLocalized();
            request.Data.Properties.Description = "SharePostDescription".GetLocalized();
            request.Data.SetWebLink(SelectedFeedItem.ShareLink);
        }

        private void GridSplitter_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            float rightColumnWidth = (float) Math.Round((float) (FeedItemColumn.ActualWidth * 1 / MainGrid.ActualWidth),
                2);
            float leftColumnWidht = 1 - rightColumnWidth;
            _settingsService.SaveFeedViewerColumnWidthFromSettingsAsync(leftColumnWidht, rightColumnWidth);
          
        }

        private  void ExtraCommentColumnGridSplitterManipulationCompleted(object sender,
            ManipulationCompletedRoutedEventArgs e)
        {
            var scrollViewer = FlipView.ContainerFromItem(SelectedFeedItem).FindDescendantByName("MainScrollViewer");
            if (scrollViewer != null)
            {
                float leftColumnWidth =
                    (float) Math.Round((float) (scrollViewer.ActualWidth * 1 / FlipView.ActualWidth), 2);
                float rightColumnWidth = 1 - leftColumnWidth;
               _settingsService.SaveFeedViewerExtraColumnWidthFromSettingsAsync(leftColumnWidth, rightColumnWidth);
                FlipViewMainColumnWidth = new GridLength(leftColumnWidth,GridUnitType.Star);
                FlipViewExtraColumnWidth = new GridLength(rightColumnWidth, GridUnitType.Star);
            }
        }

        //private async void UIElement_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        //{
        //    //var flipViewItem = FlipView.ContainerFromItem(SelectedFeedItem);

        //    //if (flipViewItem == null) return;
        //    //var fontIcon = flipViewItem.FindDescendantByName("HeartIcon");
        //    //fontIcon.Fade(value: 1, duration: 1500).StartAsync();
        //    //await  fontIcon.Scale(centerX: 10,
        //    //    centerY: 10,
        //    //    scaleX: 5f,
        //    //    scaleY: 5f,
        //    //    duration: 1500).StartAsync();
        //    //fontIcon.Fade(value: 0, duration: 1500).StartAsync();
        //    //await fontIcon.Scale(centerX: 10f,
        //    //    centerY: 10f,
        //    //    scaleX: 1.0f,
        //    //    scaleY: 1f,
        //    //    duration: 1500).StartAsync();

        //}
    }
}
