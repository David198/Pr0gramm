using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Caliburn.Micro;
using Pr0gramm.EventHandlers;
using Pr0gramm.Helpers;
using Pr0gramm.Services;
using Pr0gramm.Views;

namespace Pr0gramm.ViewModels
{
    public class ShellViewModel : Screen
    {
        private readonly WinRTContainer _container;
        private readonly IEventAggregator _eventAggregator;

        private string _header;

        private bool _isPaneOpen;
        private bool _isSettingPageOpen;
        private INavigationService _navigationService;

        private NavigationViewItem _selectedItem;

        public ShellViewModel(WinRTContainer container, IEventAggregator eventAggregator)
        {
            _container = container;
            _eventAggregator = eventAggregator;
        }

        public string Header
        {
            get => _header;
            set => Set(ref _header, value);
        }

        public bool IsSettingPageOpen
        {
            get => _isSettingPageOpen;
            set => Set(ref _isSettingPageOpen, value);
        }

        public bool IsPaneOpen
        {
            get => _isPaneOpen;
            set => Set(ref _isPaneOpen, value);
        }

        public NavigationViewItem SelectedItem
        {
            get => _selectedItem;
            set => Set(ref _selectedItem, value);
        }

        public ObservableCollection<NavigationViewItem> NavigationItems { get; } =
            new ObservableCollection<NavigationViewItem>();

        public void RefreshCommand()
        {
            var viewType = _navigationService.CurrentSourcePageType;
            if (viewType == null) return;
            var viewModelType = ViewModelLocator.LocateTypeForViewType(viewType, false);
            _eventAggregator.PublishOnUIThread(new RefreshEvent {CurrentViewType = viewModelType});
        }

        public void Mute()
        {
            SettingsService.IsMuted = true;
            _eventAggregator.PublishOnBackgroundThread(new MuteEvent(true));
        }

        public void UnMute()
        {
            SettingsService.IsMuted = false;
            _eventAggregator.PublishOnBackgroundThread(new MuteEvent(false));
        }


        public void Open()
        {
            IsPaneOpen = !IsPaneOpen;
        }


        protected override void OnInitialize()
        {
            var view = GetView() as IShellView;
            _navigationService = view?.CreateNavigationService(_container);
            if (_navigationService != null)
                _navigationService.Navigated += NavigationService_Navigated;
            PopulateNavItems();
        }


        private void NavigationService_Navigated(object sender, NavigationEventArgs e)
        {
            var viewType = _navigationService.CurrentSourcePageType;
            var viewModelType = ViewModelLocator.LocateTypeForViewType(viewType, false);
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
        }

        public void ItemInvoked(NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                _navigationService.NavigateToViewModel(typeof(SettingsViewModel));

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
    }
}
