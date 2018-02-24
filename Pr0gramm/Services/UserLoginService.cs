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
        private readonly IEventAggregator _iEventAggregator;
        private readonly IProgrammApi _iprogrammApi;
        private readonly ToastNotificationsService _toastNotifications;
        public ProfileInfo UserProfileInfo { get; set; }
        public UserLoginService(IEventAggregator iEventAggregator, IProgrammApi iprogrammApi,ToastNotificationsService toastNotifications)
        {
            _iEventAggregator = iEventAggregator;
            _iprogrammApi = iprogrammApi;
            _toastNotifications = toastNotifications;
            TryLoginAutomatically();
        }
        private const string resourceName = "Pr0gramm";
        public  bool IsLoggedIn;
        public void SaveUserLogin(string username, string password)
        {
            var vault = new PasswordVault();
            vault.Add(new PasswordCredential(
                resourceName, username, password));
        }

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
                    IsLoggedIn = false;
             
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }  
        }

        public  async void ShowUserLogin()
        {
            LoginDialog dlg = new LoginDialog(_iEventAggregator, _iprogrammApi, this, _toastNotifications);
            await dlg.ShowAsync();
        }

        private async void TryLoginAutomatically()
        {
            var credentials = GetCredentialFromLocker();
            if (credentials != null && !IsLoggedIn)
            {
                credentials.RetrievePassword();
                try
                {
                    if (await _iprogrammApi.Login(credentials.UserName, credentials.Password))
                    {
                       _iEventAggregator.PublishOnUIThread(new UserLoggedInEvent(credentials.UserName));
                        IsLoggedIn = true; 
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
