using System.Net;

namespace ordreChange.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                LogException(context, ex); // Nouveau : Utilisation des journaux enrichis
                await HandleExceptionAsync(context, ex);
            }
        }

        private void LogException(HttpContext context, Exception ex)
        {
            // Enrichir les logs avec des informations sur la requête
            var logDetails = new
            {
                Path = context.Request.Path,
                QueryString = context.Request.QueryString.ToString(),
                Method = context.Request.Method,
                RemoteIpAddress = context.Connection.RemoteIpAddress?.ToString(),
                Exception = new
                {
                    Message = ex.Message,
                    Type = ex.GetType().Name,
                    StackTrace = _env.IsDevelopment() ? ex.StackTrace : null
                }
            };

            // Enregistrement des détails dans le journal
            _logger.LogError(ex, "An unhandled exception occurred: {@LogDetails}", logDetails);
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var statusCode = ex switch
            {
                ArgumentNullException or ArgumentException => (int)HttpStatusCode.BadRequest,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                InvalidOperationException => (int)HttpStatusCode.Forbidden,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                _ => (int)HttpStatusCode.InternalServerError
            };

            var result = new
            {
                error = ex.Message,
                type = ex.GetType().Name,
                stackTrace = _env.IsDevelopment() ? ex.StackTrace : null
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            return context.Response.WriteAsJsonAsync(result);
        }
    }
}
