using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ConnectivityChecker.Interfaces;
using ConnectivityChecker.Models;
using SQLite;
using Xamarin.Essentials;

namespace ConnectivityChecker.Implementations
{
    public class LogReader : IReader
    {
        public SQLiteAsyncConnection _db { get; private set; }

        public LogReader()
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
        /// This is going to return the information that has been logged in the DB.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Issue>> GetLogFile()
        {
            await Init();

            var issues = await _db.Table<Issue>().ToListAsync();

            return issues;
        }
    }
}
