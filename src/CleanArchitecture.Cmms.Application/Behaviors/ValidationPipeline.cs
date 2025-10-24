using FluentValidation;

namespace CleanArchitecture.Cmms.Application.Behaviors
{
    internal sealed class ValidationPipeline<TRequest, TResult>
    : IPipeline<TRequest, TResult> where TRequest : notnull
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationPipeline(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResult> Handle(TRequest request, PipelineDelegate<TResult> next, CancellationToken cancellationToken = default)
        {
            if (!_validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);

            var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken))))
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
                throw new ValidationException(string.Join("; ", failures.Select(f => f.ErrorMessage)));

            return await next();
        }
    }

}
