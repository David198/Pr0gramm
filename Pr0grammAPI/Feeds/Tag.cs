using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0grammAPI.Feeds
{
   public class TagItem
    {

        public TagItem(TagItem item)
        {
            Confidence = item.Confidence;
            Id = item.Id;
            Tag = item.Tag;
        }

        public TagItem() { }
        public double Confidence { get; set; }
        public int Id { get; set; }
        public string Tag { get; set; }
    }
}
