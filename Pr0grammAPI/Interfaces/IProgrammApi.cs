using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Pr0grammAPI.Feeds;
using Pr0grammAPI.User;
using RestSharp;

namespace Pr0grammAPI.Interfaces
{
    public interface IProgrammApi
    {
        Task<Feed> GetFeed(int promoted, int following, int older, int newer, int feedFlags, string searchTags, string likes, bool self, string user);
        Task<FeedItemCommentItem> GetFeedItemComments(int id);
        Task<UserCookie> Login(string accountSid, string password);
        Task<ProfileInfo> GetUserProfileInfo(string name, FeedFlags flags);
        Task<UserSyncInfo> UserSync(int offset);
        Task VoteItem(int id, int voteState);
        Task VoteTag(int id, int voteState);
        Task VoteComment(int id, int voteState);
    }
}