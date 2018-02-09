using System.Threading.Tasks;
using Pr0grammAPI.Feeds;
using RestSharp;

namespace Pr0grammAPI.Interfaces
{
    public interface IProgrammApi
    {
        void SetSID(string accountSid, string secretKey);
        Task<Feed> GetFeed(FeedFlags feedFlags, bool promoted);
        Task<Feed> GetOlderFeed(int id, FeedFlags feedFlags, bool promoted);
        Task<FeedItemCommentItem> GetFeedItemComments(int id);
    }
}