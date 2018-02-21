
using Windows.UI.Xaml.Controls;
using Pr0gramm.ViewModels;

namespace Pr0gramm.Views
{
    public sealed partial class SettingsPage : Page
    {
        //// TODO WTS: Change the URL for your privacy policy in the Resource File, currently set to https://YourPrivacyUrlGoesHere

        public SettingsPage()
        {
            InitializeComponent();
        }

        private SettingsViewModel ViewModel => DataContext as SettingsViewModel;
    }
}
