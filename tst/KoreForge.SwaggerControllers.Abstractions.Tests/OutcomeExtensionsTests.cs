using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace KoreForge.SwaggerControllers.Tests;

public class OutcomeExtensionsTests
{
    [Fact]
    public void Success_with_data_returns_200()
    {
        var result = Outcome<int>.Ok(7, "cid").ToActionResult();

        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().Be(7);
    }

    [Fact]
    public void Success_with_Unit_returns_204()
    {
        var result = Outcome<Unit>.Ok(Unit.Value, "cid").ToActionResult();

        result.Should().BeOfType<NoContentResult>();
    }

    [Theory]
    [InlineData(ErrorOrigin.Internal, ErrorKind.Authorization, null, 403)]
    [InlineData(ErrorOrigin.Internal, ErrorKind.Logical,       null, 422)]
    [InlineData(ErrorOrigin.Internal, ErrorKind.Technical,     null, 500)]
    [InlineData(ErrorOrigin.External, ErrorKind.Technical,     400,  502)]
    [InlineData(ErrorOrigin.External, ErrorKind.Technical,     404,  502)]
    [InlineData(ErrorOrigin.External, ErrorKind.Technical,     499,  502)]
    [InlineData(ErrorOrigin.External, ErrorKind.Technical,     500,  504)]
    [InlineData(ErrorOrigin.External, ErrorKind.Technical,     503,  504)]
    [InlineData(ErrorOrigin.External, ErrorKind.Technical,     null, 504)]
    public void Failure_maps_to_expected_status(ErrorOrigin origin, ErrorKind kind, int? upstream, int expected)
    {
        var detail = upstream is null ? null : new ExternalErrorDetail(upstream, null, null);
        var error = new ErrorInfo(origin, kind, "x", detail);
        var outcome = Outcome<int>.Fail(error, "cid");

        var result = outcome.ToActionResult().Should().BeOfType<ObjectResult>().Subject;

        result.StatusCode.Should().Be(expected);
        result.Value.Should().BeSameAs(error);
    }

    [Fact]
    public void Throws_on_null_outcome()
    {
        Outcome<int>? outcome = null;

        Action act = () => outcome!.ToActionResult();

        act.Should().Throw<ArgumentNullException>();
    }
}
