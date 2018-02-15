using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Caliburn.Micro;
using Pr0gramm.Services;

namespace Pr0gramm.ViewModels
{
    public class SettingsViewModel : Screen
    {
        private readonly SettingsService _settingsService;

        // TODO WTS: Add other settings as necessary. For help see https://github.com/Microsoft/WindowsTemplateStudio/blob/master/docs/pages/settings.md
        private ElementTheme _elementTheme = ThemeSelectorService.Theme;

        private string _versionDescription;
        private bool _feedViewerExtraCommentColumn;
        private bool _alwaysStartSfw;

        public ElementTheme ElementTheme
        {
            get => _elementTheme;

            set => Set(ref _elementTheme, value);
        }

        public bool FeedViewerExtraCommentColumn
        {
            get => _feedViewerExtraCommentColumn;
            set
            {
                _settingsService.SaveFeedViewerExtraColumnActive(value);
                Set(ref _feedViewerExtraCommentColumn, value);
            }
        }

        public bool AlwaysStartSfw
        {
            get => _alwaysStartSfw;
            set
            {
                _settingsService.SaveAlwaysStartWithSFW(value);
                Set(ref _alwaysStartSfw, value);
            }
        }

        public SettingsViewModel(SettingsService settingsService)
        {
            _settingsService = settingsService;
            FeedViewerExtraCommentColumn = _settingsService.FeedViewerExtraColumnVisible;
            AlwaysStartSfw = _settingsService.AlwaysStartWithSfw;
        }

        public string VersionDescription
        {
            get => _versionDescription;

            set => Set(ref _versionDescription, value);
        }

        public async void SwitchTheme(ElementTheme theme)
        {
            await ThemeSelectorService.SetThemeAsync(theme);
            ElementTheme = theme;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();
            VersionDescription = GetVersionDescription();
        }

        private string GetVersionDescription()
        {
            var package = Package.Current;
            var packageId = package.Id;
            var version = packageId.Version;

            return $"{package.DisplayName} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
    }
}
