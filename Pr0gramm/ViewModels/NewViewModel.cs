using Caliburn.Micro;
using Pr0gramm.Services;
using Pr0grammAPI.Interfaces;

namespace Pr0gramm.ViewModels
{
    public class NewViewModel : FeedViewerViewModelBase
    {
        public NewViewModel(IProgrammApi programmProgrammApi, IEventAggregator eventAggregator,
            ToastNotificationsService toastNotificationsService) :
            base(programmProgrammApi, eventAggregator, toastNotificationsService)
        {
            ShowTop = false;
        }
    }
}
