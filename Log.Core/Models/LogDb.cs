using Log.Core.Exceptions;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using ILogger = Serilog.ILogger;

namespace Log.Core.Models
{
    public class LogDb
    {
        private static readonly ILogger _perfLogger;
        private static readonly ILogger _usageLogger;
        private static readonly ILogger _errorLogger;
        private static readonly ILogger _diagnosticLogger;


        static LogDb()
        {

            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_SQLSERVER").ToString();

            var columnOpts = GetSqlColumnOptions();

            _perfLogger = new LoggerConfiguration()
              .WriteTo
              .MSSqlServer(
              connectionString,
              sinkOptions: GetSinkOptions("PerfLogs"),
              columnOptions: columnOpts
              )
              .CreateLogger();



            _usageLogger = new LoggerConfiguration()
                .WriteTo
                .MSSqlServer(
                connectionString,
                sinkOptions: GetSinkOptions("UsageLogs"),
                columnOptions: columnOpts
                )
                .CreateLogger();

            _errorLogger = new LoggerConfiguration()
                .WriteTo
                .MSSqlServer(
                connectionString,
                sinkOptions: GetSinkOptions("ErrorLogs"),
                columnOptions: columnOpts
                )
                .CreateLogger();

            _diagnosticLogger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo
                .MSSqlServer(
                connectionString,
                sinkOptions: GetSinkOptions("DiagnosticLogs"),
                columnOptions: columnOpts
                )
                .CreateLogger();

        }

        public static void WritePerf(LogDetail infoToLog)
        {
            _perfLogger.Write(LogEventLevel.Information, GetColumName(),
                  infoToLog.Timestamp, infoToLog.Message, infoToLog.Layer,
                infoToLog.Location, infoToLog.Product, infoToLog.CustomException,
                infoToLog.ElapsedMilliseconds, infoToLog?.Exception?.ToBetterString() ?? "", infoToLog.Hostname,
                infoToLog.UserId, infoToLog.UserName, infoToLog.CorrelationId,
                infoToLog.AdditionalInfo);

        }

        public static void WriteUsage(LogDetail infoToLog)
        {
            _usageLogger.Write(LogEventLevel.Information, GetColumName(),
                 infoToLog.Timestamp, infoToLog.Message, infoToLog.Layer,
                infoToLog.Location, infoToLog.Product, infoToLog.CustomException,
                infoToLog.ElapsedMilliseconds, infoToLog?.Exception?.ToBetterString() ?? "", infoToLog.Hostname,
                infoToLog.UserId, infoToLog.UserName, infoToLog.CorrelationId,
                infoToLog.AdditionalInfo);
        }

        public static void WriteError(LogDetail infoToLog)
        {
            if (infoToLog.Exception != null)
            {
                var procName = FindProcName(infoToLog.Exception);
                infoToLog.Location = String.IsNullOrEmpty(procName) ? infoToLog.Location : procName;
                infoToLog.Message = GetMessageFromException(infoToLog.Exception);
            }

            _errorLogger.Write(LogEventLevel.Information, GetColumName(),
        infoToLog.Timestamp, infoToLog.Message, infoToLog.Layer,
        infoToLog.Location, infoToLog.Product, infoToLog.CustomException,
        infoToLog.ElapsedMilliseconds, infoToLog?.Exception?.ToBetterString() ?? "", infoToLog.Hostname,
        infoToLog.UserId, infoToLog.UserName, infoToLog.CorrelationId,
        infoToLog.AdditionalInfo);
        }
        public static void WriteDiagnostic(LogDetail infoToLog)
        {
            var writeDiagnostics = Convert.ToBoolean(Environment.GetEnvironmentVariable("ENABLE_DIAGNOSTICS").ToString());
            if (!writeDiagnostics)
            {
                return;
            }

            _diagnosticLogger.Write(LogEventLevel.Information, GetColumName(),
               infoToLog.Timestamp, infoToLog.Message, infoToLog.Layer,
                infoToLog.Location, infoToLog.Product, infoToLog.CustomException,
                infoToLog.ElapsedMilliseconds, infoToLog?.Exception?.ToBetterString() ?? "", infoToLog.Hostname,
                infoToLog.UserId, infoToLog.UserName, infoToLog.CorrelationId,
                infoToLog.AdditionalInfo);
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

            return ExceptionHelper.ToBetterString(ex);
        }


        private static MSSqlServerSinkOptions GetSinkOptions(string tableName)
        {
            return new MSSqlServerSinkOptions()
            {
                TableName = tableName,
                BatchPostingLimit = 1,
                AutoCreateSqlTable = true

            };
        }
        private static ColumnOptions GetSqlColumnOptions()
        {
            var colOptions = new ColumnOptions();
            // Remover todas as colunas adicionadas por padrão
            colOptions.Store.Remove(StandardColumn.Properties);
            colOptions.Store.Remove(StandardColumn.MessageTemplate);
            colOptions.Store.Remove(StandardColumn.Message);
            colOptions.Store.Remove(StandardColumn.Exception);
            colOptions.Store.Remove(StandardColumn.TimeStamp);
            colOptions.Store.Remove(StandardColumn.Level);

            // Adicionar colunos novas
            colOptions.AdditionalDataColumns = new Collection<DataColumn>
            {
                  new DataColumn { DataType = typeof(DateTime), ColumnName = "Timestamp"},
                  new DataColumn { DataType = typeof(string), ColumnName = "Product"},
                  new DataColumn { DataType = typeof(string), ColumnName = "Layer"},
                  new DataColumn { DataType = typeof(string), ColumnName = "Location"},
                  new DataColumn { DataType = typeof(string), ColumnName = "Message"},
                  new DataColumn { DataType = typeof(string), ColumnName = "Hostname"},
                  new DataColumn { DataType = typeof(string), ColumnName = "UserId"},
                  new DataColumn { DataType = typeof(string), ColumnName = "UserName"},
                  new DataColumn { DataType = typeof(string), ColumnName = "Exception"},
                  new DataColumn { DataType = typeof(string), ColumnName = "ElapsedMilliseconds"},
                  new DataColumn { DataType = typeof(string), ColumnName = "CorrelationId"},
                  new DataColumn { DataType = typeof(string), ColumnName = "CustomException"},
                  new DataColumn { DataType = typeof(string), ColumnName = "AdditionalInfo"}
            };

            return colOptions;
        }

        private static string GetColumName()
        {
            return "{Timestamp}{Message}{Layer}" +
                "{Location}{Product}{CustomException}" +
                "{ElapsedMilliseconds}{Exception}{Hostname}" +
                "{UserId}{UserName}{CorrelationId}" +
                "{AdditionalInfo}";
        }
    }
}
