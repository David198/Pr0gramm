using Caliburn.Micro;
using Pr0gramm.Services;
using Pr0grammAPI.Interfaces;

namespace Pr0gramm.ViewModels
{
    public class TopViewModel : FeedViewerViewModelBase
    {
        public TopViewModel(IProgrammApi programmApi, IEventAggregator eventAggregator,
            ToastNotificationsService toastNotificationsService)
            : base(programmApi, eventAggregator, toastNotificationsService)
        {
        }
    }
}
