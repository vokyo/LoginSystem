using System.Text.Json;
using CheckInSystem.Api.Middleware;
using CheckInSystem.Application.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace CheckInSystem.Tests;

public sealed class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ShouldReturnValidationResponse_WhenValidationExceptionIsThrown()
    {
        var (statusCode, contentType, body) = await InvokeMiddlewareAsync(new ValidationException(["Name is required."]));
        using var document = JsonDocument.Parse(body);

        Assert.Equal(400, statusCode);
        Assert.Equal("application/json", contentType);
        Assert.False(document.RootElement.GetProperty("Success").GetBoolean());
        Assert.Equal("One or more validation errors occurred.", document.RootElement.GetProperty("Message").GetString());
        Assert.Equal(400, document.RootElement.GetProperty("Code").GetInt32());
        Assert.Equal("Name is required.", document.RootElement.GetProperty("Data")[0].GetString());
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturnUnauthorizedResponse_WhenUnauthorizedAccessExceptionIsThrown()
    {
        var (statusCode, _, body) = await InvokeMiddlewareAsync(new UnauthorizedAccessException("Invalid token."));
        using var document = JsonDocument.Parse(body);

        Assert.Equal(401, statusCode);
        Assert.Equal("Invalid token.", document.RootElement.GetProperty("Message").GetString());
        Assert.Equal(401, document.RootElement.GetProperty("Code").GetInt32());
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturnNotFoundResponse_WhenKeyNotFoundExceptionIsThrown()
    {
        var (statusCode, _, body) = await InvokeMiddlewareAsync(new KeyNotFoundException("User not found."));
        using var document = JsonDocument.Parse(body);

        Assert.Equal(404, statusCode);
        Assert.Equal("User not found.", document.RootElement.GetProperty("Message").GetString());
        Assert.Equal(404, document.RootElement.GetProperty("Code").GetInt32());
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturnBusinessResponse_WhenBusinessExceptionIsThrown()
    {
        var (statusCode, _, body) = await InvokeMiddlewareAsync(new BusinessException("System roles cannot be deleted."));
        using var document = JsonDocument.Parse(body);

        Assert.Equal(400, statusCode);
        Assert.Equal("System roles cannot be deleted.", document.RootElement.GetProperty("Message").GetString());
        Assert.Equal(400, document.RootElement.GetProperty("Code").GetInt32());
    }

    [Fact]
    public async Task InvokeAsync_ShouldReturnGenericResponse_WhenUnhandledExceptionIsThrown()
    {
        var (statusCode, _, body) = await InvokeMiddlewareAsync(new InvalidOperationException("Boom"));
        using var document = JsonDocument.Parse(body);

        Assert.Equal(500, statusCode);
        Assert.Equal("An unexpected error occurred.", document.RootElement.GetProperty("Message").GetString());
        Assert.Equal(500, document.RootElement.GetProperty("Code").GetInt32());
    }

    private static async Task<(int StatusCode, string ContentType, string Body)> InvokeMiddlewareAsync(Exception exception)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            _ => throw exception,
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);
        context.Response.Body.Position = 0;

        using var reader = new StreamReader(context.Response.Body);
        var body = await reader.ReadToEndAsync();

        return (context.Response.StatusCode, context.Response.ContentType ?? string.Empty, body);
    }
}
