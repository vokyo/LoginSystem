using System.Net;
using System.Text.Json;
using CheckInSystem.Application.Common.Exceptions;
using CheckInSystem.Application.Common.Models;

namespace CheckInSystem.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled exception caught by middleware.");
            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException validationException => ((int)HttpStatusCode.BadRequest, ApiResponse.Fail(validationException.Message, 400, validationException.Errors)),
            UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, ApiResponse.Fail(exception.Message, 401)),
            KeyNotFoundException => ((int)HttpStatusCode.NotFound, ApiResponse.Fail(exception.Message, 404)),
            BusinessException => ((int)HttpStatusCode.BadRequest, ApiResponse.Fail(exception.Message, 400)),
            _ => ((int)HttpStatusCode.InternalServerError, ApiResponse.Fail("An unexpected error occurred.", 500))
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
