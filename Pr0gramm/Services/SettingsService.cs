using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Pr0gramm.Helpers;

namespace Pr0gramm.Services
{
    public partial class SettingsService
    {
        private const string AlwaysStartWithSfwKey = "AlwaysStartWithSFW";
        public bool IsMuted { get; set; }

        public async Task InitializeSettings()
        {
            await LoadAlwaysStartWithSFWAsync();
            await LoadFeedViewerSettingsFromSettingsAsync();
        }

        public bool AlwaysStartWithSfw { get; set; } = true;

        private async Task LoadAlwaysStartWithSFWAsync()
        {
            string tempAlwaysStartWithSFWKeyString =
                await ApplicationData.Current.LocalSettings.ReadAsync<string>(AlwaysStartWithSfwKey);
            if (!string.IsNullOrEmpty(tempAlwaysStartWithSFWKeyString))
            {
                if (bool.TryParse(tempAlwaysStartWithSFWKeyString, out bool alwaysStartWithSFW))
                    AlwaysStartWithSfw = alwaysStartWithSFW;
            }
        }

        public async void SaveAlwaysStartWithSFW(bool istrue)
        {
            AlwaysStartWithSfw = istrue;
            await ApplicationData.Current.LocalSettings.SaveAsync(AlwaysStartWithSfwKey, istrue.ToString());
        }
    }
}
