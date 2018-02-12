﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0gramm.EventHandlers
{
    public class SearchFeedItemsEvent
    {
        public string SearchTags { get; set; }

        public SearchFeedItemsEvent(string searchString)
        {
           var tags = searchString.Split(" ");
            for (int i = 0; i < tags.Length; i++)
            {
                if (i == 0)
                    SearchTags += tags[i];
                else
                {
                    SearchTags+= " "+tags[i];
                }
            }
        }
    }
}
