using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Store;
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
using Pr0gramm.Models.Enums;
using Pr0gramm.Services;
using Pr0gramm.Views;
using Pr0gramm.Views.Convertes;
using Pr0grammAPI.Annotations;
using RestSharp;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Pr0gramm.Controls
{
    public sealed partial class FeedViewer : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty FeedItemsProperty = DependencyProperty.Register(
            "FeedItems", typeof(BindableCollection<FeedItemViewModel>), typeof(FeedViewer),
            new PropertyMetadata(default(BindableCollection<FeedItemViewModel>)));


        public static readonly DependencyProperty SelectedFeedItemProperty = DependencyProperty.Register(
            "SelectedFeedItem", typeof(FeedItemViewModel), typeof(FeedViewer),
            new PropertyMetadata(default(FeedItemViewModel), SelectedItemChanged));

        private static void SelectedItemChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            ((FeedViewer) dependencyObject).OnSelectionChanged(
                (FeedItemViewModel) (dependencyPropertyChangedEventArgs.NewValue));
        }

        public static readonly DependencyProperty IsMutedProperty = DependencyProperty.Register(
            "IsMuted", typeof(bool), typeof(FeedViewer), new PropertyMetadata(default(bool), MuteChanged));

        private static void MuteChanged(DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            if (((FeedViewer) dependencyObject)._mediaPlayer != null
            ) //&& ((FeedViewer)dependencyObject)._mediaPlayer.CurrentState != MediaPlayerState.Closed)
                ((FeedViewer) dependencyObject)._mediaPlayer.IsMuted =
                    (bool) dependencyPropertyChangedEventArgs.NewValue;
        }

        public FeedViewer()
        {
            InitializeComponent();
            _settingsService = IoC.Get<SettingsService>();
            DataContext = this;
        }

        public event EventHandler<FeedItemViewModel> SelectionChanged;

        private MediaPlayer _mediaPlayer;
        private readonly SettingsService _settingsService;
        private GridLength _flipViewMainColumnWidth;
        private GridLength _flipViewExtraColumnWidth;

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

        public bool IsMuted
        {
            get { return (bool) GetValue(IsMutedProperty); }
            set { SetValue(IsMutedProperty, value); }
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
                if (value != SelectedFeedItem)
                {
                    SetValue(SelectedFeedItemProperty, value);
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
            _mediaPlayer = new MediaPlayer
            {
                IsLoopingEnabled = true,
                IsMuted = _settingsService.IsMuted,
                AutoPlay = true,
                AudioCategory = MediaPlayerAudioCategory.Media,
            };
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
                Math.Abs(scrollViewer.VerticalOffset - scrollViewer.ScrollableHeight) < 0.05f;
            if (scrollViewerReachedBottom)
                OnLoadNewItems(new EventArgs());
        }

        private void OnLoadNewItems(EventArgs e)
        {
            LoadNewItems?.Invoke(this, e);
        }

        private void SetVideoPlayBack(FeedItemViewModel item)
        {
            try
            {
                if (_mediaPlayer.PlaybackSession.CanPause)
                {
                    _mediaPlayer.Source = null;
                }
                if (item.IsVideo)
                {
                    
                    var flipViewItem = FlipView.ContainerFromItem(item);
                    if (flipViewItem == null)
                    {
                        FlipView.UpdateLayout();
                        flipViewItem = FlipView.ContainerFromItem(item);
                        if (flipViewItem == null) return;
                    }
                    _mediaPlayer.Source = MediaSource.CreateFromUri(item.ImageSource);
                    _mediaPlayer.IsMuted = IsMuted;
                    var mediaPlayerElement = flipViewItem.FindDescendant<MediaPlayerElement>();// ViewHelper.FindVisualChild<MediaPlayerElement>(flipViewItem);
                    mediaPlayerElement?.SetMediaPlayer(_mediaPlayer);
                }
            }
            catch (Exception e)
            {
            }
        }

        private void FeedViewer_OnUnloaded(object sender, RoutedEventArgs e)
        {
            _mediaPlayer?.Dispose();
        }

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
    
        private void FeedItemGridView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedFeedItem != null)
            {
                SetVideoPlayBack(SelectedFeedItem);
                FeedItemGridView.ScrollIntoView(SelectedFeedItem);
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

        private void OnSelectionChanged(FeedItemViewModel e)
        {
            SelectionChanged?.Invoke(this, e);
        }
    }
}
