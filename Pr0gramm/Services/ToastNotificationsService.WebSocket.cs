using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;
using Pr0gramm.Helpers;

namespace Pr0gramm.Services
{
    public partial class ToastNotificationsService
    {
        public void ShowToastNotificationWebSocketExeception()
        {
            // Create the toast content
            var content = new ToastContent
            {
                // TODO WTS: Check this documentation to know more about the Launch property
                // Documentation: https://developer.microsoft.com/en-us/windows/uwp-community-toolkit/api/microsoft_toolkit_uwp_notifications_toastcontent
                Launch = "ToastContentActivationParams",

                Visual = new ToastVisual
                {
                    BindingGeneric = new ToastBindingGeneric
                    {
                        Children =
                        {
                            new AdaptiveText
                            {
                                Text = "WebSocket_Exception_Title".GetLocalized()
                            },
                            new AdaptiveText
                            {
                                Text = "WebSocket_Exception_Text".GetLocalized()
                            }
                        }
                    }
                },

                Actions = new ToastActionsCustom
                {
                    Buttons =
                    {
                        // TODO WTS: Check this documentation to know more about Toast Buttons
                        // Documentation: https://developer.microsoft.com/en-us/windows/uwp-community-toolkit/api/microsoft_toolkit_uwp_notifications_toastbutton
                        new ToastButton("OK", "ToastButtonActivationArguments")
                        {
                            ActivationType = ToastActivationType.Foreground
                        }
                    }
                }
            };

            // Add the content to the toast
            var toast = new ToastNotification(content.GetXml())
            {
                // TODO WTS: Get or set the unique identifier of this notification within the notification Group. Max length 16 characters.
                // Documentation: https://docs.microsoft.com/uwp/api/windows.ui.notifications.toastnotification
                Tag = "WebSocketNotification"
            };

            // And show the toast
            ShowToastNotification(toast);
        }
    }
}
