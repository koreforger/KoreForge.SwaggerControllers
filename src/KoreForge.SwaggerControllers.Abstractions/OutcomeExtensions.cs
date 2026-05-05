using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace KoreForge.SwaggerControllers;

/// <summary>
/// Maps an <see cref="Outcome{T}"/> to an <see cref="IActionResult"/> using the
/// canonical KoreForge swagger-controllers status table:
/// <list type="bullet">
/// <item><description>Success: 200, or 204 when the payload type is <see cref="Unit"/>.</description></item>
/// <item><description>Internal + Authorization: 403</description></item>
/// <item><description>Internal + Logical: 422</description></item>
/// <item><description>Internal + Technical: 500</description></item>
/// <item><description>External + 4xx: 502</description></item>
/// <item><description>External + 5xx (or no status): 504</description></item>
/// </list>
/// </summary>
public static class OutcomeExtensions
{
    /// <summary>Map an outcome to an <see cref="IActionResult"/>.</summary>
    public static IActionResult ToActionResult<T>(this Outcome<T> outcome)
    {
        ArgumentNullException.ThrowIfNull(outcome);

        if (outcome.Success)
        {
            if (typeof(T) == typeof(Unit) || outcome.Data is null)
            {
                return new NoContentResult();
            }

            return new OkObjectResult(outcome.Data);
        }

        var error = outcome.Error!;
        var status = MapStatus(error);

        return new ObjectResult(error)
        {
            StatusCode = status,
        };
    }

    private static int MapStatus(ErrorInfo error)
    {
        if (error.Origin == ErrorOrigin.External)
        {
            var code = error.External?.StatusCode;
            if (code is >= 400 and < 500)
            {
                return StatusCodes.Status502BadGateway;
            }

            return StatusCodes.Status504GatewayTimeout;
        }

        return error.Kind switch
        {
            ErrorKind.Authorization => StatusCodes.Status403Forbidden,
            ErrorKind.Logical       => StatusCodes.Status422UnprocessableEntity,
            ErrorKind.Technical     => StatusCodes.Status500InternalServerError,
            _                       => StatusCodes.Status500InternalServerError,
        };
    }
}
