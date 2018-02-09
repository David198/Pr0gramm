using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;

namespace Pr0gramm.Models
{
    public class DragDropCompletedData
    {
        public DataPackageOperation DropResult { get; set; }

        public IReadOnlyList<object> Items { get; set; }
    }
}
