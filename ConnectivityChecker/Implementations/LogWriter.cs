using System;
using System.IO;
using System.Threading.Tasks;
using ConnectivityChecker.Interfaces;
using ConnectivityChecker.Models;
using SQLite;
using Xamarin.Essentials;

namespace ConnectivityChecker.Implementations
{
    public class LogWriter : ILogger
    {
        public SQLiteAsyncConnection _db { get; private set; }

        public LogWriter()
        {
        }

        async Task Init()
        {
            try
            {
                if (_db != null)
                {
                    return;
                }
                var dataBasePath = Path.Combine(FileSystem.AppDataDirectory, "Issues.db");
                _db = new SQLiteAsyncConnection(dataBasePath);

                await _db.CreateTableAsync<Issue>();
            }
            catch (Exception ex)
            {
                //this.errorHandler.PrintErrorMessage(ex);
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// this is going to take in the time stamp and then log the issue.
        /// </summary>
        /// <param name="happenAt"></param>
        /// <returns></returns>
        public async Task WriteLog(DateTime happenAt)
        {
            try
            {
                if (DeviceInfo.Platform != DevicePlatform.macOS)
                {
                    await Init();

                    var issue = new Issue
                    {
                        LostOccurred = happenAt.ToString("MM/dd/yyyy h:mm tt")
                    };
                    await _db.InsertAsync(issue);
                }
                else
                {
                    //write log file.
                    string path = @"/Users/michael/Library/Issues.txt";
                    using (StreamWriter sw = File.AppendText(path))
                    {
                        sw.WriteLine(happenAt);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }



        public async Task<bool> DeleteAllIssues()
        {
            try
            {
                await Init();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return false;
        }
    }
}
