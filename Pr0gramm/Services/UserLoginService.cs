using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Caliburn.Micro;
using Microsoft.Identity.Client;
using Pr0gramm.Views;
using Pr0grammAPI.Interfaces;

namespace Pr0gramm.Services
{
    public class UserLoginService
    {
        private readonly IEventAggregator _iEventAggregator;
        private readonly IProgrammApi _iprogrammApi;
        private readonly ToastNotificationsService _toastNotifications;

        public UserLoginService(IEventAggregator iEventAggregator, IProgrammApi iprogrammApi,ToastNotificationsService toastNotifications)
        {
            _iEventAggregator = iEventAggregator;
            _iprogrammApi = iprogrammApi;
            _toastNotifications = toastNotifications;
        }
        private const string resourceName = "Pr0gramm";
        public  bool IsLoggedIn;
        public  void  SaveUserLogin(string username, string password)
        {
            var vault = new Windows.Security.Credentials.PasswordVault();
            vault.Add(new Windows.Security.Credentials.PasswordCredential(
                resourceName, username, password));
        }

        public  PasswordCredential GetCredentialFromLocker()
        {
            Windows.Security.Credentials.PasswordCredential credential = null;

            var vault = new Windows.Security.Credentials.PasswordVault();
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
            catch (Exception e)
            {
                return null;
            }
           
         
        }

        public  bool DeleteUser(string username)
        {
            Windows.Security.Credentials.PasswordCredential credential = null;

            var vault = new Windows.Security.Credentials.PasswordVault();
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

                credential.RetrievePassword();
                vault.Remove(new Windows.Security.Credentials.PasswordCredential(
                    resourceName, username, credential.Password));
                IsLoggedIn = false;
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
    }
}
