using System;
using System.Collections.Generic;
using System.Text;
using Caliburn.Micro;
using Microsoft.HockeyApp;
using Pr0gramm.Helpers;
using Pr0gramm.Services;
using Pr0gramm.ViewModels;
using Pr0gramm.Views;
using Pr0grammAPI;
using Pr0grammAPI.Interfaces;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Data.Html;
using Windows.UI.Xaml;
using RestSharp;

namespace Pr0gramm
{
    public sealed partial class App
    {
        private readonly Lazy<ActivationService> _activationService;

        private WinRTContainer _container;

        public App()
        {
#if !DEBUG
            HockeyClient.Current.Configure("d2ef27ea7e7342b3befa7922cacd4c47",
              new TelemetryConfiguration {EnableDiagnostics = true});
#endif

         

            InitializeComponent();
            EnteredBackground += App_EnteredBackground;
            Initialize();
            // Deferred execution until used. Check https://msdn.microsoft.com/library/dd642331(v=vs.110).aspx for further info on Lazy<T> class.
            _activationService = new Lazy<ActivationService>(CreateActivationService);
        }



        private ActivationService ActivationService => _activationService.Value;


        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (!args.PrelaunchActivated)
                await ActivationService.ActivateAsync(args);
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            await ActivationService.ActivateAsync(args);
        }

        protected override void Configure()
        {
// This configures the framework to map between TopViewModel and TopPage
// Normally it would map between MainPageViewModel and TopPage
            var config = new TypeMappingConfiguration
            {
                IncludeViewSuffixInViewModelNames = false
            };

            ViewLocator.ConfigureTypeMappings(config);
            ViewModelLocator.ConfigureTypeMappings(config);
            _container = new WinRTContainer();
            _container.RegisterWinRTServices();
            _container.Singleton<SettingsService>();
            _container.PerRequest<ToastNotificationsService>();
            _container.Singleton<UserLoginService>();
            _container.Singleton<IProgrammApi, ProgrammApi>();
            _container.PerRequest<ShellViewModel>();
            _container.PerRequest<TopViewModel>();
            _container.PerRequest<NewViewModel>();
            _container.Singleton<SettingsViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        private ActivationService CreateActivationService()
        {
            return new ActivationService(_container, typeof(TopViewModel), _container.GetInstance<SettingsService>(), new Lazy<UIElement>(CreateShell));
        }

        private UIElement CreateShell()
        {
            return new ShellPage();
        }

        private async void App_EnteredBackground(object sender, EnteredBackgroundEventArgs e)
        {
            var deferral = e.GetDeferral();
            await Singleton<SuspendAndResumeService>.Instance.SaveStateAsync();
            deferral.Complete();
        }

        protected override async void OnBackgroundActivated(BackgroundActivatedEventArgs args)
        {
            await ActivationService.ActivateAsync(args);
        }
    }
}
