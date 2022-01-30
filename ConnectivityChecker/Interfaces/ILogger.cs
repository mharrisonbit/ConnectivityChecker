using System;
using System.Threading.Tasks;

namespace ConnectivityChecker.Interfaces
{
    public interface ILogger
    {
        Task WriteLog(DateTime happenAt);
    }
}
