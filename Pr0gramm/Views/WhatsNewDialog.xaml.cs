using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pr0gramm.Models;

namespace Pr0gramm.Views
{
    public sealed partial class WhatsNewDialog : ContentDialog
    {
        public List<ChangeLogItem> ChangeLogItems { get; set; }

        public WhatsNewDialog()
        {
            // TODO WTS: Update the contents of this dialog every time you release a new version of the app
            InitializeComponent();
            DataContext = this;
            LoadChangeLog();
        }

        private async void LoadChangeLog()
        {
            string filepath = @"Assets\ChangeLog.json";
            StorageFolder folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile file = await folder.GetFileAsync(filepath); 
            var data = await FileIO.ReadTextAsync(file);
            ChangeLogItems = JsonConvert.DeserializeObject<ChangeLogItem[]>(data, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }).ToList();
        }
    }
}
