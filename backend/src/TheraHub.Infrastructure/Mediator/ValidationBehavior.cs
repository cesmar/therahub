using FluentValidation;
using TheraHub.Application.Abstractions.Mediator;

namespace TheraHub.Infrastructure.Mediator;

public class ValidationBehavior<TRequest, TResult> : IPipelineBehavior<TRequest, TResult>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<Result<TResult>> HandleAsync(
        TRequest request,
        Func<Task<Result<TResult>>> next,
        CancellationToken cancellationToken = default)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(error => error is not null)
            .ToList();

        if (failures.Count != 0)
        {
            var errors = string.Join(", ", failures.Select(f => f.ErrorMessage));
            return Result<TResult>.Failure(errors);
        }

        return await next();
    }
}
