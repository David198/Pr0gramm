﻿using System;
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
using Pr0grammAPI.Exceptions;
using Pr0grammAPI.Interfaces;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Pr0gramm.Views
{
    public sealed partial class LoginDialog : ContentDialog
    {
        private readonly IEventAggregator _ieventAggregator;
        private readonly UserLoginService _userLoginService;
        private readonly ToastNotificationsService _toastNotificationsService;
        private bool _userIsLogginIn;

        public LoginDialog(IEventAggregator IeventAggregator,
            UserLoginService userLoginService, ToastNotificationsService toastNotificationsService)
        {
            _ieventAggregator = IeventAggregator;
            _userLoginService = userLoginService;
            _toastNotificationsService = toastNotificationsService;
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
                _userIsLogginIn = true;
                try
                {
                    if (await _userLoginService.LoginUser(UserName.Text, PasswordBox.Password,true,true))
                    {
                            ErrorText.Visibility = Visibility.Collapsed;
                            _userIsLogginIn = false;
                            Hide();
                      
                    }
                    else
                    {
                        _userIsLogginIn = false;
                        ErrorText.Visibility = Visibility.Visible;
                        ErrorText.Text = "ErrorTextLogin".GetLocalized();
                    }
                }
                catch (ApplicationException)
                {
                    _toastNotificationsService.ShowToastNotificationWebSocketExeception();
                }
                catch (BannedException)
                {
                    _toastNotificationsService.ShowToastNotificationUserBannedExeception();
                    _userIsLogginIn = false;
                    ErrorText.Visibility = Visibility.Visible;
                    ErrorText.Text = "ErrorUserBanned".GetLocalized();
                }
            }
        }

        private void LoginDialog_OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
        {
            if (args.Result == ContentDialogResult.Primary && ErrorText.Visibility.Equals(Visibility.Visible) ||
                _userIsLogginIn)
                args.Cancel = true;
        }
    }
}
