namespace Sentinel.Ground.Application.Primitives.Results;

public sealed record ServiceUnavailableError(string Message) : Error(Message);
