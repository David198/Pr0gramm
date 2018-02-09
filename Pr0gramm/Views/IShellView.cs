using Caliburn.Micro;

namespace Pr0gramm.Views
{
    public interface IShellView
    {
        INavigationService CreateNavigationService(WinRTContainer container);
    }
}
