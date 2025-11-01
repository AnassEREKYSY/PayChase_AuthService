using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace API.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            var problem = BuildProblem(ctx, ex);
            ctx.Response.ContentType = "application/json";
            ctx.Response.StatusCode = problem.Status ?? (int)HttpStatusCode.InternalServerError;
            var json = JsonSerializer.Serialize(problem);
            await ctx.Response.WriteAsync(json);
        }
    }

    private static ProblemDetails BuildProblem(HttpContext ctx, Exception ex)
    {
        var status = (int)HttpStatusCode.InternalServerError;
        var code = "error.unexpected";
        var message = "An unexpected error occurred.";

        if (ex is UnauthorizedAccessException)
        {
            status = (int)HttpStatusCode.Unauthorized;
            code = "auth.invalid_credentials";
            message = ex.Message;
        }
        else if (ex is InvalidOperationException)
        {
            status = (int)HttpStatusCode.BadRequest;
            code = "validation.error";
            message = ex.Message;
        }

        return new ProblemDetails
        {
            Status = status,
            Title = message,
            Type = code,
            Detail = ctx.TraceIdentifier,
            Instance = ctx.Request.Path
        };
    }

    private class ProblemDetails
    {
        public int? Status { get; set; }
        public string? Title { get; set; }
        public string? Type { get; set; }
        public string? Detail { get; set; }
        public string? Instance { get; set; }
    }
}
