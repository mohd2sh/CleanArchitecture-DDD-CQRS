using CleanArchitecture.Cmms.Application.Abstractions.Messaging.Models;
using CleanArchitecture.Cmms.Application.Abstractions.Persistence;
using CleanArchitecture.Cmms.Domain.Abstractions;

namespace CleanArchitecture.Cmms.Application.Behaviors
{
    public class DomainEventsPipeline<TCommand, TResult>
    : ICommandPipeline<TCommand, TResult>
    where TCommand : ICommand<TResult>
    {
        private readonly IUnitOfWork _uow;
        private readonly IMediator _mediator;

        public DomainEventsPipeline(IUnitOfWork uow, IMediator mediator)
        {
            _uow = uow;
            _mediator = mediator;
        }

        public async Task<TResult> Handle(TCommand request, MediatR.RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)
        {
            var result = await next();

            var domainEvents = _uow.CollectDomainEvents();

            foreach (var domainEvent in domainEvents)
            {
                var notification = DomainEventNotification<IDomainEvent>.Create(domainEvent);

                await _mediator.Publish(notification, cancellationToken);
            }

            return result;
        }
    }

}
