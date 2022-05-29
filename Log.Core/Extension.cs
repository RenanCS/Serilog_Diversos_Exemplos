using Log.Core.Helpers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace Log.Core
{
    public static class Extension
    {
        public static IConfiguration StaticConfig { get; private set; }

        public static IServiceCollection AddLogCore(this IServiceCollection services,
            IConfiguration configuration)
        {
            StaticConfig = configuration;
            return services;
        }

        public static IApplicationBuilder UseLogCore(this IApplicationBuilder app, IConfiguration configuration)
        {
            app.UseExceptionHandler(eApp =>
            {
                eApp.Run(async context =>
                {
                    context.Response.StatusCode = (int)configuration.GetValue("LogCore:ErrorCode", HttpStatusCode.InternalServerError);

                    context.Response.ContentType = configuration.GetValue("LogCore:ContentType", "application/json");

                    var errorCtx = context.Features.Get<IExceptionHandlerFeature>();
                    if (errorCtx != null)
                    {
                        var ex = errorCtx.Error;

                        WebHelper.LogWebError(configuration.GetValue("LogCore:Product", "Default API Services"),
                            configuration.GetValue("LogCore:Layer", "Default API"), ex, context);


                        var errorId = Activity.Current?.Id ?? context.TraceIdentifier;
                        var jsonResponse = JsonConvert.SerializeObject(new CustomErrorResponse
                        {
                            ErrorId = errorId,
                            Message = configuration.GetValue("LogCore:Display",
                                "Ops! algo de errado não está certo.")
                        });

                        await context.Response.WriteAsync(jsonResponse, Encoding.UTF8);
                    }
                });
            });

            return app;
        }
    }
}
