using System.Buffers;

namespace AspNetCore.Middleware.Exceptions;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate _next)
    {
        this._next = _next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch(Exception e)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.WriteAsJsonAsync<ExceptionBody>(new ExceptionBody(e.Message, e.ToString()));
        }
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}

public sealed record ExceptionBody(
    string? Message,
    string? Details);

public sealed record ExceptionHandlingConfiguration(
    Type? ExceptionType = null,
    bool LogException = true,
    LogLevel LogLevel = LogLevel.Error,
    string? LogFormatString = null,
    IEnumerable<object>? LogFormatParams = null,
    string? StatusCode = null,
    object? ResponseBody = null,
    bool IncludeMessage = false,
    bool IncludeDetails = false,
    bool Continue = false,
    Func<Exception, HttpContext, bool>? ExceptionFilter = null,
    Func<Exception, HttpContext, bool>? ExceptionLogger = null,
    Func<Exception, HttpContext, bool>? ResponseGenerator = null);