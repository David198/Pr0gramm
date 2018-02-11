using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Microsoft.Identity.Client;

namespace Pr0gramm.Services
{
    public class UserLoginService
    {

        private static string resourceName = "Pr0gramm";
        public static bool IsLoggedIn;
        public static void  SaveUserLogin(string username, string password)
        {
            var vault = new Windows.Security.Credentials.PasswordVault();
            vault.Add(new Windows.Security.Credentials.PasswordCredential(
                resourceName, username, password));
        }

        public static PasswordCredential GetCredentialFromLocker()
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

        public static bool DeleteUser(string username)
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
    }
}
