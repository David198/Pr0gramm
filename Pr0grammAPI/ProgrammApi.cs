﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Pr0grammAPI.Feeds;
using Pr0grammAPI.Interfaces;
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

        #endregion

        private RestClient client = new RestClient(PR0URL);


        private async Task<T> Execute<T>(RestRequest request) where T : new()
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
            return response.Data;
        }

        public async Task<Feed> GetFeed(FeedFlags flags, bool promoted, string searchTags)
        {
            var request = new RestRequest {Resource = PROITEMSGET};
            FinishRequest(flags, promoted, request, searchTags);
            return await Execute<Feed>(request);
        }

        public async Task<Feed> GetOlderFeed(int id, FeedFlags flags, bool promoted, string searchTags)
        {
            var request = new RestRequest {Resource = PROITEMSGET};
            request.AddParameter("older", id);
            FinishRequest(flags, promoted, request, searchTags);
            return await Execute<Feed>(request);
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
            return await Execute<FeedItemCommentItem>(request);
        }

        public async Task<User.User> Login(string accountSid, string password)
        {
            var request = new RestRequest(Method.POST) {Resource = PROLOGIN};
            client.CookieContainer = new CookieContainer();
            client.Authenticator = new SimpleAuthenticator("name", accountSid, "password", password);
            var loginInfo = await Login<User.UserLoginInfo>(request);
            if (loginInfo.Success)
            {
                var userInfoRequest = new RestRequest(Method.GET) {Resource = PROUSERSYNC};
                userInfoRequest.AddParameter("offset", 577);
                client.Authenticator = null;
                return await Login<User.User>(userInfoRequest);
            }
            else
            {
                return null;
            }
        }

        private async Task<T> Login<T>(RestRequest request) where T : new()
        {
            client.Proxy = new WebProxy("127.0.0.1", 8888);

            var response = await client.ExecuteTaskAsync<T>(request);

            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response.  Check inner details for more info.";
                var programmException = new ApplicationException(message, response.ErrorException);
                throw programmException;
            }
            if (response.Cookies.Count > 0)
            {
                foreach (var cookie in response.Cookies)
                {
                    client.CookieContainer.Add(new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain));
                }
            }
            return response.Data;
        }
    }
}