using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.HockeyApp;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pr0grammAPI.Exceptions;
using Pr0grammAPI.Feeds;
using Pr0grammAPI.Interfaces;
using Pr0grammAPI.User;
using RestSharp;
using RestSharp.Authenticators;

namespace Pr0grammAPI
{
    public class ProgrammApi : IProgrammApi
    {
        #region Singleton

        public ProgrammApi()
        {
        }

        #endregion

        #region VARIABLES

        private const string PR0URL = "https://pr0gramm.com";
        private const string PROITEMSGET = "/api/items/get";
        private const string PROCOMMENTGET = "/api/items/info";
        private const string PROLOGIN = "/api/user/login";
        private const string PROUSERSYNC = "/api/user/sync";
        private const string PROUSERINFO = "/api/profile/info";
        private const string PROITEMVOTE = " /api/items/vote";
        private const string PROTAGVOTE = " /api/tags/vote";
        private const string PROCOMMENTVOTE = " /api/comments/vote";

        private string Nonce;

        public UserCookie UserCookie { get; set; }

        #endregion

        private RestClient client = new RestClient(PR0URL);

        public void SetNonce(string newNonce)
        {
            Nonce = newNonce;
        }

        private async Task<T> ExecuteAsync<T>(RestRequest request, bool setCookies) where T : new()
        {
#if DEBUG
            client.Proxy = new WebProxy("127.0.0.1", 8888);
#endif
            var response = await client.ExecuteTaskAsync<T>(request);

            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response.  Check inner details for more info.";
                var programmException = new ApplicationException(message, response.ErrorException);
                throw programmException;
            }

            if (response.Cookies.Count > 0 && setCookies)
            {
                foreach (var cookie in response.Cookies)
                {
                    if (cookie.Name.Equals("me"))
                    {
                        UserCookie = new UserCookie(cookie.Value);
                        if (UserCookie != null)
                            Nonce = UserCookie.Id.Substring(0, 16);
                    }

                    client.CookieContainer.Add(new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
                }
            }

            return response.Data;
        }

        public async Task<Feed> GetFeed(int promoted, int following, int older, int newer, int feedFlags,
            string searchTags, string likes,
            bool self, string user)
        {
            var request = new RestRequest {Resource = PROITEMSGET};
            PrepareRequest(request, promoted, following, older, newer, feedFlags, searchTags, likes, self, user);
            return await ExecuteAsync<Feed>(request, false);
        }

        private void PrepareRequest(RestRequest request, int promoted, int following, int older, int newer,
            int feedFlags, string searchTags, string likes, bool self, string user)
        {
            if (older != 0)
                request.AddParameter("older", older);
            request.AddParameter("flags", feedFlags);
            if (promoted == 1)
                request.AddParameter("promoted", 1);
            if (!string.IsNullOrEmpty(searchTags))
                request.AddParameter("tags", searchTags);
        }

        public async Task<FeedItemCommentItem> GetFeedItemComments(int id)
        {
            var request = new RestRequest {Resource = PROCOMMENTGET};
            request.AddParameter("itemId", id);
            return await ExecuteAsync<FeedItemCommentItem>(request, false);
        }

        public async Task<UserCookie> Login(string name, string password)
        {
            var request = new RestRequest(Method.POST) {Resource = PROLOGIN};
            client.CookieContainer = new CookieContainer();
            request.AddParameter("name", name);
            request.AddParameter("password", password);
            var loginInfo = await ExecuteAsync<UserLoginInfo>(request, true);
            if (loginInfo.Success)
            {
                return UserCookie;
            }

            if (loginInfo.Ban)
            {
                throw new BannedException();
            }

            return null;
        }

        public async Task<ProfileInfo> GetUserProfileInfo(string name, FeedFlags flags)
        {
            var userInfoRequest = new RestRequest(Method.GET) {Resource = PROUSERINFO};
            userInfoRequest.AddParameter("name", name);
            userInfoRequest.AddParameter("flags", (int) flags);
            var userLoginInfo = await ExecuteAsync<ProfileInfo>(userInfoRequest, false);
            return userLoginInfo;
        }

        public async Task<UserSyncInfo> UserSync(int offset)
        {
            var userInfoRequest = new RestRequest(Method.GET) {Resource = PROUSERSYNC};
            userInfoRequest.AddParameter("offset", offset);
            var userSyncInfo = await ExecuteAsync<UserSyncInfo>(userInfoRequest, false);
            return userSyncInfo;
        }

        public async Task VoteItem(int id, int voteState)
        {
            var voteRequest = new RestRequest(Method.POST){Resource = PROITEMVOTE};
            voteRequest.AddParameter("id", id);
            voteRequest.AddParameter("vote", voteState);
            voteRequest.AddParameter("_nonce", Nonce);
            await ExecuteAsync<object>(voteRequest, false);
        }

        public async Task VoteTag(int id, int voteState)
        {
            var voteRequest = new RestRequest(Method.POST) { Resource = PROTAGVOTE };
            voteRequest.AddParameter("id", id);
            voteRequest.AddParameter("vote", voteState);
            voteRequest.AddParameter("_nonce", Nonce);
            await ExecuteAsync<object>(voteRequest, false);
        }

        public async Task VoteComment(int id, int voteState)
        {
            var voteRequest = new RestRequest(Method.POST) { Resource = PROCOMMENTVOTE };
            voteRequest.AddParameter("id", id);
            voteRequest.AddParameter("vote", voteState);
            voteRequest.AddParameter("_nonce", Nonce);
            await ExecuteAsync<object>(voteRequest, false);
        }
    }
}