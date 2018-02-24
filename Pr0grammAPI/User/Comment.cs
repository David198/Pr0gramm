using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0grammAPI.User
{
   public class Comment
    {
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public int Down { get; set; }
        public int Id { get; set; }
        public int ItemId { get; set; }
        public string Thumb { get; set; }
        public int Up { get; set; }
    }
}
