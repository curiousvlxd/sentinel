using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Sentinel.ServiceDefaults.Extensions;

public static class ServiceDefaultsExtensions
{
    public static WebApplicationBuilder AddServiceDefaults(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks();
        builder.Services.AddProblemDetails();
        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });
        return builder;
    }

    public static void ConfigureJsonOptions(Microsoft.AspNetCore.Mvc.JsonOptions options)
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    }

    public static IApplicationBuilder UseExceptionHandlerDefaults(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var feature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = feature?.Error;

                if (exception is null)
                    return;

                var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("Sentinel.ExceptionHandler");
                logger.LogUnhandledException(exception);

                var problemDetailsService = context.RequestServices.GetRequiredService<IProblemDetailsService>();
                await problemDetailsService.WriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context,
                    Exception = exception,
                    ProblemDetails =
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Title = "Server Error",
                        Detail = exception.Message
                    }
                });
            });
        });
        return app;
    }

    public static void MapDefaultEndpoints(this WebApplication app) => app.MapHealthChecks("/health");
}

internal static partial class ExceptionHandlerLogging
{
    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception")]
    public static partial void LogUnhandledException(this ILogger logger, Exception exception);
}
