using System;

namespace Pr0gramm.EventHandlers
{
    public class CameraControlEventArgs : EventArgs
    {
        public CameraControlEventArgs(string photo)
        {
            Photo = photo;
        }

        public string Photo { get; set; }
    }
}
