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
        private const string FeedViewerLeftGridColumnWidthKey = "FeedViewerLeftGridColumnWidth";
        private const string FeedViewerRightGridColumnWidthKey = "FeedViewerRightGridColumnWidth";
        private const string FeedViewerExtraLeftGridColumnWidthKey = "FeedViewerExtraLeftGridColumnWidth";
        private const string FeedViewerExtraRightGridColumnWidthKey = "FeedViewerExtraRightGridColumnWidth";
        private const string FeedViewerExtraColumVisibleKey = "FeedViewerExtraColumn";

        public float FeedViewerLeftGridColumnWidth { get; set; } = 0.6f;
        public float FeedViewerRightGridColumnWidth { get; set; } = 0.4f;
        public float FeedViewerExtraLeftGridColumnWidth { get; set; } = 0.5f;
        public float FeedViewerExtraRightGridColumnWidth { get; set; } = 0.5f;
        public bool FeedViewerExtraColumnVisible { get; set; }

        private async Task LoadFeedViewerSettingsFromSettingsAsync()
        {

            string tempFeedViewerLeftGridColumnWidthString = await ApplicationData.Current.LocalSettings.ReadAsync<string>(FeedViewerLeftGridColumnWidthKey);
            if (!string.IsNullOrEmpty(tempFeedViewerLeftGridColumnWidthString))
            {
                if (float.TryParse(tempFeedViewerLeftGridColumnWidthString, out float feedViewerLeftGridColumnWidth))
                    FeedViewerLeftGridColumnWidth = feedViewerLeftGridColumnWidth;
            }

            string tempFeedViewerRightGridColumnWidthString = await ApplicationData.Current.LocalSettings.ReadAsync<string>(FeedViewerRightGridColumnWidthKey);
            if (!string.IsNullOrEmpty(tempFeedViewerRightGridColumnWidthString))
            {
                if (float.TryParse(tempFeedViewerRightGridColumnWidthString, out float feedViewerRightGridColumnWidth))
                    FeedViewerRightGridColumnWidth = feedViewerRightGridColumnWidth;
            }

            string tempFeedViewerExtraRightGridColumnWidthString = await ApplicationData.Current.LocalSettings.ReadAsync<string>(FeedViewerExtraRightGridColumnWidthKey);
            if (!string.IsNullOrEmpty(tempFeedViewerExtraRightGridColumnWidthString))
            {
                if (float.TryParse(tempFeedViewerExtraRightGridColumnWidthString, out float feedViewerExtraRightGridColumnWidth))
                    FeedViewerExtraRightGridColumnWidth = feedViewerExtraRightGridColumnWidth;
            }

            string tempFeedViewerExtraLeftGridColumnWidthString = await ApplicationData.Current.LocalSettings.ReadAsync<string>(FeedViewerExtraLeftGridColumnWidthKey);
            if (!string.IsNullOrEmpty(tempFeedViewerExtraLeftGridColumnWidthString))
            {
                if (float.TryParse(tempFeedViewerExtraLeftGridColumnWidthString, out float feedViewerExtraLeftGridColumnWidth))
                    FeedViewerExtraLeftGridColumnWidth = feedViewerExtraLeftGridColumnWidth;
            }

            string tempFeedViewerExtraColumnString = await ApplicationData.Current.LocalSettings.ReadAsync<string>(FeedViewerExtraColumVisibleKey);
            if (!string.IsNullOrEmpty(tempFeedViewerExtraColumnString))
            {
                if (bool.TryParse(tempFeedViewerExtraColumnString, out bool feedViewerExtraColumn))
                    FeedViewerExtraColumnVisible = feedViewerExtraColumn;
            }
        }

        public async void SaveFeedViewerColumnWidthFromSettingsAsync(float feedViewerLeftGridColumnWidth, float feedViewerRightGridColumnWidth)
        {
            FeedViewerLeftGridColumnWidth = feedViewerLeftGridColumnWidth;
            FeedViewerRightGridColumnWidth = feedViewerRightGridColumnWidth;
            await ApplicationData.Current.LocalSettings.SaveAsync(FeedViewerLeftGridColumnWidthKey, feedViewerLeftGridColumnWidth.ToString());
            await ApplicationData.Current.LocalSettings.SaveAsync(FeedViewerRightGridColumnWidthKey, feedViewerRightGridColumnWidth.ToString());
        }

        public async void SaveFeedViewerExtraColumnWidthFromSettingsAsync(float feedViewerExtraLeftGridColumnWidth, float feedViewerExtraRightGridColumnWidth)
        {
            FeedViewerExtraLeftGridColumnWidth = feedViewerExtraLeftGridColumnWidth;
            FeedViewerExtraRightGridColumnWidth = feedViewerExtraRightGridColumnWidth;
            await ApplicationData.Current.LocalSettings.SaveAsync(FeedViewerExtraLeftGridColumnWidthKey, feedViewerExtraLeftGridColumnWidth.ToString());
            await ApplicationData.Current.LocalSettings.SaveAsync(FeedViewerExtraRightGridColumnWidthKey, feedViewerExtraRightGridColumnWidth.ToString());
        }

        public async void SaveFeedViewerExtraColumnActive(bool isVisible)
        {
            FeedViewerExtraColumnVisible = isVisible;
            await ApplicationData.Current.LocalSettings.SaveAsync(FeedViewerExtraColumVisibleKey, isVisible.ToString());
        }
    }
}
