using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Pr0gramm.Helpers;
using Pr0gramm.Views;

namespace Pr0gramm.Services
{
    public static class FirstRunDisplayService
    {
        internal static async Task ShowIfAppropriateAsync()
        {
            var hasShownFirstRun = false;
            hasShownFirstRun = await ApplicationData.Current.LocalSettings.ReadAsync<bool>(nameof(hasShownFirstRun));

            if (!hasShownFirstRun)
            {
                await ApplicationData.Current.LocalSettings.SaveAsync(nameof(hasShownFirstRun), true);
                await ThemeSelectorService.SetThemeAsync(ElementTheme.Dark);
                var dialog = new FirstRunDialog();
                await dialog.ShowAsync();
            }
        }
    }
}
