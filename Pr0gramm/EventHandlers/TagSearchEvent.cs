using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0gramm.EventHandlers
{
    /// <summary>
    /// Event when Tag is clicked FeedViewer
    /// </summary>
   public class TagSearchEvent
    {
        public string Tag;

        public TagSearchEvent(string tag)
        {
            Tag = tag;
        }
    }
}
