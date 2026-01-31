namespace Sentinel.Ground.Application.Primitives.Results;

public sealed record ExternalServiceError(int StatusCode, string? Detail) : Error(Detail ?? $"External service returned {StatusCode}");
