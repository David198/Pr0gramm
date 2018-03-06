using Caliburn.Micro;
using Pr0gramm.Services;
using Pr0grammAPI.Interfaces;

namespace Pr0gramm.ViewModels
{
    public class NewViewModel : FeedViewerViewModelBase
    {
        public NewViewModel(IProgrammApi programmProgrammApi, IEventAggregator eventAggregator,
            ToastNotificationsService toastNotificationsService, SettingsService settingsService, CacheService cacheService, FeedService feedService) :
            base(programmProgrammApi, eventAggregator, toastNotificationsService, settingsService, cacheService, feedService)
        {
            Promoted = 0;
        }
    }
}
