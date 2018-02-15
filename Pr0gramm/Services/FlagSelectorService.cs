using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Pr0gramm.Helpers;
using Pr0grammAPI.Feeds;

namespace Pr0gramm.Services
{
    public static class FlagSelectorService
    {
        public static FeedFlags ActualFlag { get; set; } = FeedFlags.SFW;

        public static async void SetActualFeedFlagAsync(bool sfw, bool nsfw, bool nsfl, bool loggedIn)
        {
        
            if (sfw && !nsfw && !nsfl && !loggedIn)
            {
                ActualFlag = FeedFlags.SFW;
            }


            if (sfw && !nsfw && !nsfl && loggedIn)
            {
                ActualFlag = FeedFlags.SFWLogin;
            }

            if (sfw && nsfw && !nsfl)
            {
                ActualFlag = FeedFlags.SFWNSFW;
            }

            if (sfw && !nsfw && nsfl)

            {
                ActualFlag = FeedFlags.SFWNSFL;
            }
            if (!sfw && nsfw && !nsfl)
            {
                ActualFlag = FeedFlags.NSFW;
            }
            if (!sfw && nsfw && nsfl)
            {
                ActualFlag = FeedFlags.NSFWNSFL;
            }
            if (!sfw && !nsfw && nsfl)
            {
                ActualFlag = FeedFlags.NSFL;
            }
            if (sfw && nsfw && nsfl)
                ActualFlag = FeedFlags.ALL;
            await SaveFlagsInSettingsAsync(ActualFlag);
        }

        private static async Task<FeedFlags> LoadFlagFromSettingsAsync()
        {
            var chacheFlag = FeedFlags.SFW;
            var flagName = await ApplicationData.Current.LocalSettings.ReadAsync<string>(SettingsKey);

            if (!string.IsNullOrEmpty(flagName))
                Enum.TryParse(flagName, out chacheFlag);

            return chacheFlag;
        }

        private const string SettingsKey = "RequestedFlags";

        private static async Task SaveFlagsInSettingsAsync(FeedFlags flags)
        {
            await ApplicationData.Current.LocalSettings.SaveAsync(SettingsKey, flags.ToString());
        }

        public static async Task InitializeAsync()
        {
            ActualFlag = await LoadFlagFromSettingsAsync();
        }

    }
}
