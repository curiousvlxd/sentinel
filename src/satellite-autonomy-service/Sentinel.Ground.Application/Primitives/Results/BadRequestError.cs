namespace Sentinel.Ground.Application.Primitives.Results;

public sealed record BadRequestError(string Message) : Error(Message);
