using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Data.Html;
using Windows.Storage;
using Pr0gramm.Helpers;
using Pr0gramm.Views;
using RestSharp;

namespace Pr0gramm.Services
{
    public static class NewVersionDisplayService
    {
        internal static async Task ShowIfAppropriateAsync()
        {
            RestClient restClient = new RestClient("https://rink.hockeyapp.net/apps/d2ef27ea7e7342b3befa7922cacd4c47");
            var response = await restClient.ExecuteGetTaskAsync(new RestRequest());
            if (response.ErrorException == null)
            {
                var responseContent = HtmlUtilities.ConvertToText(response.Content);
                var index = responseContent.IndexOf("Version", StringComparison.Ordinal);
                var newestVersion = responseContent.Substring(index + 8, 7);

                var currentVersion = PackageVersionToReadableString(Package.Current.Id.Version);
                if (!currentVersion.Equals(newestVersion))
                {
                    NewVersionDialog dlg = new NewVersionDialog();
                    await dlg.ShowAsync();
                }
            }
        }

        private static string PackageVersionToReadableString(PackageVersion packageVersion)
        {
            return $"{packageVersion.Major}.{packageVersion.Minor}.{packageVersion.Build}.{packageVersion.Revision}";
        }
    }
}
