namespace CleanArchitecture.Cmms.Application.Abstractions.Messaging;

public interface IRequest<out T> : MediatR.IRequest<T> { }
