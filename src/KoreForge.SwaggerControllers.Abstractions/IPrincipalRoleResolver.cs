using System.Security.Claims;

namespace KoreForge.SwaggerControllers;

/// <summary>
/// Resolves the role names for an authenticated <see cref="ClaimsPrincipal"/>.
/// Hosts can replace the default implementation to project custom claim shapes.
/// </summary>
public interface IPrincipalRoleResolver
{
    /// <summary>Return the set of role names attached to the principal.</summary>
    IReadOnlyCollection<string> GetRoles(ClaimsPrincipal user);
}
