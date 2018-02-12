using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pr0grammAPI.Feeds
{
    public class Comment
    {
        public double Confidence { get; set; }
        public string Content { get; set; }
        public DateTime Created { get; set; }
        public int Down { get; set; }
        public int Id { get; set; }
        public int Mark { get; set; }
        public string Name { get; set; }
        private int? _parent;

        public Comment(Comment copyElement)
        {
            Content = copyElement.Content;
            Confidence = copyElement.Confidence;
            Created = copyElement.Created;
            Down = copyElement.Down;
            Id = copyElement.Id;
            Mark = copyElement.Mark;
            Name = copyElement.Name;
            Parent = copyElement.Parent;
            Up = copyElement.Up;

        }

        public Comment()
        {
            
        }

        public int? Parent
        {
            get {  return _parent.Equals(0)? null : _parent; }
            set { _parent = value; }
        }

        public int Up { get; set;  }
    }
}