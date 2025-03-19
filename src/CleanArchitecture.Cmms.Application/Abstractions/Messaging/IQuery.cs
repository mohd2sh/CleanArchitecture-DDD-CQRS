namespace CleanArchitecture.Cmms.Application.Abstractions.Messaging;

public interface IQuery<out T> : IRequest<T> { }
