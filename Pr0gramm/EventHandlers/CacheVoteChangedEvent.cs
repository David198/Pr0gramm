using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pr0gramm.Models.Enums;

namespace Pr0gramm.EventHandlers
{
   public class CacheVoteChangedEvent
    {
        public int Id { get; set; }
        public CacheVoteType VoteType { get; set; }

        public CacheVoteChangedEvent(int id, CacheVoteType voteType)
        {
            Id = id;
            VoteType = voteType;
        }
    }
}
