namespace KoreForge.SwaggerControllers;

/// <summary>
/// Options bound by the host (typically from a database row) and consumed by
/// <see cref="AuthDelegatingHandler"/> to decorate outbound HTTP requests.
/// </summary>
public sealed class ExternalAuthOptions
{
    /// <summary>Authentication mode to apply on every outbound request.</summary>
    public AuthMode Mode { get; set; } = AuthMode.ClientCredentials;

    /// <summary>Bearer JWT to send when <see cref="Mode"/> is <see cref="AuthMode.JwtOnly"/> or <see cref="AuthMode.Both"/>.</summary>
    public string? JwtToken { get; set; }

    /// <summary>Client id sent in <see cref="ClientIdHeader"/>.</summary>
    public string? ClientId { get; set; }

    /// <summary>Client secret sent in <see cref="ClientSecretHeader"/>.</summary>
    public string? ClientSecret { get; set; }

    /// <summary>Header carrying the client id. Defaults to <c>X-IBM-Client-Id</c>.</summary>
    public string ClientIdHeader { get; set; } = "X-IBM-Client-Id";

    /// <summary>Header carrying the client secret. Defaults to <c>X-IBM-Client-Secret</c>.</summary>
    public string ClientSecretHeader { get; set; } = "X-IBM-Client-Secret";

    /// <summary>Header carrying the JWT (with <c>Bearer </c> prefix). Defaults to <c>Authorization</c>.</summary>
    public string JwtHeader { get; set; } = "Authorization";
}
