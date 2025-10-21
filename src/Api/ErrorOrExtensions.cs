using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Error = ErrorOr.Error;

namespace Api;

// Extension methods that convert ErrorOr<T> results into ASP.NET Core IResult responses.
// This allows consistent error handling across the API layer.
public static class ErrorOrExtensions
{
    // Converts an ErrorOr<T> into an HTTP 200 OK or an error response.
    public static IResult MatchToResult<T>(this ErrorOr<T> result, HttpContext httpContext)
    {
        return result.Match(
            Results.Ok,                              // If successful → return 200 OK with value
            errors => MapErrors(httpContext, errors) // If error → map errors to ProblemDetails response
        );
    }

    // Converts an ErrorOr<T> into HTTP 201 Created or an error response.
    public static IResult MatchToResultCreated<T>(
        this ErrorOr<T> result,
        HttpContext httpContext,
        string uri)
    {
        return result.Match(
            value => Results.Created(uri, value),    // Success → 201 Created with resource URI
            errors => MapErrors(httpContext, errors)
        );
    }

    // Converts an ErrorOr<Unit> into HTTP 204 No Content or an error response.
    public static IResult MatchToResultNoContent(
        this ErrorOr<Unit> result,
        HttpContext httpContext)
    {
        return result.Match(
            _ => Results.NoContent(),                // Success → 204 No Content
            errors => MapErrors(httpContext, errors)
        );
    }

    // Private helper method that maps a list of ErrorOr.Errors into HTTP ProblemDetails.
    // This ensures consistent problem responses according to RFC 7807.
    private static IResult MapErrors(HttpContext httpContext, List<Error> errors)
    {
        // The current request path (used as "instance" in ProblemDetails)
        string instance = httpContext.Request.Path;

        // --- VALIDATION ERRORS (400 Bad Request) ---
        if (errors.All(e => e.Type == ErrorType.Validation))
        {
            // Group validation errors by code and collect descriptions
            var errorsDict = errors
                .GroupBy(e => e.Code)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.Description).ToArray()
                );

            // Construct standard ValidationProblemDetails for validation errors
            var problemDetails = new ValidationProblemDetails(errorsDict)
            {
                Type = "https://example.com/errors/validation",
                Title = "Validation error",
                Status = StatusCodes.Status400BadRequest,
                Instance = instance
            };

            return Results.BadRequest(problemDetails);
        }

        // --- NOT FOUND (404) ---
        if (errors.All(e => e.Type == ErrorType.NotFound))
        {
            var problemDetails = new ProblemDetails
            {
                Type = "https://example.com/errors/not-found",
                Title = "Resource not found",
                Status = StatusCodes.Status404NotFound,
                Detail = string.Join("; ", errors.Select(e => e.Description)),
                Instance = instance
            };

            return Results.NotFound(problemDetails);
        }

        // --- CONFLICT (409) ---
        if (errors.All(e => e.Type == ErrorType.Conflict))
        {
            var problemDetails = new ProblemDetails
            {
                Type = "https://example.com/errors/conflict",
                Title = "Conflict",
                Status = StatusCodes.Status409Conflict,
                Detail = string.Join("; ", errors.Select(e => e.Description)),
                Instance = instance
            };

            return Results.Conflict(problemDetails);
        }

        // --- FORBIDDEN (403) ---
        if (errors.All(e => e.Type == ErrorType.Forbidden))
        {
            var problemDetails = new ProblemDetails
            {
                Type = "https://example.com/errors/forbidden",
                Title = "Forbidden",
                Status = StatusCodes.Status403Forbidden,
                Detail = string.Join("; ", errors.Select(e => e.Description)),
                Instance = instance
            };

            return Results.Json(problemDetails, statusCode: StatusCodes.Status403Forbidden);
        }

        // --- GENERAL / UNKNOWN ERROR (default to 400) ---
        var generalProblem = new ProblemDetails
        {
            Type = "https://example.com/errors/bad-request",
            Title = "Request could not be processed",
            Status = StatusCodes.Status400BadRequest,
            Detail = string.Join("; ", errors.Select(e => e.Description)),
            Instance = instance
        };

        return Results.BadRequest(generalProblem);
    }
}
