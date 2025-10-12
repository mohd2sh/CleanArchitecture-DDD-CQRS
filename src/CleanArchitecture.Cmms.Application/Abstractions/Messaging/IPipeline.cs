using MediatR;

namespace CleanArchitecture.Cmms.Application.Abstractions.Messaging
{
    public interface IPipeline<in TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
    }
}
