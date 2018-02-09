﻿using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Pr0gramm.Helpers;

namespace Pr0gramm.Services
{
    public static class ThemeSelectorService
    {
        private const string SettingsKey = "RequestedTheme";

        public static ElementTheme Theme { get; set; } = ElementTheme.Default;

        public static async Task InitializeAsync()
        {
            Theme = await LoadThemeFromSettingsAsync();
        }

        public static async Task SetThemeAsync(ElementTheme theme)
        {
            Theme = theme;

            SetRequestedTheme();
            await SaveThemeInSettingsAsync(Theme);
        }

        public static void SetRequestedTheme()
        {
            if (Window.Current.Content is FrameworkElement frameworkElement)
                frameworkElement.RequestedTheme = Theme;
        }

        private static async Task<ElementTheme> LoadThemeFromSettingsAsync()
        {
            var cacheTheme = ElementTheme.Default;
            var themeName = await ApplicationData.Current.LocalSettings.ReadAsync<string>(SettingsKey);

            if (!string.IsNullOrEmpty(themeName))
                Enum.TryParse(themeName, out cacheTheme);

            return cacheTheme;
        }

        private static async Task SaveThemeInSettingsAsync(ElementTheme theme)
        {
            await ApplicationData.Current.LocalSettings.SaveAsync(SettingsKey, theme.ToString());
        }
    }
}
