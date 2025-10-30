namespace CleanArchitecture.Core.Application.Abstractions.Messaging;

public interface IQuery<out T> : IRequest<T> { }
