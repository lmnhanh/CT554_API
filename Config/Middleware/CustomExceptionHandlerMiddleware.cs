using CT554_API.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CT554_API.Config.Middleware
{

    public class CustomExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        public CustomExceptionHandlerMiddleware(RequestDelegate next)
        {
            //_logger = logger;
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ILogger<CustomExceptionHandlerMiddleware> _logger)
        {
            try
            {
                //Next to the remaining middleware
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, _logger);
            }
        }
        private async Task HandleExceptionAsync(HttpContext context, Exception exception, ILogger<CustomExceptionHandlerMiddleware> _logger)
        {
            ErrorResponse messageToReponses = exception switch
            {
                SqlException => new ErrorResponse() { errors = new (){ "Some unexpected SQL errors has occured" } },
                DbUpdateException => new ErrorResponse() { errors = new() { "There are some errors when update the database" } },
                _ => new ErrorResponse() { errors = new() { "Internal server error" } },
            };

            //_logger.Write(exception.Message, LogLevel.Error);
            _logger.LogError(exception.InnerException?.Message ?? exception.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = messageToReponses.StatusCode;
            await context.Response.WriteAsync(messageToReponses.ToString());
        }
    }
}
