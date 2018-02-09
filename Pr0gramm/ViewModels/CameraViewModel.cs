using System;
using Windows.UI.Xaml.Media.Imaging;
using Caliburn.Micro;
using Pr0gramm.EventHandlers;

namespace Pr0gramm.ViewModels
{
    public class CameraViewModel : Screen
    {
        private BitmapImage _photo;

        public BitmapImage Photo
        {
            get => _photo;
            set => Set(ref _photo, value);
        }

        public void OnPhotoTaken(CameraControlEventArgs args)
        {
            if (!string.IsNullOrEmpty(args.Photo))
                Photo = new BitmapImage(new Uri(args.Photo));
        }
    }
}
