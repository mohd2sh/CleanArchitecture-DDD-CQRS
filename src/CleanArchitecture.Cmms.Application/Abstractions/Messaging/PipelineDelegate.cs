namespace CleanArchitecture.Cmms.Application.Abstractions.Messaging;

/// <summary>
/// Custom delegate 
/// </summary>
public delegate Task<TResponse> PipelineDelegate<TResponse>();
