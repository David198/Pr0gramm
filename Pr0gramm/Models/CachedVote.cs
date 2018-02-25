using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pr0gramm.Models.Enums;

namespace Pr0gramm.Models
{
   public class CachedVote
    {
        public int Itemid { get; }
        public CacheVoteType CacheVoteType { get; }
        public Vote Vote { get; }

        public CachedVote(int itemid, string cacheVoteType, string vote)
        {
            Itemid = itemid;

            CacheVoteType.TryParse(cacheVoteType, out CacheVoteType type);
            CacheVoteType = type;
            Vote.TryParse(vote, out Vote vt);
            CacheVoteType = type;
            Vote = vt;
        }
    }
}
