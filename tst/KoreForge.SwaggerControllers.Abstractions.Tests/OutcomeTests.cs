using FluentAssertions;
using Xunit;

namespace KoreForge.SwaggerControllers.Tests;

public class OutcomeTests
{
    [Fact]
    public void Ok_carries_data_and_correlation_id()
    {
        var outcome = Outcome<int>.Ok(42, "abc");

        outcome.Success.Should().BeTrue();
        outcome.Data.Should().Be(42);
        outcome.Error.Should().BeNull();
        outcome.CorrelationId.Should().Be("abc");
    }

    [Fact]
    public void Fail_carries_error_and_correlation_id()
    {
        var error = new ErrorInfo
        {
            Origin = ErrorOrigin.Internal,
            Kind = ErrorKind.Logical,
            Code = "LOGICAL",
            Message = "nope"
        };

        var outcome = Outcome<int>.Fail(error, "abc");

        outcome.Success.Should().BeFalse();
        outcome.Data.Should().Be(default(int));
        outcome.Error.Should().BeSameAs(error);
        outcome.CorrelationId.Should().Be("abc");
    }
}

