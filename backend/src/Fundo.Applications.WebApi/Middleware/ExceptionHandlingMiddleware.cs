using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fundo.Applications.WebApi.Middleware
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

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleException(context, ex);
            }
        }

        private async Task HandleException(HttpContext context, Exception ex)
        {
            var (statusCode, title, errorCode) = MapException(ex);

            if (statusCode >= 500)
            {
                _logger.LogError(ex, "Unhandled exception");
            }
            else
            {
                _logger.LogWarning(ex, "Request failed: {Message}", ex.Message);
            }

            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

            var problem = new ProblemDetails
            {
                Title = title,
                Status = statusCode,
                Detail = ex.Message,
                Instance = context.Request.Path.ToString()
            };

            problem.Extensions["traceId"] = traceId;
            problem.Extensions["errorCode"] = errorCode;

            var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }

        private static (int statusCode, string title, string errorCode) MapException(Exception ex)
        {
            return ex switch
            {
                ArgumentOutOfRangeException => (StatusCodes.Status400BadRequest, "Invalid request.", "invalid_request"),
                ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request.", "invalid_request"),
                InvalidOperationException => (StatusCodes.Status400BadRequest, "Invalid operation.", "invalid_operation"),
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found.", "not_found"),
                DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Conflict.", "conflict"),
                _ => (StatusCodes.Status500InternalServerError, "Unexpected error.", "unexpected_error")
            };
        }
    }
}
