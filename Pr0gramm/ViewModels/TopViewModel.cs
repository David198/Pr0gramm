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
        private readonly UserLoginService _userLoginService;

        public  TopViewModel(IProgrammApi programmProgrammApi, IEventAggregator eventAggregator,
            ToastNotificationsService toastNotificationsService, UserLoginService userLoginService)
            : base(programmProgrammApi, eventAggregator, toastNotificationsService)
        {
            _userLoginService = userLoginService;
        }

        private async void InitializeTheme()
        {
            await ThemeSelectorService.InitializeAsync();
            ThemeSelectorService.SetRequestedTheme();
        }

        protected override async void OnInitialize()
        {
            base.OnInitialize();
            await TryLoginAutomatically();
            InitializeTheme();
        }

        private async Task TryLoginAutomatically()
        {
            var credentials = _userLoginService.GetCredentialFromLocker();
            if (credentials != null && !_userLoginService.IsLoggedIn)
            {
                credentials.RetrievePassword();
                try
                {
                    var user = await ProgrammApi.Login(credentials.UserName, credentials.Password);
                    if (user != null)
                    {
                        EventAggregator.PublishOnUIThread(new UserLoggedInEvent(credentials.UserName));
                        _userLoginService.IsLoggedIn = true;
                    }
                }
                catch (Exception)
                {
                   ToastNotificationsService.ShowToastNotificationWebSocketExeception();
                }

            }
        }
    }
}
