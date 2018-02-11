using System.Threading.Tasks;
using Pr0grammAPI.Feeds;
using RestSharp;

namespace Pr0grammAPI.Interfaces
{
    public interface IProgrammApi
    {
        Task<Feed> GetFeed(FeedFlags feedFlags, bool promoted);
        Task<Feed> GetOlderFeed(int id, FeedFlags feedFlags, bool promoted);
        Task<FeedItemCommentItem> GetFeedItemComments(int id);
        Task<User.User> Login(string accountSid, string password);
    }
}