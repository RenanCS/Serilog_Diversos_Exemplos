using Log.Core.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Text.Json;

namespace Log.Core.Filters
{
    public class TrackPerformanceFilter : IActionFilter
    {
        private readonly string _layer;
        private readonly string _product;
        private PerfTracker _tracker;

        public TrackPerformanceFilter(string product, string layer)
        {
            _product = product;
            _layer = layer;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;
            var activity = $"{request.Path}-{request.Method}";

            var dict = new Dictionary<string, object>();
            if (context.RouteData.Values?.Keys != null)
            {
                foreach (var key in context.RouteData.Values?.Keys)
                {
                    dict.Add($"RouteData-{key}", (string)context.RouteData.Values[key]);

                }
            }

            if (context.ActionArguments.Count != 0)
            {
                var result = context.Result;

                if (result is ObjectResult response)
                {
                    var data = JsonSerializer.Serialize(response.Value);
                }

                foreach (var key in context.ActionArguments.Values)
                {
                    var inputModelName = key.GetType().FullName;

                    foreach (var propertyValue in key.GetType().GetProperties())
                    {
                        string propName = propertyValue.Name;
                        string postedValue = key.GetType().GetProperty(propName).GetValue(key, null).ToString();
                        dict.Add($"InputModel-{inputModelName}-{propName}", postedValue);
                    }

                }
            }

            var details = WebHelper.GetWebLogDetail(_product, _layer, activity,
                context.HttpContext, dict);

            _tracker = new PerfTracker(details);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _tracker?.Stop();
        }
    }
}
