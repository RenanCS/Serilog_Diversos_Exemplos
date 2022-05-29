using Log.Core.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Log.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    //[TrackUsage("SeriLog Exemplos", "Api", "WeatherForecast")]
    public class WeatherForecastController : ControllerBase
    {
        private static string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet("RegisterDiagnostic")]
        public void RegisterDiagnostic()
        {
            WebHelper.LogWebDiagnostic("SeriLog Exemplos", "Api", "RegisterDiagnostic-Get ...", HttpContext,
                 new Dictionary<string, object>
                 {
                    {"Information", "Important information here"}
                 });

        }

        [HttpGet("RegisterObjectReferenceNotSetToAnInstanceOfAnObject")]
        public void RegisterObjectReferenceNotSetToAnInstanceOfAnObject()
        {

            Summaries = null;

            var temperature = Summaries[0];
        }

        [HttpGet("RegisterExceptionWithTextInformation")]
        public void RegisterExceptionWithTextInformation()
        {
            throw new Exception("Register additional information with exception");
        }

        [HttpPost("RegisterNullableObjectMustHaveAValue")]
        public void RegisterNullableObjectMustHaveAValue(WeatherForecast weather)
        {
            weather.Date = null;

            var year = weather.Date.Value.Year;
        }
    }
}
