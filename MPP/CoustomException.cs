using Newtonsoft.Json;
using System.Net;

namespace MPP
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Log the exception here
            System.Diagnostics.Debug.WriteLine(exception);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new { error = exception.Message };
            var payload = JsonConvert.SerializeObject(response);
            return context.Response.WriteAsync(payload);
        }
    }
}
