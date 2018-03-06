using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Caliburn.Micro;
using Microsoft.Identity.Client;
using Pr0gramm.EventHandlers;
using Pr0gramm.Views;
using Pr0grammAPI.Exceptions;
using Pr0grammAPI.Interfaces;
using Pr0grammAPI.User;

namespace Pr0gramm.Services
{
    public class UserLoginService
    {
        private readonly UserSyncService _userSyncService;
        private readonly IEventAggregator _iEventAggregator;
        private readonly IProgrammApi _iprogrammApi;
        private readonly ToastNotificationsService _toastNotifications;
        public ProfileInfo UserProfileInfo { get; set; }
        public UserLoginService(IEventAggregator iEventAggregator, IProgrammApi iprogrammApi,ToastNotificationsService toastNotifications, UserSyncService userSyncService)
        {
            _userSyncService = userSyncService;
            _iEventAggregator = iEventAggregator;
            _iprogrammApi = iprogrammApi;
            _toastNotifications = toastNotifications;
        }
        private const string resourceName = "Pr0gramm";
        public  bool IsLoggedIn;
        public void SaveUserLoginToVault(string username, string password)
        {
            var vault = new PasswordVault();
            vault.Add(new PasswordCredential(
                resourceName, username, password));
        }

        public string ActualUser { get; set; }

        public PasswordCredential GetCredentialFromLocker()
        {
            PasswordCredential credential = null;

            var vault = new PasswordVault();
            try
            {
                var credentialList = vault.FindAllByResource(resourceName);
                if (credentialList.Count > 0)
                {
                    if (credentialList.Count == 1)
                    {
                        credential = credentialList[0];
                    }
                }
              
                return credential;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public bool DeleteUser(string username)
        {
            PasswordCredential credential = null;

            var vault = new PasswordVault();
            try
            {
                var credentialList = vault.FindAllByResource(resourceName);
                if (credentialList.Count > 0)
                {
                    if (credentialList.Count == 1)
                    {
                        credential = credentialList[0];
                    }
                    credential.RetrievePassword();
                    vault.Remove(new PasswordCredential(
                        resourceName, username, credential.Password));
                    _userSyncService.ResetOffset();
                    _userSyncService.StopSyncRoutine();
                    ActualUser = "";
                    IsLoggedIn = false;       
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }  
        }

        public async void ShowUserLogin()
        {
            LoginDialog dlg = new LoginDialog(_iEventAggregator, this, _toastNotifications);
            await dlg.ShowAsync();
        }

        public async Task<bool> LoginUser(string name, string password, bool savePassword, bool publishToUi)
        {
            try
            {
                var cookie = await _iprogrammApi.Login(name, password);
                if (cookie!=null)
                {
                    if(publishToUi)
                     _iEventAggregator.PublishOnUIThread(new UserLoggedInEvent(name));
                    IsLoggedIn = true;
                    if(savePassword)
                        SaveUserLoginToVault(name,password);
                    ActualUser = name;
                    _userSyncService.StartSyncRoutine();
                    return true;
                }
            }
            catch (ApplicationException)
            {
                _toastNotifications.ShowToastNotificationWebSocketExeception();
            }
            catch (BannedException)
            {
                _toastNotifications.ShowToastNotificationUserBannedExeception();
            }

            return false;
        }

        public async Task TryLoginAutomatically()
        {
            var credentials = GetCredentialFromLocker();
            if (credentials != null && !IsLoggedIn)
            {
                credentials.RetrievePassword();
                try
                {
                    await LoginUser(credentials.UserName, credentials.Password, false, false);
                }
                catch (ApplicationException)
                {
                    _toastNotifications.ShowToastNotificationWebSocketExeception();
                }
            }
        }

        public async Task RetrieveUserInfo(string name)
        {
            try
            {
                UserProfileInfo = await _iprogrammApi.GetUserProfileInfo(name, FlagSelectorService.ActualFlag);
            }
            catch (ApplicationException)
            {
                _toastNotifications.ShowToastNotificationWebSocketExeception();
            }
            catch (BannedException)
            {
                _toastNotifications.ShowToastNotificationUserBannedExeception();
                DeleteUser(name);
            }

        }
    }
}
