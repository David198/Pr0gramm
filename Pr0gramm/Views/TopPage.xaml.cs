using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Pr0gramm.ViewModels;

namespace Pr0gramm.Views
{
    public sealed partial class TopPage : Page
    {
        public TopPage()
        {
            InitializeComponent();
        }

        private TopViewModel ViewModel => DataContext as TopViewModel;

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
        }
    }
}
