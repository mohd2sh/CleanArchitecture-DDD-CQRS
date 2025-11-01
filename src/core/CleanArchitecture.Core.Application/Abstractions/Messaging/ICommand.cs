namespace CleanArchitecture.Core.Application.Abstractions.Messaging;

public interface ICommand<out T> : IRequest<T> { }
