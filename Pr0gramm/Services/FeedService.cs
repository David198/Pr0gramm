using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pr0grammAPI.Feeds;
using Pr0grammAPI.Interfaces;

namespace Pr0gramm.Services
{
   public class FeedService
    {
        private readonly IProgrammApi _programmApi;
        private readonly CacheService _cacheService;
        private readonly ToastNotificationsService _toastNotificationsService;

        public FeedService(IProgrammApi programmApi, CacheService cacheService, ToastNotificationsService toastNotificationsService)
        {
            _programmApi = programmApi;
            _cacheService = cacheService;
            _toastNotificationsService = toastNotificationsService;
        }

        public async Task<Feed> GetFeed(int promoted, int following, int older, int newer, int feedFlags, string searchTags, string likes, bool self, string user)
        {
            try
            {
                var feed = await _programmApi.GetFeed(promoted, following, older, newer, feedFlags, searchTags, likes, self,
                    user);
                if (searchTags != null && searchTags.Contains("repost"))
                {
                    _cacheService.CacheReposts(feed.Items);
                }
                else
                {
                    FindReposts(promoted, following, older, newer, feedFlags, searchTags, likes, self, user);
                }
                return feed;
            }
            catch (ApplicationException)
            {
                _toastNotificationsService.ShowToastNotificationWebSocketExeception();
            }
            return null;
        }

        private async void FindReposts(int promoted, int following, int older, int newer, int feedFlags, string searchTags,
            string likes, bool self, string user)
        {
            if (searchTags == null)
                searchTags = "repost";
            else
            {
                searchTags += " repost";   
            }
            try
            {
                var repostInfoFeed = await _programmApi.GetFeed(promoted, following, older, newer, feedFlags, searchTags, likes,
                    self,
                    user);
                _cacheService.CacheReposts(repostInfoFeed.Items);
            }
            catch (ApplicationException)
            {

            }

        }
    }
}
