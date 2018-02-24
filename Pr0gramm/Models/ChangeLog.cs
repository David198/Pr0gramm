using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0gramm.Models
{
    public class ChangeLogItem
    {
        public string Version { get; set; }
        public List<Changes> Changes { get; set; }
    }

    public class Changes
    {
        public string Type { get; set; }
        public string Change { get; set; }
    }

}
