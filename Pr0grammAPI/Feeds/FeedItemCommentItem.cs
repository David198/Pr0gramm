using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0grammAPI.Feeds
{
    public class FeedItemCommentItem
    {
        public string Chache { get; set; }
        public List<Comment> Comments { get; set; }

        public List<TagItem> Tags { get; set; }

        public int Qc { get; set; }
        public int Rt { get; set; }

        public int Ts { get; set; }

    }
}
