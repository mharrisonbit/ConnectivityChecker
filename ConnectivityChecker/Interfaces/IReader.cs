using System.Collections.Generic;
using System.Threading.Tasks;
using ConnectivityChecker.Models;

namespace ConnectivityChecker.Interfaces
{
    public interface IReader
    {
        Task<List<Issue>> GetLogFile();
    }
}
