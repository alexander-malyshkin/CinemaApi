using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace CinemaApp.Api
{
    public class WebRequestLoggingMiddleware
    {
        private readonly RequestDelegate _requestDelegate;
        private readonly ILogger<WebRequestLoggingMiddleware> _logger;

        public WebRequestLoggingMiddleware(RequestDelegate requestDelegate, ILogger<WebRequestLoggingMiddleware> logger)
        {
            _requestDelegate = requestDelegate;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                await _requestDelegate.Invoke(context);
            }
            finally
            {
                sw.Stop();
                _logger.LogInformation("Request {method} at {url} took {elapsedMilliseconds}ms and resulted in status code {statusCode}", 
                    context.Request.Method, context.Request.Path, sw.ElapsedMilliseconds, context.Response.StatusCode);
            }
        }
    }
}
