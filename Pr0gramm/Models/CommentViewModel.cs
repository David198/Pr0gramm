using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Caliburn.Micro;
using Pr0gramm.Helpers;
using Pr0grammAPI.Annotations;
using Pr0grammAPI.Feeds;

namespace Pr0gramm.Models
{
    public class CommentViewModel : Comment, INotifyPropertyChanged
    {
        private int _parentDepth;

        public CommentViewModel(Comment comment) : base (comment)
        {
            if (comment.Parent == 0) comment.Parent = null;
            ParentDepthList = new BindableCollection<int>();
        }

        public BindableCollection<int> ParentDepthList { get; set; }

        public int ParentDepth
        {
            get => _parentDepth;
            set
            {
                if (value == _parentDepth) return;
                _parentDepth = value;
                for (var i = 1; i < value; i++)
                    ParentDepthList.Add(0);
            }
        }

        public bool ScoreIsAwailable
        {
            get
            {
                if ((DateTime.Now - Created).TotalHours > 1)
                    return true;
                return false;
            }
        }

        public string CreatedString => DateTimeUtlis.MakeCreatedString(Created);

        public event PropertyChangedEventHandler PropertyChanged;

        public void CalculateCommentDepth(List<Comment> comments)
        {
            if (Parent == null) return;
            var currentComment = (Comment)this;
            var depth = 0;

            while (true)
            {
                depth++;
                currentComment = comments.FirstOrDefault(comment => comment.Id.Equals(currentComment.Parent));
                if (currentComment == null) break;
            }
            ParentDepth = depth;
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
