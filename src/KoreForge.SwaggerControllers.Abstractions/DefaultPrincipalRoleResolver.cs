using System.Security.Claims;

namespace KoreForge.SwaggerControllers;

/// <summary>
/// Default <see cref="IPrincipalRoleResolver"/> that reads <see cref="ClaimTypes.Role"/>
/// claims, trims whitespace, drops empty values, and deduplicates case-insensitively.
/// </summary>
public sealed class DefaultPrincipalRoleResolver : IPrincipalRoleResolver
{
    /// <inheritdoc />
    public IReadOnlyCollection<string> GetRoles(ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var claim in user.FindAll(ClaimTypes.Role))
        {
            var value = claim.Value?.Trim();
            if (!string.IsNullOrEmpty(value))
            {
                roles.Add(value);
            }
        }

        return roles;
    }
}
