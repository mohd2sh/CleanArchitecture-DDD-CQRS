namespace CleanArchitecture.Core.Application.Abstractions.Messaging;

/// <summary>
/// Custom delegate 
/// </summary>
public delegate Task<TResponse> PipelineDelegate<TResponse>();
