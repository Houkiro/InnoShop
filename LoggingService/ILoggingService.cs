using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoggingService
{
    public interface ILoggingService
    {
        void LogInfo(string message);
        void LogError(string message, Exception ex);
        void LogWarning(string message);
        void LogDebug(string message);
    }

}
