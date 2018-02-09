using Windows.UI.Xaml.Controls;
using Pr0gramm.ViewModels;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Pr0gramm.Views
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NewPage : Page
    {
        public NewPage()
        {
            InitializeComponent();
        }

        private NewViewModel ViewModel => DataContext as NewViewModel;
    }
}
