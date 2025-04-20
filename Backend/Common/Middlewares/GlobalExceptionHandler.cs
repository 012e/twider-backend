using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Net;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetailsService;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IProblemDetailsService problemDetailsService)
    {
        _logger = logger;
        _problemDetailsService = problemDetailsService;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception");

        var statusCode = HttpStatusCode.InternalServerError;
        var title = "An unexpected error occurred.";
        var details = exception.Message;
        var type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"; // Default Internal Server Error type

        switch (exception)
        {
            case ValidationException validationException
                : // Assuming ValidationException is a custom exception or from a library like FluentValidation
                statusCode = HttpStatusCode.BadRequest;
                title = "Validation Failed";
                details = validationException.Message;
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"; // Bad Request type
                // You might want to add specific validation errors to ProblemDetails.Extensions here
                break;

            case ArgumentException argumentException:
                statusCode = HttpStatusCode.BadRequest;
                title = "Invalid Argument";
                details = argumentException.Message;
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"; // Bad Request type
                break;

            case KeyNotFoundException keyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                title = "Resource Not Found";
                details = keyNotFoundException.Message;
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"; // Not Found type
                break;

            case BadHttpRequestException badHttpRequestException:
                title = "Bad Request";
                statusCode = HttpStatusCode.BadRequest;
                details = badHttpRequestException.Message;
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"; // Bad Request type
                break;

            // Add more custom exception cases as needed
            // case SpecificCustomException customException:
            //     statusCode = HttpStatusCode.SpecificCode;
            //     title = "Custom Error Title";
            //     details = customException.Message;
            //     type = "your-custom-error-type-uri";
            //     // Add custom properties to problemDetails.Extensions if needed
            //     break;
        }

        // Set the response status code
        httpContext.Response.StatusCode = (int)statusCode;

        // Create the ProblemDetails object
        var problemDetails = new ProblemDetails
        {
            Title = title,
            Status = (int)statusCode,
            Detail = details,
            Instance = httpContext.Request.Path,
            Type = type // Set the type URI
        };

        // You can add additional information to the extensions
        // if (!httpContext.Response.Headers.ContainsKey("traceId"))
        // {
        //     problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        // }


        await _problemDetailsService.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails
        });

        return true;
    }
}