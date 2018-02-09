using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace Pr0grammAPI.Feeds
{
    public class Feed
    {
        public bool AtEnd { get; set; }
        public bool AtStart { get; set; }
        public string Error { get; set; }
        public List<FeedItem> Items { get; set; }
    }
}