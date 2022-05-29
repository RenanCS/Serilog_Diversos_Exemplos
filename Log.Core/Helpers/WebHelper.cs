using Log.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Log.Core.Helpers
{
    public static class WebHelper
    {
        public static void LogWebUsage(string product, string layer, string activityName,
            HttpContext context, Dictionary<string, object> additionalInfo = null)
        {
            var details = GetWebLogDetail(product, layer, activityName, context, additionalInfo);
            LogDb.WriteUsage(details);
        }

        public static void LogWebDiagnostic(string product, string layer, string message,
            HttpContext context, Dictionary<string, object> diagnosticInfo = null)
        {
            var details = GetWebLogDetail(product, layer, message, context, diagnosticInfo);
            LogDb.WriteDiagnostic(details);
        }

        public static void LogWebError(string product, string layer, Exception ex,
            HttpContext context)
        {
            var details = GetWebLogDetail(product, layer, null, context);
            details.Exception = ex;

            LogDb.WriteError(details);
        }

        public static LogDetail GetWebLogDetail(string product, string layer,
            string activityName, HttpContext context,
            Dictionary<string, object> additionalInfo = null)
        {
            var detail = new LogDetail
            {
                Product = product,
                Layer = layer,
                Message = activityName,
                Hostname = Environment.MachineName,
                CorrelationId = Activity.Current?.Id ?? context.TraceIdentifier,
                AdditionalInfo = additionalInfo ?? new Dictionary<string, object>()
            };

            GetUserData(detail, context);
            GetRequestData(detail, context);
            // Session data??
            // Cookie data??

            return detail;
        }

        private static void GetRequestData(LogDetail detail, HttpContext context)
        {
            var request = context.Request;
            if (request == null) return;
            detail.Location = request.Path;

            detail.AdditionalInfo.Add("UserAgent", request.Headers["User-Agent"]);
            // non en-US preferences here??
            detail.AdditionalInfo.Add("Languages", request.Headers["Accept-Language"]);

            var qdict = QueryHelpers.ParseQuery(request.QueryString.ToString());
            foreach (var key in qdict.Keys) detail.AdditionalInfo.Add($"QueryString-{key}", qdict[key]);
        }

        private static void GetUserData(LogDetail detail, HttpContext context)
        {
            var userId = "";
            //var userName = "";
            var givenName = "";
            var familyName = "";


            var user = context.User; // ClaimsPrincipal.Current is not sufficient
            if (user != null)
            {
                var i = 1; // i included in dictionary key to ensure uniqueness
                foreach (var claim in user.Claims)
                    switch (claim.Type)
                    {
                        case "sub":
                            userId = claim.Value;
                            break;
                        case "given_name":
                            givenName = claim.Value;
                            break;
                        case "family_name":
                            familyName = claim.Value;
                            break;
                        // example dictionary key: UserClaim-4-role 
                        default:
                            detail.AdditionalInfo.Add($"UserClaim-{i++}-{claim.Type}", claim.Value);
                            break;
                    }
            }

            detail.UserId = userId;
            detail.UserName = $"{givenName} {familyName}";
        }
    }
}
