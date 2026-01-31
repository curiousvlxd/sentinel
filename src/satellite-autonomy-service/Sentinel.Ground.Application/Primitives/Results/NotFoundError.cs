namespace Sentinel.Ground.Application.Primitives.Results;

public sealed record NotFoundError(string Message = "Resource not found") : Error(Message);
