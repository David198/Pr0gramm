using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.HockeyApp;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;

namespace Pr0grammAPI.User
{
   public class UserCookie
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public bool Paid { get; set; }
        public bool Admin { get; set; }

        public string DecodedString { get; set; } 

        public UserCookie()
        {

        }


        private void DecodeLoginCookie(RestResponseCookie cookie)
        {
          
        }

        public UserCookie(string rawString)
        {
            try
            {
                DecodedString = HttpUtility.UrlDecode(rawString, Encoding.UTF8);
                 var cookie  = JsonConvert.DeserializeObject<UserCookie>(DecodedString, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                this.Id = cookie.Id;
                this.Admin = cookie.Admin;
                this.Name = cookie.Name;
                this.Paid = cookie.Paid;
            }
            catch (Exception e)
            {
                HockeyClient.Current.TrackException(e);
            }
        }
    }

}
