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


        private async Task<T> ExecuteAsync<T>(RestRequest request,bool setCookies) where T : new()
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
                        DecodeLoginCookie(cookie);
                    }
                    client.CookieContainer.Add(new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
                }
            }
            return response.Data;
        }

        private void DecodeLoginCookie(RestResponseCookie cookie)
        {
            try
            {
                var encodedText = HttpUtility.UrlDecode(cookie.Value, Encoding.UTF8);
                UserCookie = JsonConvert.DeserializeObject<UserCookie>(encodedText, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                Nonce = UserCookie.Id.Substring(0, 16);
            }
            catch (Exception e)
            {
                HockeyClient.Current.TrackException(e);
            }
        }

        public async Task<Feed> GetFeed(FeedFlags flags, bool promoted, string searchTags)
        {
            var request = new RestRequest {Resource = PROITEMSGET};
            FinishRequest(flags, promoted, request, searchTags);
            return await ExecuteAsync<Feed>(request,false);
        }

        public async Task<Feed> GetOlderFeed(int id, FeedFlags flags, bool promoted, string searchTags)
        {
            var request = new RestRequest {Resource = PROITEMSGET};
            request.AddParameter("older", id);
            FinishRequest(flags, promoted, request, searchTags);
            return await ExecuteAsync<Feed>(request,false);
        }

        private static void FinishRequest(FeedFlags flags, bool promoted, RestRequest request, string searchwords)
        {
            request.AddParameter("flags", (int) flags);
            if (promoted)
                request.AddParameter("promoted", 1);
            if (!string.IsNullOrEmpty(searchwords))
                request.AddParameter("tags", searchwords);
        }

        public async Task<FeedItemCommentItem> GetFeedItemComments(int id)
        {
            var request = new RestRequest {Resource = PROCOMMENTGET};
            request.AddParameter("itemId", id);
            return await ExecuteAsync<FeedItemCommentItem>(request,false);
        }

        public async Task<bool> Login(string name, string password)
        {
            var request = new RestRequest(Method.POST) {Resource = PROLOGIN};
            client.CookieContainer = new CookieContainer();
            request.AddParameter("name", name);
            request.AddParameter("password", password);
            var loginInfo = await ExecuteAsync<UserLoginInfo>(request,true);
            if (loginInfo.Success)
            {
                return true;
            }
            if (loginInfo.Ban)
            {
                throw new BannedException();
            }
            return false;
        }

        public async Task<ProfileInfo> GetUserProfileInfo(string name, FeedFlags flags)
        {
            var userInfoRequest = new RestRequest(Method.GET) { Resource = PROUSERINFO };
            userInfoRequest.AddParameter("name", name);
            userInfoRequest.AddParameter("flags", (int) flags);
            var userLoginInfo = await ExecuteAsync<ProfileInfo>(userInfoRequest,false);
            return userLoginInfo;
        }

        public async Task<UserSyncInfo> UserSync(int offset)
        {
            var userInfoRequest = new RestRequest(Method.GET) { Resource = PROUSERSYNC };
            userInfoRequest.AddParameter("offset", offset);
            var userSyncInfo = await ExecuteAsync<UserSyncInfo>(userInfoRequest,false);
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