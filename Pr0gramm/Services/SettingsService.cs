using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Pr0gramm.Helpers;

namespace Pr0gramm.Services
{
    public class SettingsService
    {
        private const string FeedViewerLeftGridColumnWidthKey = "FeedViewerLeftGridColumnWidth";
        private const string FeedViewerRightGridColumnWidthKey = "FeedViewerRightGridColumnWidth";
        public bool IsMuted { get; set; }

        public float FeedViewerLeftGridColumnWidth { get; set; } = 0.6f;
        public float FeedViewerRightGridColumnWidth { get; set; } = 0.4f;

        public async Task InitializeSettings()
        {
            await LoadFeedViewerColumnWidthFromSettingsAsync();
        }

        private async Task InitializeFeedViewerColumnWidthsAsyncTask()
        {
            await LoadFeedViewerColumnWidthFromSettingsAsync();
        }

        private async Task LoadFeedViewerColumnWidthFromSettingsAsync()
        {
         
            string tempFeedViewerLeftGridColumnWidthString = await ApplicationData.Current.LocalSettings.ReadAsync<string>(FeedViewerLeftGridColumnWidthKey);
            if (!string.IsNullOrEmpty(tempFeedViewerLeftGridColumnWidthString))
            {
                var feedViewerLeftGridColumnWidth = 0f;
                if (float.TryParse(tempFeedViewerLeftGridColumnWidthString, out feedViewerLeftGridColumnWidth))
                    FeedViewerLeftGridColumnWidth = feedViewerLeftGridColumnWidth;
            }

            string tempFeedViewerRightGridColumnWidthString = await ApplicationData.Current.LocalSettings.ReadAsync<string>(FeedViewerRightGridColumnWidthKey);
            if (!string.IsNullOrEmpty(tempFeedViewerRightGridColumnWidthString))
            {
                var feedViewerRightGridColumnWidth = 0f;
                if (float.TryParse(tempFeedViewerRightGridColumnWidthString, out feedViewerRightGridColumnWidth))
                    FeedViewerRightGridColumnWidth = feedViewerRightGridColumnWidth;
            }
        }

        public async void SaveFeedViewerColumnWidthFromSettingsAsync(float feedViewerLeftGridColumnWidth, float feedViewerRightGridColumnWidth)
        {
            FeedViewerLeftGridColumnWidth = feedViewerLeftGridColumnWidth;
            FeedViewerRightGridColumnWidth = feedViewerRightGridColumnWidth;
            await ApplicationData.Current.LocalSettings.SaveAsync(FeedViewerLeftGridColumnWidthKey, feedViewerLeftGridColumnWidth.ToString());
            await ApplicationData.Current.LocalSettings.SaveAsync(FeedViewerRightGridColumnWidthKey, feedViewerRightGridColumnWidth.ToString());
        }
    }
}
