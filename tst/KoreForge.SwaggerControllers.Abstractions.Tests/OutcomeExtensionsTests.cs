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

    [Theory]
    [InlineData(ErrorOrigin.Internal, ErrorKind.Authorization, null, 403)]
    [InlineData(ErrorOrigin.Internal, ErrorKind.Logical,       null, 422)]
    [InlineData(ErrorOrigin.Internal, ErrorKind.Technical,     null, 500)]
    [InlineData(ErrorOrigin.External, ErrorKind.Technical,     400,  400)]
    [InlineData(ErrorOrigin.External, ErrorKind.Technical,     404,  404)]
    [InlineData(ErrorOrigin.External, ErrorKind.Technical,     503,  503)]
    [InlineData(ErrorOrigin.External, ErrorKind.Technical,     null, 502)]
    public void Failure_maps_to_expected_status(ErrorOrigin origin, ErrorKind kind, int? upstream, int expected)
    {
        var detail = upstream is null ? null : new ExternalErrorDetail { DependencyName = "Test", HttpStatusCode = upstream };
        var error = new ErrorInfo { Origin = origin, Kind = kind, Code = "ERR", Message = "x", ExternalDetail = detail };
        var outcome = Outcome<int>.Fail(error, "cid");

        var result = outcome.ToActionResult().Should().BeOfType<ObjectResult>().Subject;

        result.StatusCode.Should().Be(expected);
    }

    [Fact]
    public void Failure_error_body_includes_code_and_message()
    {
        var error = new ErrorInfo { Origin = ErrorOrigin.Internal, Kind = ErrorKind.Technical, Code = "MY_CODE", Message = "my msg" };
        var outcome = Outcome<int>.Fail(error, "cid-123");

        var result = outcome.ToActionResult().Should().BeOfType<ObjectResult>().Subject;
        var body = result.Value!;
        var code = body.GetType().GetProperty("Code")?.GetValue(body)?.ToString();
        var msg = body.GetType().GetProperty("Message")?.GetValue(body)?.ToString();
        var corrId = body.GetType().GetProperty("CorrelationId")?.GetValue(body)?.ToString();

        code.Should().Be("MY_CODE");
        msg.Should().Be("my msg");
        corrId.Should().Be("cid-123");
    }
}

