namespace CleanArchitecture.Cmms.Application.Abstractions.Messaging;

public interface ICommand<out T> : IRequest<T> { }
