using System;
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

        public ProgrammApi() { }


        #endregion

        #region VARIABLES
        private const string PR0URL = "https://pr0gramm.com";
        private const string PROITEMSGET ="/api/items/get";
        private const string PROCOMMENTGET = "/api/items/info";


        #endregion

        private string _accountSid;
        private string _secretKey;

        public void SetSID(string accountSid, string secretKey)
        {
            _accountSid = accountSid;
            _secretKey = secretKey; 
        }


        private async Task<T> Execute<T>(RestRequest request,string baseURL) where T : new()
        {
            RestClient client = new RestClient(baseURL);
            client.Proxy = new WebProxy("127.0.0.1", 8888);

            if (String.IsNullOrEmpty(_accountSid) && !String.IsNullOrEmpty(_secretKey))
            {
                client.Authenticator = new HttpBasicAuthenticator(_accountSid, _secretKey);
                request.AddParameter("AccountSid", _accountSid, ParameterType.UrlSegment); // used on every request
                client.CookieContainer = new CookieContainer();
            } 
            var response = await client.ExecuteTaskAsync<T>(request);

            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response.  Check inner details for more info.";
                var programmException = new ApplicationException(message, response.ErrorException);
                throw programmException;
            }
            return response.Data;
        }

        public async Task<Feed> GetFeed(FeedFlags flags, bool promoted)
        {
            var request = new RestRequest {Resource = PROITEMSGET};
            request.AddParameter("flags", (int)flags);
            if (promoted)
                request.AddParameter("promoted", 1);
            return await Execute<Feed>(request, PR0URL);
        }

        public async Task<Feed> GetOlderFeed(int id, FeedFlags flags, bool promoted)
        {
            var request = new RestRequest {Resource = PROITEMSGET};
            request.AddParameter("older", id);
            request.AddParameter("flags", (int)flags);
            if (promoted)
                request.AddParameter("promoted", 1);
            return await Execute<Feed>(request, PR0URL);
        }

        public async Task<FeedItemCommentItem> GetFeedItemComments(int id)
        {
            var request = new RestRequest { Resource = PROCOMMENTGET };
            request.AddParameter("itemId", id);
            return await Execute<FeedItemCommentItem>(request, PR0URL);
        }
    }
}
