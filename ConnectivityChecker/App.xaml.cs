using ConnectivityChecker.Implementations;
using ConnectivityChecker.Interfaces;
using ConnectivityChecker.ViewModels;
using ConnectivityChecker.Views;
using Prism.Ioc;
using Xamarin.Essentials.Implementation;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;

namespace ConnectivityChecker
{
    public partial class App
    {
        public App()
        {
        }

        protected override async void OnInitialized()
        {
            InitializeComponent();

            var result = await NavigationService.NavigateAsync("/NavigationPage/MainView");

            if (!result.Success)
            {
                System.Diagnostics.Debugger.Break();
            }
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<MainView, MainViewModel>();

            containerRegistry.RegisterSingleton<ILogger, LogWriter>();
            containerRegistry.RegisterSingleton<IReader, LogReader>();
            containerRegistry.RegisterSingleton<IShare, ShareImplementation>();
        }
    }
}
