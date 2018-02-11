using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Caliburn.Micro;
using Pr0gramm.ViewModels;

namespace Pr0gramm.Views
{
    public sealed partial class ShellPage : IShellView
    {
        public ShellPage()
        {
            InitializeComponent();
        }

        private ShellViewModel ViewModel => DataContext as ShellViewModel;

        public INavigationService CreateNavigationService(WinRTContainer container)
        {
            return container.RegisterNavigationService(shellFrame);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void FlagsUpdated(object sender, object e)
        {
            // cm:Message.Attach= not working for flyout.
            ViewModel.FlagsUpdated();
        }
    }
}
