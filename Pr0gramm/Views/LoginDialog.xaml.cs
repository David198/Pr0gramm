using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Caliburn.Micro;
using Pr0gramm.EventHandlers;
using Pr0gramm.Helpers;
using Pr0gramm.Services;
using Pr0grammAPI.Interfaces;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Pr0gramm.Views
{
    public sealed partial class LoginDialog : ContentDialog
    {
        private readonly IEventAggregator _ieventAggregator;
        private readonly IProgrammApi _iprogrammApi;
        private bool userIsLogginIn;

        public LoginDialog(IEventAggregator IeventAggregator, IProgrammApi IprogrammApi)
        {
            _ieventAggregator = IeventAggregator;
            _iprogrammApi = IprogrammApi;
            this.InitializeComponent();
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender,
            ContentDialogButtonClickEventArgs args)
        {
            if (string.IsNullOrEmpty(UserName.Text) || string.IsNullOrEmpty(PasswordBox.Password))
            {
                ErrorText.Visibility = Visibility.Visible;
                ErrorText.Text = "ErrorTextCredentialsMissing".GetLocalized();
            }
            else
            {
                userIsLogginIn = true;
                var user = await _iprogrammApi.Login(UserName.Text, PasswordBox.Password);
                if (user != null)
                {
                    UserLoginService.SaveUserLogin(UserName.Text, PasswordBox.Password);
                    _ieventAggregator.PublishOnUIThread(new UserLoggedInEvent(UserName.Text));
                    ErrorText.Visibility = Visibility.Collapsed;
                    userIsLogginIn = false;
                    this.Hide();
                }
                else
                {
                    userIsLogginIn = false;
                    ErrorText.Visibility = Visibility.Visible;
                    ErrorText.Text = "ErrorTextLogin".GetLocalized();
                }
            }
        }

        private void LoginDialog_OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result == ContentDialogResult.Primary && ErrorText.Visibility.Equals(Visibility.Visible) || userIsLogginIn)
                args.Cancel = true;
        }
    }
}
