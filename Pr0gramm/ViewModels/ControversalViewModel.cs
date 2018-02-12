using System;
using Caliburn.Micro;
using Pr0gramm.Helpers;
using Pr0gramm.Services;
using Pr0grammAPI.Interfaces;

namespace Pr0gramm.ViewModels
{
    public class ControversalViewModel : FeedViewerViewModelBase
    {
        public ControversalViewModel(IProgrammApi programmApi, IEventAggregator iEventAggregator,
            ToastNotificationsService toastNotificationsService) : base(programmApi, iEventAggregator,
            toastNotificationsService)
        {
            //FixSearchTag = "f:controversial";
        }

 
    }
}
