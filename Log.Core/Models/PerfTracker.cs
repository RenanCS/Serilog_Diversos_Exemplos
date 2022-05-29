using Log.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace Log.Core
{
    public class PerfTracker
    {
        const string STARTEDTIME = "Started";
        const string FINISHEDTIME = "Finished";
        private readonly Stopwatch _sw;
        private readonly LogDetail _infoToLog;


        public PerfTracker(LogDetail details)
        {
            _sw = Stopwatch.StartNew();
            _infoToLog = details;

            var beginTime = DateTime.Now;
            if (_infoToLog.AdditionalInfo == null)
                _infoToLog.AdditionalInfo = new Dictionary<string, object>
                {
                    {STARTEDTIME, beginTime}
                };
            else
                _infoToLog.AdditionalInfo.Add(
                    STARTEDTIME, beginTime);
        }

        public PerfTracker(string name, string userId, string userName, string location, string product, string layer)
        {
            _sw = Stopwatch.StartNew();
            _infoToLog = new LogDetail()
            {
                Message = name,
                UserId = userId,
                UserName = userName,
                Product = product,
                Layer = layer,
                Location = location,
                Hostname = Environment.MachineName
            };

            var beginTime = DateTime.Now;
            _infoToLog.AdditionalInfo = new Dictionary<string, object>()
            {
                {STARTEDTIME, beginTime }
            };
        }

        public PerfTracker(string name, string userId, string userName, string location,
            string product, string layer, Dictionary<string, object> perfParams) :
            this(name, userId, userName, location, product, layer)
        {
            foreach (var item in perfParams)
            {
                _infoToLog.AdditionalInfo.Add("input-" + item.Key, item.Value);
            }
        }

        public void Stop()
        {
            _sw.Stop();

            _infoToLog.ElapsedMilliseconds = _sw.ElapsedMilliseconds;

            var endTime = DateTime.Now;

            if (_infoToLog.AdditionalInfo == null)
            {
                _infoToLog.AdditionalInfo = new Dictionary<string, object>();
            }

            _infoToLog.AdditionalInfo.Add(FINISHEDTIME, endTime.ToString(CultureInfo.InvariantCulture));

            if (_infoToLog.AdditionalInfo.ContainsKey(STARTEDTIME))
            {
                var startTime = (DateTime)_infoToLog.AdditionalInfo.GetValueOrDefault(STARTEDTIME);
                var diffTime = startTime - endTime;
                _infoToLog.AdditionalInfo.Add("DiffTime", diffTime.TotalMinutes.ToString(CultureInfo.InvariantCulture));
            }

            LogDb.WritePerf(_infoToLog);
        }


    }
}
