using Microsoft.AspNetCore.Mvc;
using Sentinel.Ground.Application.Primitives.Results;

namespace Sentinel.Ground.Api.Extensions;

public static class ResultExtensions
{
    extension(Error error)
    {
        public IActionResult Match()
        {
            return error switch
            {
                NotFoundError nf => new NotFoundObjectResult(nf.Message),
                BadRequestError br => new BadRequestObjectResult(br.Message),
                ConflictError cf => new ConflictObjectResult(cf.Message),
                ServiceUnavailableError su => new ObjectResult(new { error = "Satellite service unreachable.", detail = su.Message }) { StatusCode = 503 },
                ExternalServiceError ex => new ObjectResult(string.IsNullOrEmpty(ex.Detail) ? null : ex.Detail) { StatusCode = ex.StatusCode },
                _ => new BadRequestObjectResult(error.Message)
            };
        }

        internal ActionResult<T> ToActionResult<T>() => (ActionResult)error.Match();
    }

    extension<T>(Result<T> result)
    {
        public ActionResult<T> Match()
            => result.IsOk ? result.Value : result.Error.ToActionResult<T>();

        public ActionResult<T> Match(Func<T, IActionResult> onOk)
            => result.IsOk ? (ActionResult)onOk(result.Value) : result.Error.ToActionResult<T>();
    }

    extension(Result result)
    {
        public IActionResult Match()
        {
            return result.Match(
                () => new NoContentResult(),
                err => err.Match());
        }

        public IActionResult Match(Func<IActionResult> onOk)
            => result.IsOk ? onOk() : result.Error.Match();
    }
}
