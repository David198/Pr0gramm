using System;

namespace Pr0gramm.Services
{
    public class OnBackgroundEnteringEventArgs : EventArgs
    {
        public OnBackgroundEnteringEventArgs(SuspensionState suspensionState, Type target)
        {
            SuspensionState = suspensionState;
            Target = target;
        }

        public SuspensionState SuspensionState { get; set; }

        public Type Target { get; }
    }
}
