namespace Sentinel.Ground.Application.Primitives.Results;

public sealed record ConflictError(string Message) : Error(Message);
