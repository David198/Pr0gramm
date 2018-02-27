using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Caliburn.Micro;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Pr0gramm.Models;

namespace Pr0gramm.Views
{
    public sealed partial class WhatsNewDialog : ContentDialog
    {
        public BindableCollection<ChangeLogItem> ChangeLogItems { get; set; }

        public WhatsNewDialog()
        {
            // TODO WTS: Update the contents of this dialog every time you release a new version of the app
            InitializeComponent();
            DataContext = this;
            ChangeLogItems = new BindableCollection<ChangeLogItem>();
            LoadChangeLog();
        }

        private async void LoadChangeLog()
        {
            string filepath = @"Assets\ChangeLog.json";
            StorageFolder folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFile file = await folder.GetFileAsync(filepath); 
            var data = await FileIO.ReadTextAsync(file);
            ChangeLogItems.AddRange(JsonConvert.DeserializeObject<ChangeLogItem[]>(data, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            }).ToList());

        }
    }
}
