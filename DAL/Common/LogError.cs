using System.IO;
using System;
using Microsoft.Extensions.Hosting;

namespace DAL.Common
{
    public class LogError : IDisposable
    {
        void IDisposable.Dispose()
        {
        }
        
        public void LogErrorInTextFile(Exception ex)
        {
            string webRootPath = Directory.GetCurrentDirectory();          
            
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
        public void LogErrorInTextFileTest( string? userid, string? code, string? dimension)
        {
            string webRootPath = Directory.GetCurrentDirectory();

            string logFolderPath = Path.Combine(webRootPath, "ErrorLog");
            string logFilePath = Path.Combine(logFolderPath, "ErrorLog.txt");

            string message = $"Time: {DateTime.Now:dd/MM/yyyy hh:mm:ss tt}\n";
            message += "-----------------------------------------------------------\n";
            
            message += $"UserId: {userid ?? "Not provided"}\n";
            message += $"Code: {code ?? "Not provided"}\n";
            message += $"Code: {dimension ?? "Not provided"}\n";

            message += "-----------------------------------------------------------\n";

            Directory.CreateDirectory(logFolderPath);
            File.AppendAllText(logFilePath, message);
        }
        public LogError()
        {

        }

    }
}
