using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ConnectivityChecker.Interfaces;
using ConnectivityChecker.Models;
using Prism.Commands;
using Prism.Navigation;
using Xamarin.Essentials;
using Xamarin.Essentials.Interfaces;

namespace ConnectivityChecker.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public DelegateCommand StartCheckingBtn { get; private set; }
        public DelegateCommand GetLogsBtn { get; private set; }
        public DelegateCommand ShareIssuesBtn { get; private set; }

        private bool runCheck = false;
        private readonly ILogger logger;
        private readonly IReader reader;
        private readonly IShare share;

        public MainViewModel(INavigationService navigationService, ILogger logger, IReader reader, IShare share) : base(navigationService)
        {
            this.logger = logger;
            this.reader = reader;
            this.share = share;

            StartCheckingBtn = new DelegateCommand(async () => await StartConnectivityCheck());
            GetLogsBtn = new DelegateCommand(async () => await GetLogsCmd()).ObservesCanExecute(() => IsEnabled);
            ShareIssuesBtn = new DelegateCommand(async () => await ShareAllIssuesCmd());
            BtnActionTxt = "Start";

            IsEnabled = true;
            if (DeviceInfo.Platform == DevicePlatform.macOS)
            {
                IsEnabled = false;
            }

            ListOfIssues = new ObservableCollection<Issue>();
        }

        bool _IsEnabled;
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set { SetProperty(ref _IsEnabled, value); }
        }

        string _BtnActionTxt;
        public string BtnActionTxt
        {
            get { return _BtnActionTxt; }
            set { SetProperty(ref _BtnActionTxt, value); }
        }

        ObservableCollection<Issue> _ListOfIssues;
        public ObservableCollection<Issue> ListOfIssues
        {
            get { return _ListOfIssues; }
            set { SetProperty(ref _ListOfIssues, value); }
        }

        private async Task StartConnectivityCheck()
        {
            runCheck = !runCheck;
            BtnActionTxt = runCheck ? "Stop" : "Start";

            while (runCheck)
            {
                try
                {
                    var ping = new System.Net.NetworkInformation.Ping();
                    var result = ping.Send("www.google.com");
                }
                catch (Exception)
                {
                    var timeItHappened = DateTime.Now.ToString("MM/dd/yyyy h:mm tt");
                    await this.logger.WriteLog(DateTime.Now);
                    ListOfIssues.Add(new Issue
                        {
                            LostOccurred = timeItHappened
                        }
                    );
                }
                ListOfIssues.Reverse();
                //var timeToWait = 20000;//this is 10 seconds;
                var timeToWait = 120000;//this is 2 minutes
                await Task.Delay(timeToWait);
            }
        }

        private async Task GetLogsCmd()
        {
            ListOfIssues.Clear();
            //this is going to get the logs and then share them in a string format.
            var answer = await this.reader.GetLogFile();
            answer.Reverse();
            ListOfIssues = new ObservableCollection<Issue>(answer);
        }

        private async Task ShareAllIssuesCmd()
        {
            var emailBody = "";
            var answer = await this.reader.GetLogFile();
            answer.Reverse();
            ListOfIssues.Clear();
            ListOfIssues = new ObservableCollection<Issue>(answer);
            
            //this is going to create an email that can be sent to who ever needs to see the issues.
            foreach (var issue in ListOfIssues)
            {
                emailBody += $"{issue.LostOccurred},\n";
            }

            var fn = $"Internet_Issues.txt";
            var file = Path.Combine(FileSystem.CacheDirectory, fn);
            File.WriteAllText(file, emailBody);

            await this.share.RequestAsync(new ShareFileRequest
            {
                Title = $"Times the internet was not working",
                File = new ShareFile(file)
            });

        }
    }
}
