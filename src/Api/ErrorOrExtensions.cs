using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Error = ErrorOr.Error;

namespace Api;

public static class ErrorOrExtensions
{
    public static IResult MatchToResult<T>(this ErrorOr<T> result, HttpContext httpContext)
    {
        return result.Match(
            Results.Ok,
            errors => MapErrors(httpContext, errors)
        );
    }

    public static IResult MatchToResultCreated<T>(this ErrorOr<T> result, HttpContext httpContext, string uri)
    {
        return result.Match(
            value => Results.Created(uri, value),
            errors => MapErrors(httpContext, errors)
        );
    }

    public static IResult MatchToResultNoContent(this ErrorOr<Unit> result, HttpContext httpContext)
    {
        return result.Match(
            _ => Results.NoContent(),
            errors => MapErrors(httpContext, errors)
        );
    }

    private static IResult MapErrors(HttpContext httpContext, List<Error> errors)
    {
        string instance = httpContext.Request.Path;

        if (errors.All(e => e.Type == ErrorType.Validation))
        {
            var errorsDict = errors
                .GroupBy(e => e.Code)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.Description).ToArray()
                );

            var problemDetails = new ValidationProblemDetails(errorsDict)
            {
                Type = "https://example.com/errors/validation",
                Title = "Validation error",
                Status = StatusCodes.Status400BadRequest,
                Instance = instance
            };

            return Results.BadRequest(problemDetails);
        }

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