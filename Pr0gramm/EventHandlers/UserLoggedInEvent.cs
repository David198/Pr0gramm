using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0gramm.EventHandlers
{
    public class UserLoggedInEvent
    {
        public string UserName { get; set; }

        public UserLoggedInEvent(string userName)
        {
            UserName = userName;
        }
    }
}
