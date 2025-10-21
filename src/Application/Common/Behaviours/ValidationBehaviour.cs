using ErrorOr;
using FluentValidation;
using MediatR;

namespace Application.Common.Behaviours;

// A MediatR pipeline behavior that runs validation before executing the handler
public class ValidationBehaviour<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IErrorOr
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // If there are no validators for this request type, just continue to the next handler
        if (!_validators.Any())
            return await next(cancellationToken);

        // Run all validators asynchronously for the given request
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(request, cancellationToken))
        );

        // Collect all validation failures (errors) from all validators
        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        // If no validation errors found, continue with the next handler in the pipeline
        if (!failures.Any())
            return await next(cancellationToken);

        // Convert FluentValidation errors into ErrorOr's validation errors
        var errors = failures
            .Select(f => Error.Validation(f.PropertyName, f.ErrorMessage))
            .ToList();

        // Return validation errors instead of calling the handler
        // (dynamic cast needed because TResponse can vary)
        return (TResponse)(dynamic)errors;
    }
}
