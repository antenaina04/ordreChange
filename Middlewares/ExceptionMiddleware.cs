using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NLog;
using System.Data;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security;

namespace ordreChange.Middlewares
{
    /// <summary>
    /// Middleware pour gérer les exceptions globales dans l'application.
    /// Capture les exceptions non gérées et renvoie une réponse JSON avec le statut HTTP approprié.
    /// </summary
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private static readonly NLog.ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initialise une nouvelle instance du middleware d'exception.
        /// </summary>
        /// <param name="next">Délégué de requête pour le prochain middleware dans la chaîne.</param>
        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Intercepte les requêtes HTTP et capture les exceptions non gérées.
        /// </summary>
        /// <param name="context">Contexte HTTP de la requête en cours.</param>
        /// <returns>Tâche asynchrone représentant l'opération.</returns>
        ///  InvokeAsync est automatiquement utilisé par ASP.NET Core pour chaque requête HTTP lorsque le middleware est enregistré dans le pipeline(Program.cs).
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // Proceed to the next middleware
            }
            catch (Exception ex)
            {
                Logger.Error($"Exception: {ex.Message}, StackTrace: {ex.StackTrace}");
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        /// Gère les exceptions capturées et renvoie une réponse JSON avec le statut HTTP approprié.
        /// </summary>
        /// <param name="context">Contexte HTTP de la requête en cours.</param>
        /// <param name="exception">Exception capturée.</param>
        /// <returns>Tâche asynchrone représentant l'opération.</returns>
        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            // Assigner le statut(HTTP status code) selon l'exception (ex: 401, 400, 404, 409, ...)
            var Status = exception switch
            {
                ArgumentNullException => (int)HttpStatusCode.BadRequest,
                ArgumentException => (int)HttpStatusCode.BadRequest,
                InvalidOperationException => (int)HttpStatusCode.Conflict,
                NullReferenceException => (int)HttpStatusCode.InternalServerError,
                IndexOutOfRangeException => (int)HttpStatusCode.BadRequest,
                FileNotFoundException => (int)HttpStatusCode.NotFound,
                NotImplementedException => (int)HttpStatusCode.NotImplemented,
                TimeoutException => (int)HttpStatusCode.RequestTimeout,
                UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
                KeyNotFoundException => (int)HttpStatusCode.NotFound,
                FormatException => (int)HttpStatusCode.BadRequest,
                OverflowException => (int)HttpStatusCode.BadRequest,
                DivideByZeroException => (int)HttpStatusCode.BadRequest,
                OperationCanceledException => (int)HttpStatusCode.RequestTimeout,
                StackOverflowException => (int)HttpStatusCode.InternalServerError,
                OutOfMemoryException => (int)HttpStatusCode.InternalServerError,
                IOException => (int)HttpStatusCode.InternalServerError,
                SecurityException => (int)HttpStatusCode.Forbidden,
                SerializationException => (int)HttpStatusCode.InternalServerError,
                AggregateException => (int)HttpStatusCode.InternalServerError,
                ApplicationException => (int)HttpStatusCode.InternalServerError,
                COMException => (int)HttpStatusCode.InternalServerError,
                DataException => (int)HttpStatusCode.InternalServerError,
                DbUpdateException => (int)HttpStatusCode.InternalServerError,
                InvalidCastException => (int)HttpStatusCode.BadRequest,
                InvalidProgramException => (int)HttpStatusCode.InternalServerError,
                MemberAccessException => (int)HttpStatusCode.InternalServerError,
                RankException => (int)HttpStatusCode.BadRequest,
                SynchronizationLockException => (int)HttpStatusCode.InternalServerError,
                TypeInitializationException => (int)HttpStatusCode.InternalServerError,
                _ => (int)HttpStatusCode.InternalServerError
            };

            // Formatter la réponse HTTP après qu'une exception a été levée (format JSON)
            var response = new
            {
                Status,
                exception.Message
            };

            return context.Response.WriteAsync(JsonConvert.SerializeObject(response));
        }
    }
}
