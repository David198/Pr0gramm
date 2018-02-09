using Windows.UI.Xaml.Controls;
using Pr0gramm.ViewModels;

namespace Pr0gramm.Views
{
    public sealed partial class CameraPage : Page
    {
        public CameraPage()
        {
            InitializeComponent();
        }

        private CameraViewModel ViewModel => DataContext as CameraViewModel;
    }
}
