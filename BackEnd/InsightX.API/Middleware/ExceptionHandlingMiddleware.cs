using System.Net;
using System.Text.Json;

namespace InsightX.API.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
                var correlationId = Guid.NewGuid().ToString("N")[..12];

                _logger.LogError(ex,
                    "Unhandled exception [CorrelationId={CorrelationId}] while processing {Method} {Path}{Query}",
                    correlationId,
                    context.Request.Method,
                    context.Request.Path,
                    context.Request.QueryString);

                await HandleExceptionAsync(context, ex, correlationId);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception, string correlationId)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, message) = exception switch
            {
                ArgumentException or ArgumentNullException
                    => ((int)HttpStatusCode.BadRequest, "Invalid request data. Please check your input and try again."),
                UnauthorizedAccessException
                    => ((int)HttpStatusCode.Unauthorized, "You are not authorized to perform this action."),
                KeyNotFoundException
                    => ((int)HttpStatusCode.NotFound, "The requested resource was not found."),
                InvalidOperationException
                    => ((int)HttpStatusCode.Conflict, "The operation could not be completed due to a conflict."),
                _
                    => ((int)HttpStatusCode.InternalServerError, "An unexpected error occurred. Please try again later.")
            };

            context.Response.StatusCode = statusCode;

            var response = new
            {
                StatusCode = statusCode,
                Error = message,
                CorrelationId = correlationId
            };

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
        }
    }
}

