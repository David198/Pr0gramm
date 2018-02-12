using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Caliburn.Micro;
using Microsoft.HockeyApp;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Pr0gramm.EventHandlers;
using Pr0gramm.Helpers;
using Pr0gramm.Models;
using Pr0gramm.Services;
using Pr0gramm.Views;
using Pr0gramm.Views.Convertes;
using Pr0grammAPI.Annotations;

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
        }


        private bool IsMuted { get; set; }
        private  MediaPlayer _mediaPlayer;


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
            var scrollViewer = FeedItemGridView.ChildrenBreadthFirst().OfType<ScrollViewer>().First();
            scrollViewer.ViewChanged += ScrollViewer_OnViewChanged;
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
                    IsMuted = SettingsService.IsMuted,
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

        public void SearchTag(string tag)
        {

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
               // HockeyClient.Current.TrackException(e);
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
               // HockeyClient.Current.TrackException(ex);
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
             //  HockeyClient.Current.TrackException(e);
            }
        }

        private async void DownloadImageClick(object sender, RoutedEventArgs e)
        {
            var flipViewItem = FlipView.ContainerFromItem(SelectedFeedItem);
            if (flipViewItem == null) return;
            var myImageSource = ViewHelper.FindVisualChild<ImageEx>(flipViewItem);
            var bitmap = new RenderTargetBitmap();
            await bitmap.RenderAsync(myImageSource);

            var savePicker = new FileSavePicker {SuggestedStartLocation = PickerLocationId.PicturesLibrary};
            savePicker.FileTypeChoices.Add("Image", new List<string>() { ".jpg" });
            savePicker.SuggestedFileName = "NewImage";
            var savefile = await savePicker.PickSaveFileAsync();
            if (savefile == null)
                return;

            var pixels = await bitmap.GetPixelsAsync();
            using (IRandomAccessStream stream = await savefile.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await
                    BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);
                byte[] bytes = pixels.ToArray();
                encoder.SetPixelData(BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Ignore,
                    (uint)bitmap.PixelWidth,
                    (uint)bitmap.PixelHeight,
                    200,
                    200,
                    bytes);
                await encoder.FlushAsync();
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
                mediaPlayerElement.MaxHeight = FlipView.ActualHeight * 0.5;
            }
        }
    }
}
