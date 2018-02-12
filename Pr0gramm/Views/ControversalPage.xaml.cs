using System;

using Pr0gramm.ViewModels;

using Windows.UI.Xaml.Controls;

namespace Pr0gramm.Views
{
    public sealed partial class ControversalPage : Page
    {
        public ControversalPage()
        {
            InitializeComponent();
        }

        private ControversalViewModel ViewModel
        {
            get { return DataContext as ControversalViewModel; }
        }
    }
}
