using Serilog;
using Serilog.Events;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;

namespace Log.Core
{
    public class Log
    {
        private static readonly ILogger _perfLogger;
        private static readonly ILogger _usageLogger;
        private static readonly ILogger _errorLogger;
        private static readonly ILogger _diagnosticLogger;

        static Log()
        {

            var pathProject = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;

            var path = Path.Combine(pathProject, @"files");
            _perfLogger = new LoggerConfiguration()
                .WriteTo.File(path: $"{path}\\perf.txt")
                .CreateLogger();

            _usageLogger = new LoggerConfiguration()
                .WriteTo.File(path: $"{path}\\usage.txt")
                .CreateLogger();

            _errorLogger = new LoggerConfiguration()
                .WriteTo.File(path: $"{path}\\error.txt")
                .CreateLogger();

            _diagnosticLogger = new LoggerConfiguration()
                            .WriteTo.File(path: $"{path}\\diagnostic.txt")
                            .CreateLogger();
        }

        public static void WritePerf(LogDetail infoToLog)
        {
            _perfLogger.Write(LogEventLevel.Information, "{@LogDetail}", infoToLog);
        }

        public static void WriteUsage(LogDetail infoToLog)
        {
            _usageLogger.Write(LogEventLevel.Information, "{@LogDetail}", infoToLog);
        }

        public static void WriteError(LogDetail infoToLog)
        {
            if (infoToLog.Exception != null)
            {
                var procName = FindProcName(infoToLog.Exception);
                infoToLog.Location = String.IsNullOrEmpty(procName) ? infoToLog.Location : procName;
                infoToLog.Message = GetMessageFromException(infoToLog.Exception);
            }

            _errorLogger.Write(LogEventLevel.Information, "{@LogDetail}", infoToLog);
        }

        private static string FindProcName(Exception ex)
        {
            var sqlEx = ex as SqlException;
            if (sqlEx != null)
            {
                var procName = sqlEx.Procedure;
                if (!string.IsNullOrEmpty(procName))
                {
                    return procName;
                }
            }

            if (!string.IsNullOrEmpty((string)ex.Data["Procedure"]))
            {
                return (string)ex.Data["Procedure"];
            }

            if (ex.InnerException != null)
            {
                return FindProcName(ex.InnerException);
            }


            return null;
        }

        private static string GetMessageFromException(Exception ex)
        {
            if (ex.InnerException != null)
            {
                return GetMessageFromException(ex.InnerException);
            }

            return ex.Message;
        }
        public static void WriteDiagnostic(LogDetail infoToLog)
        {
            var writeDiagnostics = Convert.ToBoolean(ConfigurationManager.AppSettings["EnableDiagnostics"]);
            if (!writeDiagnostics)
            {
                return;
            }

            _diagnosticLogger.Write(LogEventLevel.Information, "{@FlogDetail}", infoToLog);
        }
    }
}
