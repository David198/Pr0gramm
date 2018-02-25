using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Pr0grammAPI.Feeds;
using Pr0grammAPI.User;
using RestSharp;

namespace Pr0grammAPI.Interfaces
{
    public interface IProgrammApi
    {
        Task<Feed> GetFeed(FeedFlags feedFlags, bool promoted, string searchTags);
        Task<Feed> GetOlderFeed(int id, FeedFlags feedFlags, bool promoted, string searchTags);
        Task<FeedItemCommentItem> GetFeedItemComments(int id);
        Task<bool> Login(string accountSid, string password);
        Task<ProfileInfo> GetUserProfileInfo(string name, FeedFlags flags);
        Task<UserSyncInfo> UserSync(int offset);
    }
}