using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Caliburn.Micro;

using Pr0gramm.EventHandlers;
using Pr0gramm.Helpers;
using Pr0gramm.Services;
using Pr0gramm.Views;

using Pr0grammAPI.Feeds;
using Pr0grammAPI.Interfaces;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Pr0gramm.ViewModels
{
    public class ShellViewModel : Screen, IHandle<UserLoggedInEvent>, IHandle<TagSearchEvent>
    {
        private readonly WinRTContainer _container;
        private readonly IEventAggregator _eventAggregator;
        private readonly UserLoginService _userLoginService;
        private readonly SettingsService _settingsService;

        private string _header;

        private bool _isPaneOpen;
        private bool _isSettingPageOpen;
        private INavigationService _navigationService;

        private NavigationViewItemBase _selectedItem;
        private bool _sfwChecked;
        private bool _nsfwChecked;
        private bool _nsflChecked;
        private string _flagLabel;
        private bool _isUserLoggedIn;
        private bool _isMuted;
        private string _searchText;
        public string ActualUser { get; set; }

        public ShellViewModel(WinRTContainer container, IEventAggregator eventAggregator, UserLoginService userLoginService, SettingsService settingsService)
        {
            _container = container;
            _eventAggregator = eventAggregator;
            _userLoginService = userLoginService;
            _settingsService = settingsService;
            _eventAggregator.Subscribe(this);  
        }

        public bool ShowFlagButton
        {
            get { return IsUserLoggedIn && !IsSettingPageOpen; }
        }

        public bool IsUserLoggedIn
        {
            get { return _isUserLoggedIn; }
            set
            {
                _isUserLoggedIn = value;
                NotifyOfPropertyChange(nameof(ShowFlagButton));
            }
        }

        public bool SfwChecked
        {
            get => _sfwChecked;
            set
            {
                Set(ref _sfwChecked, value);
                UpdateFlagCheckboxEnabledState();
            }
        }

        public bool SfwCheckedEnabled
        {
            get
            {
                if (!NsflChecked && !NsfwChecked) return false;
                return true;
            }
        }

        public bool NsfwChecked
        {
            get => _nsfwChecked;
            set
            {
                Set(ref _nsfwChecked, value);
                UpdateFlagCheckboxEnabledState();
            }
        }

        public bool NsfwCheckedEnabled
        {
            get
            {
                if (!NsflChecked && !SfwChecked) return false;
                return true;
            }
        }

        public bool NsflChecked
        {
            get => _nsflChecked;
            set
            {
                Set(ref _nsflChecked, value);
                UpdateFlagCheckboxEnabledState();
            }
        }

        public bool NsflCheckedEnabled
        {
            get
            {
                if (!NsfwChecked && !SfwChecked) return false;
                return true;
            }
        }

        private void UpdateFlagCheckboxEnabledState()
        {
            NotifyOfPropertyChange(nameof(SfwCheckedEnabled));
            NotifyOfPropertyChange(nameof(NsfwCheckedEnabled));
            NotifyOfPropertyChange(nameof(NsflCheckedEnabled));
        }

        public string FlagLabel
        {
            get => _flagLabel;
            set => Set(ref _flagLabel, value);
        }

        public string Header
        {
            get => _header;
            set => Set(ref _header, value);
        }

        public bool IsSettingPageOpen
        {
            get => _isSettingPageOpen;
            set
            {
                Set(ref _isSettingPageOpen, value);
                NotifyOfPropertyChange(nameof(ShowFlagButton));
            }
        }

        public bool IsPaneOpen
        {
            get => _isPaneOpen;
            set
            {
                Set(ref _isPaneOpen, value);
                NotifyOfPropertyChange(nameof(ShowFlagButton));
            }
        }

        public bool IsMuted
        {
            get => _isMuted;
            set
            {
                Set(ref _isMuted, value);
                NotifyOfPropertyChange(nameof(IsMuted));
            }
        }

        public string SearchText
        {
            get { return _searchText; }
            set => Set(ref _searchText, value);
        }

        public NavigationViewItemBase SelectedItem
        {
            get => _selectedItem;
            set => Set(ref _selectedItem, value);
        }

        public ObservableCollection<NavigationViewItemBase> NavigationItems { get; set; } =
            new ObservableCollection<NavigationViewItemBase>();

        public void RefreshCommand()
        {
            PublishRefreshCommand();
        }

        private void PublishRefreshCommand()
        {
            var viewType = _navigationService.CurrentSourcePageType;
            if (viewType == null) return;
            var viewModelType = ViewModelLocator.LocateTypeForViewType(viewType, false);
            _eventAggregator.PublishOnUIThread(new RefreshEvent {CurrentViewType = viewModelType});
        }

        public void Mute()
        {
            _settingsService.IsMuted = true;
            _eventAggregator.PublishOnBackgroundThread(new MuteEvent(true));
        }

        public void UnMute()
        {
            _settingsService.IsMuted = false;
            _eventAggregator.PublishOnBackgroundThread(new MuteEvent(false));
        }

        public void FlagsUpdated()
        {
            FlagSelectorService.SetActualFeedFlagAsync(SfwChecked, NsfwChecked, NsflChecked, IsUserLoggedIn);
            PublishRefreshCommand();
            UpdateFlagLabel();
        }

        private void UpdateFlagLabel()
        {
            FlagLabel = "";
            if (SfwChecked && NsfwChecked && NsflChecked)
            {
                FlagLabel = "All".GetLocalized();
                return;
            }
            if (SfwChecked)
                FlagLabel += "SFW";
            if (NsfwChecked)
                FlagLabel += " NSFW";
            if (NsflChecked)
                FlagLabel += " NSFL";
        }

        public void Open()
        {
            IsPaneOpen = !IsPaneOpen;
        }


        protected override  void OnInitialize()
        {
            var view = GetView() as IShellView;
            _navigationService = view?.CreateNavigationService(_container);
            if (_navigationService != null)
                _navigationService.Navigated += NavigationService_Navigated;
            PopulateNavItems();
            NotifyOfPropertyChange(nameof(ShowFlagButton));
            InitializeFeedFlags();
            IsMuted = true;
            Mute();
        }

        public void Search()
        {
            _eventAggregator.PublishOnUIThread(new SearchFeedItemsEvent(SearchText));
        }

        public void ClearSearch()
        {
            SearchText = "";
            _eventAggregator.PublishOnUIThread(new SearchFeedItemsEvent(SearchText));
        }

        private async void InitializeFeedFlags()
        {
             if(!_settingsService.AlwaysStartWithSfw)
                await FlagSelectorService.InitializeAsync();
            switch (FlagSelectorService.ActualFlag)
            {
                case FeedFlags.NSFW:
                    NsfwChecked = true;
                    break;
                case FeedFlags.NSFL:
                    NsflChecked = true;
                    break;
                case FeedFlags.SFW:
                    SfwChecked = true;
                    break;
                case FeedFlags.SFWLogin:
                    SfwChecked = true;
                    break;
                case FeedFlags.SFWNSFW:
                    SfwChecked = true;
                    NsfwChecked = true;
                    break;
                case FeedFlags.SFWNSFL:
                    SfwChecked = true;
                    NsflChecked = true;
                    break;
                case FeedFlags.ALL:
                    SfwChecked = true;
                    NsflChecked = true;
                    NsfwChecked = true;
                    break;
                case FeedFlags.NSFWNSFL:
                    NsflChecked = true;
                    NsfwChecked = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            UpdateFlagLabel();
        }

        private void PopulateNavItems()
        {
            NavigationItems.Clear();
            NavigationItems.Add(new NavigationViewItem
            {
                Content = "Shell_Top".GetLocalized(),
                Icon = new SymbolIcon(Symbol.Home),
                Tag = typeof(TopViewModel)
            });

            NavigationItems.Add(new NavigationViewItem
            {
                Content = "Shell_New".GetLocalized(),
                Icon = new SymbolIcon(Symbol.Up),
                Tag = typeof(NewViewModel)
            });
            //NavigationItems.Add(new NavigationViewItem
            //{
            //    Content = "Shell_Controversial".GetLocalized(),
            //    Icon = new SymbolIcon(Symbol.SolidStar),
            //    Tag = typeof(ControversalViewModel)
            //});
            NavigationItems.Add(new NavigationViewItemSeparator());
            if (_userLoginService.IsLoggedIn)
            {
                SetLoginNavItems();
                ActualUser = _userLoginService.ActualUser;
                IsUserLoggedIn = true;
                _userLoginService.IsLoggedIn = true;
                if (_settingsService.AlwaysStartWithSfw)
                    FlagSelectorService.SetActualFeedFlagAsync(true, NsfwChecked, NsflChecked, IsUserLoggedIn);
            }
            else
            {
                NavigationItems.Add(new NavigationViewItem
                {
                    Content = "Login".GetLocalized(),
                    Icon = new SymbolIcon(Symbol.Contact)
                });
            }
        }

        public void ItemInvoked(NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                _navigationService.NavigateToViewModel(typeof(SettingsViewModel));
                return;
            }
            if (args.InvokedItem.ToString().Equals("Login".GetLocalized()))
            {
                _userLoginService.ShowUserLogin();

                return;
            }
            if (args.InvokedItem.ToString().Equals("Logout".GetLocalized()))
            {
                LogoutUser();
                return;
            }

            Navigate(args.InvokedItem);
        }

        private void Navigate(object item)
        {
            var navigationItem = NavigationItems.FirstOrDefault(items => items.Content.Equals(item.ToString()));
            if (navigationItem != null)
                _navigationService.NavigateToViewModel(navigationItem.Tag as Type);
        }

        private void NavigationService_Navigated(object sender, NavigationEventArgs e)
        {
            var viewType = _navigationService.CurrentSourcePageType;
            var viewModelType = ViewModelLocator.LocateTypeForViewType(viewType, false);
            SearchText = "";
            var navigationitem = NavigationItems?.FirstOrDefault(i => i.Tag as Type == viewModelType);
            if (navigationitem != null)
            {
                SelectedItem = navigationitem;
                if (SelectedItem.Content != null) Header = SelectedItem.Content.ToString();
                IsSettingPageOpen = false;
            }
            else
            {
                IsSettingPageOpen = true;
                Header = "Shell_Settings".GetLocalized();
            }
        }

        private void LogoutUser()
        {
            if (_userLoginService.DeleteUser(ActualUser))
            {
                NavigationItems.Remove(NavigationItems.First(item =>
                {
                    if (item.Content != null)
                        return item.Content.Equals("Logout".GetLocalized());
                    return false;
                }));
                NavigationItems.Insert(NavigationItems.Count, new NavigationViewItem
                {
                    Content = "Login".GetLocalized(),
                    Icon = new SymbolIcon(Symbol.Contact)
                });
                IsUserLoggedIn = false;
                FlagSelectorService.SetActualFeedFlagAsync(SfwChecked, false, false, IsUserLoggedIn);
                PublishRefreshCommand();
            }
        }


        public void Handle(UserLoggedInEvent message)
        {
            SetLoginNavItems();
            ActualUser = message.UserName;
            IsUserLoggedIn = true;
            _userLoginService.IsLoggedIn = true;
            if(_settingsService.AlwaysStartWithSfw)
                FlagSelectorService.SetActualFeedFlagAsync(true, NsfwChecked, NsflChecked, IsUserLoggedIn);
            PublishRefreshCommand();
        }

        private void SetLoginNavItems()
        {
            NavigationItems.Remove(NavigationItems.FirstOrDefault(item =>
            {
                if (item.Content != null)
                    return item.Content.Equals("Login".GetLocalized());
                return false;
            }));
            NavigationItems.Insert(NavigationItems.Count, new NavigationViewItem
            {
                Content = "Logout".GetLocalized(),
                Icon = new SymbolIcon(Symbol.BlockContact)
            });
        }

        public void Handle(TagSearchEvent message)
        {
            SearchText = message.Tag;
        }
    }
}
