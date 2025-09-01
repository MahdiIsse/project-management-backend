using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using ProjectManagement.Infrastructure.Exceptions;

namespace ProjectManagement.Api.Middleware;
public class ExceptionMiddleware
{
  private readonly RequestDelegate _next;
  private readonly ILogger<ExceptionMiddleware> _logger;
  private readonly IHostEnvironment _environment;

  public ExceptionMiddleware(
    RequestDelegate next,
    ILogger<ExceptionMiddleware> logger,
    IHostEnvironment environment)
  {
    _next = next;
    _logger = logger;
    _environment = environment;
  }

  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (Exception exception)
    {
      await HandleExceptionAsync(context, exception);
    }
  }

  private async Task HandleExceptionAsync(HttpContext context, Exception exception)
  {
    var (statusCode, message, details) = GetExceptionDetails(exception);

    _logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

    var errorResponse = new
    {
      success = false,
      error = new
      {
        message = message,
        statusCode = statusCode,
        timestamp = DateTime.UtcNow,
        path = context.Request.Path.Value,
        method = context.Request.Method,
        details = _environment.IsDevelopment() ? details : null
      }
    };

    context.Response.ContentType = "application/json";
    context.Response.StatusCode = statusCode;

    var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
    {
      WriteIndented = _environment.IsDevelopment()
    });

    await context.Response.WriteAsync(jsonResponse);
  }

  private (int statusCode, string message, string details) GetExceptionDetails(Exception exception)
  {
    return exception switch
    {
      NotFoundException notFound => (
        (int)HttpStatusCode.NotFound,
        notFound.Message,
        "The requested resource was not found"
      ),

      ForbiddenException forbidden => (
        (int)HttpStatusCode.Forbidden,
        forbidden.Message,
        "Access to the requested resource is forbidden"
      ),

      UnauthorizedException unauthorized => (
        (int)HttpStatusCode.Unauthorized,
        unauthorized.Message,
        "Authentication is required to access this resource"
      ),

      ConflictException conflict => (
        (int)HttpStatusCode.Conflict,
        conflict.Message,
        "There was a conflict with the current state of the resource"
      ),

      BusinessValidationException validation => (
        (int)HttpStatusCode.BadRequest,
        validation.Message,
        "Business validation failed"
      ),

      ArgumentException argument => (
        (int)HttpStatusCode.BadRequest,
        argument.Message,
        "Invalid argument provided"
      ),

      KeyNotFoundException keyNotFound => (
        (int)HttpStatusCode.NotFound,
        "The requested resource was not found",
        keyNotFound.Message
      ),

      InvalidOperationException invalidOp => (
        (int)HttpStatusCode.BadRequest,
        "The operation is not valid in the current state",
        invalidOp.Message
      ),

      DbUpdateConcurrencyException concurrency => (
        (int)HttpStatusCode.Conflict,
        "The data has been modified by another user. Please refresh and try again.",
        "Concurrency conflict detected"
      ),

      DbUpdateException dbUpdate => (
        (int)HttpStatusCode.BadRequest,
        "A database update error occurred. Please check your data and try again.",
        _environment.IsDevelopment() ? dbUpdate.Message : "Database constraint violation"
      ),

      SqlException sqlEx => (
        (int)HttpStatusCode.InternalServerError,
        "A database error occurred. Please try again later.",
        _environment.IsDevelopment() ? sqlEx.Message : "Database operation failed"
      ),

      TimeoutException timeout => (
        (int)HttpStatusCode.RequestTimeout,
        "The operation timed out. Please try again.",
        "Database operation timeout"
      ),

      _ => (
        (int)HttpStatusCode.InternalServerError,
        "An unexpected error occurred. Please try again later.",
        _environment.IsDevelopment() ? exception.Message : "Internal server error"
      )
    };
  }
}
