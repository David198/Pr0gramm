using System;
using System.Threading.Tasks;
using Caliburn.Micro;
using Pr0gramm.EventHandlers;
using Pr0gramm.Services;
using Pr0grammAPI.Interfaces;

namespace Pr0gramm.ViewModels
{
    public class TopViewModel : FeedViewerViewModelBase
    {
        public  TopViewModel(IProgrammApi programmProgrammApi, IEventAggregator eventAggregator,
            ToastNotificationsService toastNotificationsService, SettingsService settingsService)
            : base(programmProgrammApi, eventAggregator, toastNotificationsService, settingsService)
        {
        }

        private async void InitializeTheme()
        {
            await ThemeSelectorService.InitializeAsync();
            ThemeSelectorService.SetRequestedTheme();
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            InitializeTheme();
        }

      
    }
}
