using CleanArchitecture.Cmms.Application.Abstractions.Messaging;
using FluentValidation;
using MediatR;

namespace CleanArchitecture.Cmms.Application.Behaviors
{
    internal sealed class ValidationCommandPipeline<TCommand, TResult>
    : ICommandPipeline<TCommand, TResult>
    where TCommand : ICommand<TResult>
    {
        private readonly IEnumerable<IValidator<TCommand>> _validators;

        public ValidationCommandPipeline(IEnumerable<IValidator<TCommand>> validators)
        {
            _validators = validators;
        }

        public async Task<TResult> Handle(TCommand request, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
                return await next();

            var context = new ValidationContext<TCommand>(request);

            var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken))))
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Any())
                throw new ValidationException(string.Join("; ", failures.Select(f => f.ErrorMessage)));

            return await next();
        }
    }

}
