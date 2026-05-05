using Microsoft.Extensions.Options;

namespace KoreForge.SwaggerControllers;

/// <summary>
/// <see cref="DelegatingHandler"/> that decorates every outbound request with
/// the headers required by the configured <see cref="AuthMode"/>.
/// Existing headers on the request are not overwritten - this enables per-call
/// overrides via Refit method-level header attributes.
/// </summary>
public sealed class AuthDelegatingHandler : DelegatingHandler
{
    private readonly IOptionsMonitor<ExternalAuthOptions> _options;

    /// <summary>Construct the handler. Intended to be wired through <c>HttpClientFactory</c>.</summary>
    public AuthDelegatingHandler(IOptionsMonitor<ExternalAuthOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <inheritdoc />
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var options = _options.CurrentValue;

        var sendJwt = options.Mode is AuthMode.JwtOnly or AuthMode.Both;
        var sendClientCreds = options.Mode is AuthMode.ClientCredentials or AuthMode.Both;

        if (sendJwt
            && !string.IsNullOrWhiteSpace(options.JwtToken)
            && !request.Headers.Contains(options.JwtHeader))
        {
            request.Headers.TryAddWithoutValidation(options.JwtHeader, $"Bearer {options.JwtToken}");
        }

        if (sendClientCreds)
        {
            if (!string.IsNullOrWhiteSpace(options.ClientId)
                && !request.Headers.Contains(options.ClientIdHeader))
            {
                request.Headers.TryAddWithoutValidation(options.ClientIdHeader, options.ClientId);
            }

            if (!string.IsNullOrWhiteSpace(options.ClientSecret)
                && !request.Headers.Contains(options.ClientSecretHeader))
            {
                request.Headers.TryAddWithoutValidation(options.ClientSecretHeader, options.ClientSecret);
            }
        }

        return base.SendAsync(request, cancellationToken);
    }
}
