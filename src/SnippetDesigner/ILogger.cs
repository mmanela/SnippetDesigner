using System;
using System.Threading.Tasks;

namespace Microsoft.SnippetDesigner
{
    public interface ILogger
    {
        Task LogAsync(string message, string source, LogType logType);
        Task LogAsync(string message, string source, Exception e);
        Task MessageBoxAsync(string title, string message, LogType logType);


        void Log(string message, string source, LogType logType);
        void Log(string message, string source, Exception e);
        void MessageBox(string title, string message, LogType logType);
    }
}
