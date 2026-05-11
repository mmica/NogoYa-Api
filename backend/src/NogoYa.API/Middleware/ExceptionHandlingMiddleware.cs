using System.Net;
using System.Text.Json;
using NogoYa.Domain.Exceptions;

namespace NogoYa.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(
        RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
    { _next = next; _logger = logger; _env = env; }

    public async Task InvokeAsync(HttpContext context)
    {
        try { await _next(context); }
        catch (Exception ex) { await HandleAsync(context, ex); }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        context.Response.ContentType = "application/problem+json";
        var traceId = context.TraceIdentifier;

        var (status, title, errors) = ex switch
        {
            NotFoundException nf => (HttpStatusCode.NotFound, nf.Message, (object?)null),
            BusinessRuleException br => (HttpStatusCode.UnprocessableEntity, br.Message, null),
            Domain.Exceptions.ValidationException ve => (HttpStatusCode.BadRequest, ve.Message, (object?)ve.Errors),
            FluentValidation.ValidationException fv => (HttpStatusCode.BadRequest, "Error de validación.",
                (object?)fv.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray())),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "No autorizado.", null),
            _ => (HttpStatusCode.InternalServerError, "Ocurrió un error inesperado.", null)
        };

        if (status == HttpStatusCode.InternalServerError)
            _logger.LogError(ex, "Unhandled exception. TraceId={TraceId}", traceId);
        else
            _logger.LogWarning(ex, "Handled exception: {Message}. TraceId={TraceId}", ex.Message, traceId);

        context.Response.StatusCode = (int)status;
        var problem = new
        {
            type = $"https://httpstatuses.com/{(int)status}",
            title, status = (int)status, traceId, errors,
            detail = _env.IsDevelopment() ? ex.ToString() : null
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
    }
}
