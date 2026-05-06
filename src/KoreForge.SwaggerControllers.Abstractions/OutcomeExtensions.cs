using Microsoft.AspNetCore.Mvc;

namespace KoreForge.SwaggerControllers;

public static class OutcomeExtensions
{
    public static IActionResult ToActionResult<T>(this Outcome<T> outcome)
    {
        if (outcome.Success)
            return new OkObjectResult(outcome.Data);

        var error = outcome.Error!;
        var statusCode = error.Kind switch
        {
            ErrorKind.Authorization => 403,
            ErrorKind.Logical => 422,
            ErrorKind.Technical when error.Origin == ErrorOrigin.External =>
                error.ExternalDetail?.HttpStatusCode ?? 502,
            _ => 500
        };

        return new ObjectResult(new
        {
            error.Code,
            error.Message,
            Origin = error.Origin.ToString(),
            Kind = error.Kind.ToString(),
            outcome.CorrelationId
        })
        {
            StatusCode = statusCode
        };
    }
}
