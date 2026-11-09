using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using TaxStream.Core.Exceptions;

namespace TaxStream.WebAPI.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            NotFoundException nfe => (HttpStatusCode.NotFound, "Not Found", nfe.Message),
            AuthorizationException ae => (HttpStatusCode.NotFound, "Not Found", "The requested resource was not found."), // 404 to prevent enumeration
            ConflictException ce => (HttpStatusCode.Conflict, "Conflict", ce.Message),
            DomainException de => (HttpStatusCode.BadRequest, "Bad Request", de.Message),
            OperationCanceledException => (HttpStatusCode.ServiceUnavailable, "Service Unavailable", "The request was cancelled or timed out."),
            _ => (HttpStatusCode.InternalServerError, "Internal Server Error", "An unexpected error occurred.")
        };

        if (statusCode == HttpStatusCode.InternalServerError)
        {
            // Log full details only server-side; never expose to client
            _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        }
        else
        {
            _logger.LogWarning("Handled exception [{Type}]: {Message}", exception.GetType().Name, exception.Message);
        }

        var problem = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
