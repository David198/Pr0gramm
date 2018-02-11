using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0grammAPI.Feeds
{
    public enum FeedFlags
    {
        NSFW = 2,
        NSFL = 4,
        SFW = 1,
        SFWLogin = 9,
        SFWNSFW = 11,
        SFWNSFL = 13,
        ALL = 15,
        NSFWNSFL = 6
    }
}
