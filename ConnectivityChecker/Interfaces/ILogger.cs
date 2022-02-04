using System;
using System.Threading.Tasks;

namespace ConnectivityChecker.Interfaces
{
    public interface ILogger
    {
        Task<bool> DeleteAllIssues();
        Task WriteLog(DateTime happenAt);
    }
}
