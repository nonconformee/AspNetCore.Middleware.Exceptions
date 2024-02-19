using System.Buffers;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Builder;



namespace AspNetCore.Middleware.Exceptions;



public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ExceptionHandlingConfiguration config, ILogger<ExceptionHandlingMiddleware> logger, IServiceProvider services)
    {
        var cont = config.ContinueAfterHandling;

        try
        {
            await _next(context);
        }
        catch(Exception e)
        {
            // TODO: replace
            if(config.ExceptionType is not null && !e.GetType().IsAssignableTo(config.ExceptionType))
            {
                cont = false;
                return;
            }

            // TODO: check value of cont
            if(config.ExceptionFilter is not null)
            {
                if(!config.ExceptionFilter(e, context, config, services))
                {
                    cont = false;
                    return;
                }
            }

            if(config.UseILogger)
            {
                var logMessage = new StringBuilder();

                if (config.IncludeMessageInLog)
                {
                    logMessage.Append(e.Message);

                    if(config.IncludeDetailsInLog)
                    {
                        logMessage.AppendLine();
                        logMessage.AppendLine();
                    }
                }

                if(config.IncludeDetailsInLog)
                {
                    logMessage.Append(e.ToString());
                }

                if(logMessage.Length > 0)
                {
                    logger.Log(config.LogLevel, logMessage.ToString(), e);
                }
            }

            if(config.ExceptionLogger is not null)
            {
                var messageFromDelegate = config.ExceptionLogger(e, context, config, services);

                if(!string.IsNullOrWhiteSpace(messageFromDelegate) && config.UseILogger)
                {
                    logger.Log(config.LogLevel, messageFromDelegate, e);
                }
            }

            if(config.ResponseGenerator is not null)
            {
                cont = config.ResponseGenerator(e, context, config, services);
            }
            else
            {
                context.Response.StatusCode = config.ResponseStatusCode;

                await context.Response.WriteAsJsonAsync(new ExceptionBody(
                    config.IncludeMessageInResponse ? e.Message : null, config.IncludeDetailsInResponse ? e.ToString() : null));
            }
        }
        finally
        {
            if(cont)
            {
                await _next(context);
            }
        }
    }
}



public static class ExceptionHandlingMiddlewareExtensions
{
    public static WebApplicationBuilder AddExceptionHandling(this WebApplicationBuilder builder, Action<ExceptionHandlingConfiguration>? configBuilder = null)
    {
        var config = new ExceptionHandlingConfiguration();
        configBuilder?.Invoke(config);
        builder.Services.AddSingleton(config);
        return builder;
    }

    public static WebApplication UseExceptionHandling(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        return app;
    }
}



public sealed record ExceptionHandlingConfiguration(
    Type? ExceptionType = null, // will be replaced
    bool UseILogger = true,
    LogLevel LogLevel = LogLevel.Error,
    bool IncludeMessageInLog = true,
    bool IncludeDetailsInLog = true,
    int ResponseStatusCode = StatusCodes.Status500InternalServerError,
    bool IncludeMessageInResponse = false,
    bool IncludeDetailsInResponse = false,
    bool ContinueAfterHandling = false,
    Func<Exception, HttpContext, ExceptionHandlingConfiguration, IServiceProvider, bool>? ExceptionFilter = null,
    Func<Exception, HttpContext, ExceptionHandlingConfiguration, IServiceProvider, string?>? ExceptionLogger = null,
    Func<Exception, HttpContext, ExceptionHandlingConfiguration, IServiceProvider, bool>? ResponseGenerator = null);



internal sealed record ExceptionBody(
    string? Message,
    string? Details);