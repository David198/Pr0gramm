using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0grammAPI.User
{
    public class UserLoginInfo
    {
        public bool Ban { get; set; }
        public string cache { get; set; }
        public string Identifier { get; set; }
        public int Qc { get; set; }
        public int Rt { get; set; }
        public bool Success { get; set; }
        public string Ts { get; set; }
    }
}
