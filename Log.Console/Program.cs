using Log.Console.Models;
using Log.Core;
using Log.Data.CustomAdo;
using Log.Data.Dapper;
using System;
using System.Configuration;
using System.Data.SqlClient;

namespace Log.Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var flogginDetail = GetLogDetail("starting application", null);

            Log.Core.Log.WriteDiagnostic(flogginDetail);

            var tracker = new PerfTracker("FloggerConsole_Execution", "", flogginDetail.UserName, flogginDetail.Location, flogginDetail.Product, flogginDetail.Layer);

            var conection = ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString();

            LogExceptionConsole();
            LogExceptionADO(conection);
            LogExceptionDapperADO(conection);
            LogExceptionEntityFramework();


            flogginDetail = GetLogDetail("use flogging console", null);
            Log.Core.Log.WriteUsage(flogginDetail);

            flogginDetail = GetLogDetail("stopping app", null);
            Log.Core.Log.WriteDiagnostic(flogginDetail);

            tracker.Stop();

        }

        private static void LogExceptionADO(string conection)
        {

            using (var db = new SqlConnection(conection))
            {
                try
                {
                    db.Open();

                    //Raw ado.net Original
                    //var rawAdoSp = new SqlCommand("CreatedNewCustomer", db)
                    //{
                    //    CommandType = System.Data.CommandType.StoredProcedure
                    //};
                    //rawAdoSp.Parameters.Add(new SqlParameter("@Name", "waytoolongforitsowngood"));
                    //rawAdoSp.Parameters.Add(new SqlParameter("@TotalPurchases", 12000));
                    //rawAdoSp.Parameters.Add(new SqlParameter("@TotalReturns", 100.50M));
                    //rawAdoSp.ExecuteNonQuery();

                    // Raw ado.nert custom
                    var sp = new Sproc(db, "CreatedNewCustomer");
                    sp.SetParam("@Name", "waytoolongforitsowngood");
                    sp.SetParam("@TotalPurchases", 12000);
                    sp.SetParam("@NaTotalReturnsme", 100.50M);
                    sp.ExecNonQuery();

                }
                catch (Exception ex)
                {
                    var adoFloggingDetail = GetLogDetail("", ex);
                    Log.Core.Log.WriteError(adoFloggingDetail);
                }
            }
        }

        private static void LogExceptionDapperADO(string conection)
        {
            using (var db = new SqlConnection(conection))
            {
                try
                {
                    ////Dapper original
                    //db.Execute("CreateNewCustomer", new
                    //{
                    //    Name = "dappernametoolongtowork",
                    //    TotalPurchases = 12000,
                    //    TotalReturns = 100.50M
                    //}, commandType: System.Data.CommandType.StoredProcedure);

                    // Dapper custom
                    db.DapperProcNonQuery("CreateNewCustomer", new
                    {
                        Name = "dappernametoolongtowork",
                        TotalPurchases = 12000,
                        TotalReturns = 100.50M
                    });

                }
                catch (Exception ex)
                {
                    var adoFloggingDetail = GetLogDetail("", ex);
                    Log.Core.Log.WriteError(adoFloggingDetail);
                }
            }
        }


        private static void LogExceptionEntityFramework()
        {
            var contextDb = new CustomerDbContext();
            try
            {
                //Entity Framework
                var name = new SqlParameter("@Name", "waytoolongforitsowngood");
                var totalPurchases = new SqlParameter("@TotalPurchases", 12000);
                var totalReturns = new SqlParameter("@TotalReturns", 100.50M);

                contextDb.Database.ExecuteSqlCommand("EXEC CreateNewCustomer @Name, @TotalPurchases, @TotalReturns", name, totalPurchases, totalReturns);
            }
            catch (Exception ex)
            {
                var entityFloggingDetail = GetLogDetail("", ex);
                Log.Core.Log.WriteError(entityFloggingDetail);
            }

        }


        private static void LogExceptionConsole()
        {
            LogDetail flogginDetail;
            try
            {
                var ex = new Exception("Something bad has happened!");
                ex.Data.Add("input param", "nothing to see here");
                throw ex;
            }
            catch (Exception ex)
            {
                flogginDetail = GetLogDetail("", ex);
                Log.Core.Log.WriteError(flogginDetail);
            }

        }

        private static LogDetail GetLogDetail(string message, Exception ex)
        {
            return new LogDetail
            {
                Product = "Flogger",
                Location = "FloggerConsole",
                Layer = "Job",
                UserName = Environment.UserName,
                Hostname = Environment.MachineName,
                Message = message,
                Exception = ex
            };
        }
    }
}
