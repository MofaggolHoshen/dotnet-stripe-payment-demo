using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using DotnetStripePaymentDemo.DTOs.Responses;

namespace DotnetStripePaymentDemo.Middleware
{
    /// <summary>
    /// Global error handling middleware
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            HttpStatusCode httpStatusCode;
            string errorMessage;

            switch (exception)
            {
                case ArgumentNullException:
                    httpStatusCode = HttpStatusCode.BadRequest;
                    errorMessage = "Required parameter is missing";
                    break;

                case ArgumentException:
                    httpStatusCode = HttpStatusCode.BadRequest;
                    errorMessage = exception.Message;
                    break;

                case KeyNotFoundException:
                    httpStatusCode = HttpStatusCode.NotFound;
                    errorMessage = "Resource not found";
                    break;

                case UnauthorizedAccessException:
                    httpStatusCode = HttpStatusCode.Unauthorized;
                    errorMessage = "Unauthorized access";
                    break;

                case InvalidOperationException:
                    httpStatusCode = HttpStatusCode.BadRequest;
                    errorMessage = exception.Message;
                    break;

                default:
                    httpStatusCode = HttpStatusCode.InternalServerError;
                    errorMessage = "An internal server error occurred";
                    break;
            }

            context.Response.StatusCode = (int)httpStatusCode;

            var response = ApiResponse<object>.ErrorResponse(errorMessage);
            var result = JsonSerializer.Serialize(response);

            return context.Response.WriteAsJsonAsync(response);
        }
    }
}
