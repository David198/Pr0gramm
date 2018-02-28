using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0grammAPI.User
{
   public class UserCookie
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public bool Paid { get; set; }
        public bool Admin { get; set; }
    }
}
