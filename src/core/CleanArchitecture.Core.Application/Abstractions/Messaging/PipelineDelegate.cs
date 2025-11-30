namespace CleanArchitecture.Core.Application.Abstractions.Messaging;

/// <summary>
/// Custom delegate for pipelines that return a value.
/// </summary>
public delegate Task<TResponse> PipelineDelegate<TResponse>();

/// <summary>
/// Custom delegate for pipelines that don't return a value (void/Task).
/// </summary>
public delegate Task PipelineDelegate();
