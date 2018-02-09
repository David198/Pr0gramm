using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Pr0grammAPI.Annotations;

namespace Pr0grammAPI.Feeds
{
    public class FeedItem : INotifyPropertyChanged
    {
        private const string PROTHUMBURL = "http://thumb.pr0gramm.com/";
        private const string PROIMAGEURL = "http://img.pr0gramm.com/";
        private const string PROVIDEOURL = "http://vid.pr0gramm.com/";

        public int Id { get; set; }
        public int Promoted { get; set; }
        public int Up { get; set; }
        public int Down { get; set; }
        public DateTime Created { get; set; }
        public string Image { get; set; }
        public string Thumb { get; set; }
        public string FullSize { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Audio { get; set; }
        public string Source { get; set; }
        public FeedFlags Flags { get; set; }
        public string User { get; set; }
        public int Mark { get; set; }


        public Uri ImageSource => IsVideo ? new Uri(PROVIDEOURL+Image) : new Uri(PROIMAGEURL + Image);

        public bool IsVideo => Image.Split('.')[1].Equals("mp4");

        public Uri ThumbSource => new Uri(PROTHUMBURL+ Thumb);

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
