using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
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

        private readonly int timeToWait = 120;

        public MainViewModel(INavigationService navigationService, ILogger logger, IReader reader, IShare share) : base(navigationService)
        {
            this.logger = logger;
            this.reader = reader;
            this.share = share;

            StartCheckingBtn = new DelegateCommand(async () => await StartConnectivityCheck());
            GetLogsBtn = new DelegateCommand(async () => await BuildListOfIssues()).ObservesCanExecute(() => IsEnabled);
            ShareIssuesBtn = new DelegateCommand(async () => await ShareAllIssuesCmd());
            BtnActionTxt = "Start";
            SecondsTillNextCheck = timeToWait.ToString();

            IsEnabled = true;
            if (DeviceInfo.Platform == DevicePlatform.macOS)
            {
                IsEnabled = false;
            }

            ListOfIssues = new ObservableCollection<Issue>();
            ProgressBarValue = new int();
        }

        bool _IsEnabled;
        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set { SetProperty(ref _IsEnabled, value); }
        }

        decimal _ProgressBarValue;
        public decimal ProgressBarValue
        {
            get { return _ProgressBarValue; }
            set { SetProperty(ref _ProgressBarValue, value); }
        }

        string _BtnActionTxt;
        public string BtnActionTxt
        {
            get { return _BtnActionTxt; }
            set { SetProperty(ref _BtnActionTxt, value); }
        }

        string _SecondsTillNextCheck;
        public string SecondsTillNextCheck
        {
            get { return _SecondsTillNextCheck; }
            set { SetProperty(ref _SecondsTillNextCheck, value); }
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
                    await this.logger.WriteLog(DateTime.Now);
                    await BuildListOfIssues();
                }

                if (ListOfIssues.Count <=1)
                {
                    await BuildListOfIssues();
                }

                await LoopForPregressBar(timeToWait);
            }
        }

        async Task LoopForPregressBar(decimal timeToWait)
        {
            try
            {
                ProgressBarValue = 1;
                for (int i = 0; i < timeToWait; i++)
                {
                    //Thread.Sleep(100);
                    await Task.Delay(1000);
                    decimal percentComplete = i / timeToWait;
                    ProgressBarValue = 1 - percentComplete;
                    SecondsTillNextCheck = (timeToWait - i).ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task BuildListOfIssues()
        {
            ListOfIssues.Clear();
            var answer = await this.reader.GetLogFile();
            answer.Reverse();
            ListOfIssues = new ObservableCollection<Issue>(answer);
        }

        private async Task ShareAllIssuesCmd()
        {
            var emailBody = "";
            await BuildListOfIssues();
            
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
