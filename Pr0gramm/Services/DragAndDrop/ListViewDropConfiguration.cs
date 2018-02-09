using System.Windows.Input;
using Windows.UI.Xaml;

namespace Pr0gramm.Services.DragAndDrop
{
    public class ListViewDropConfiguration : DropConfiguration
    {
        public static readonly DependencyProperty DragItemsStartingCommandProperty =
            DependencyProperty.Register("DragItemsStartingCommand", typeof(ICommand), typeof(DropConfiguration),
                new PropertyMetadata(null));

        public static readonly DependencyProperty DragItemsCompletedCommandProperty =
            DependencyProperty.Register("DragItemsCompletedCommand", typeof(ICommand), typeof(DropConfiguration),
                new PropertyMetadata(null));

        public ICommand DragItemsStartingCommand
        {
            get => (ICommand) GetValue(DragItemsStartingCommandProperty);
            set => SetValue(DragItemsStartingCommandProperty, value);
        }

        public ICommand DragItemsCompletedCommand
        {
            get => (ICommand) GetValue(DragItemsCompletedCommandProperty);
            set => SetValue(DragItemsCompletedCommandProperty, value);
        }
    }
}
