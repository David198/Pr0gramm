using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0grammAPI.User
{
    public class User
    {
        public int Admin { get; set; }
        public int Banned { get; set; }
        public int Id { get; set; }
        public int Mark { get; set; }
        public string Name { get; set; }
        public DateTime Registered { get; set; }
        public int Score { get; set; }
    }
}
