using Amazon.CognitoIdentityProvider.Model;

namespace Subscription.Security.Middlewares;

public class CustomExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CustomExceptionMiddleware> _logger;

    public CustomExceptionMiddleware(RequestDelegate next, ILogger<CustomExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro capturado pelo middleware.");
            await HandleExceptionAsync(context, ex.InnerException ?? ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = GetStatusCode(exception);

        var response = new
        {
            exception.Message,
            Status = context.Response.StatusCode
        };

        return context.Response.WriteAsJsonAsync(response);
    }

    private static int GetStatusCode(Exception exception)
    {
        switch (exception)
        {
            case ArgumentException:
                return 400;
            case KeyNotFoundException:
                return 404;
            case UnauthorizedAccessException:
                return 401;
            case NotAuthorizedException:
                return 400;
            case CodeMismatchException:
                return 400;
            default:
                return 500;
        }
    }
}
