using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bookify.Middleware
{
    /// <summary>
    /// Global exception handler middleware.
    /// Catches all unhandled exceptions, logs them, and returns a consistent
    /// RFC 7807 ProblemDetails-style JSON response — never leaking stack traces.
    /// </summary>
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionHandlingMiddleware(RequestDelegate next,ILogger<ExceptionHandlingMiddleware> logger,IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception for {Method} {Path}",
                    context.Request.Method, context.Request.Path);

                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, title) = exception switch
            {
                ArgumentNullException or ArgumentException
                    => (HttpStatusCode.BadRequest, "Invalid request data."),
                KeyNotFoundException
                    => (HttpStatusCode.NotFound, "Resource not found."),
                UnauthorizedAccessException
                    => (HttpStatusCode.Unauthorized, "Unauthorized."),
                InvalidOperationException
                    => (HttpStatusCode.UnprocessableEntity, "Operation not allowed."),
                _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
            };

            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                status = (int)statusCode,
                title,
                // بنعرض تفاصيل الخطأ في الـ Development فقط للأمان
                detail = _env.IsDevelopment() ? exception.Message : null,
                traceId = context.TraceIdentifier
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

           
            await context.Response.WriteAsJsonAsync(response, options);
        }
    }

    // Extension method for clean registration in Program.cs
    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
            => app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}