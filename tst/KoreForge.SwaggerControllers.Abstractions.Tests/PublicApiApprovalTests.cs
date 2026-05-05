using PublicApiGenerator;
using VerifyXunit;
using Xunit;

namespace KoreForge.SwaggerControllers.Tests;

public class PublicApiApprovalTests
{
    [Fact]
    public Task Public_api_has_not_changed()
    {
        var publicApi = typeof(Outcome<>).Assembly.GeneratePublicApi();

        return Verifier.Verify(publicApi);
    }
}
