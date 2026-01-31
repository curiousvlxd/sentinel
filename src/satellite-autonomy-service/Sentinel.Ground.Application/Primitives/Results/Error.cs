namespace Sentinel.Ground.Application.Primitives.Results;

public abstract record Error(string Message)
{
    public static NotFoundError NotFound(string message = "Resource not found") => new(message);

    public static BadRequestError BadRequest(string message) => new(message);

    public static ConflictError Conflict(string message) => new(message);

    public static ServiceUnavailableError ServiceUnavailable(string message = "Service unavailable") => new(message);

    public static ExternalServiceError ExternalService(int statusCode, string? detail = null) => new(statusCode, detail);
}
