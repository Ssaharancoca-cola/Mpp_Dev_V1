using System.IO;
using System;
using Microsoft.Extensions.Hosting;

namespace DAL.Common
{
    public class LogError : IDisposable
    {
        private readonly IHostEnvironment _hostEnvironment;

        public LogError(IHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
        }
        public LogError()
        {

        }
        public void LogErrorInTextFile(Exception ex)
        {
            string webRootPath = _hostEnvironment.ContentRootPath;
            string logFolderPath = Path.Combine(webRootPath, "ErrorLog");
            string logFilePath = Path.Combine(logFolderPath, "ErrorLog.txt");

            string message = $"Time: {DateTime.Now:dd/MM/yyyy hh:mm:ss tt}\n";
            message += "-----------------------------------------------------------\n";
            message += $"Message: {ex.Message}\n";
            message += $"StackTrace: {ex.StackTrace}\n";
            message += $"Source: {ex.Source}\n";
            message += $"TargetSite: {ex.TargetSite?.ToString()}\n";
            message += $"InnerException: {ex.InnerException}\n";
            message += "-----------------------------------------------------------\n";

            Directory.CreateDirectory(logFolderPath);
            File.AppendAllText(logFilePath, message);
        }

        public void Dispose()
        {
            //Dispose any resources if needed
        }

    }
}
