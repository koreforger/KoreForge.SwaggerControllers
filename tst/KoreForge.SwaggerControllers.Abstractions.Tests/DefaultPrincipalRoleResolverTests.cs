using System.Security.Claims;
using FluentAssertions;
using Xunit;

namespace KoreForge.SwaggerControllers.Tests;

public class DefaultPrincipalRoleResolverTests
{
    [Fact]
    public void Returns_distinct_trimmed_roles_case_insensitively()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Admin"),
            new Claim(ClaimTypes.Role, " admin "),
            new Claim(ClaimTypes.Role, "Reader"),
            new Claim(ClaimTypes.Role, ""),
            new Claim(ClaimTypes.Role, "   "),
        }));

        var roles = new DefaultPrincipalRoleResolver().GetRoles(principal);

        roles.Should().HaveCount(2);
        roles.Should().Contain(r => string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase));
        roles.Should().Contain(r => string.Equals(r, "Reader", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Returns_empty_for_principal_without_role_claims()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        new DefaultPrincipalRoleResolver().GetRoles(principal).Should().BeEmpty();
    }

    [Fact]
    public void Throws_on_null_principal()
    {
        Action act = () => new DefaultPrincipalRoleResolver().GetRoles(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
