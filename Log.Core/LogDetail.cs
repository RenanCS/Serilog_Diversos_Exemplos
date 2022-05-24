using System;
using System.Collections.Generic;

namespace Log.Core
{
    /*
     Dados que serão armazenados em cada registro de log
     */
    public class LogDetail
    {
        public LogDetail()
        {
            Timestamp = DateTime.Now;
        }

        public DateTime Timestamp { get; private set; }
        public string Message { get; set; }

        // WHERE

        public string Product { get; set; }
        public string Layer { get; set; }
        public string Location { get; set; }
        public string Hostname { get; set; }

        // WHO

        public string UserId { get; set; }
        public string UserName { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; }

        // EVERYTHING ELSE

        public long? ElapsedMilliseconds { get; set; }
        public Exception Exception { get; set; }
        public string CorrelationId { get; set; }
        public Dictionary<string, object> AdditionalInfo { get; set; }
    }

}
