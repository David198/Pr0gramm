using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0grammAPI.User
{
   public class ProfileInfo
    {
        public List<Badges> Badges { get; set; }
        public int CommentCount { get; set; }
        public List<Comment> Comments { get; set; }
        public int FollowCount { get; set; }
        public int LikeCount { get; set; }
        public List<Likes> Likes { get; set; }
        public bool LikesArePublic { get; set; }
        public int TagCount { get; set; }
        public List<Uploads> Uploads { get; set; }
        public UserSyncInfo UserSyncInfo { get; set; }

    }
}
