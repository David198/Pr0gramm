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
    }
}
