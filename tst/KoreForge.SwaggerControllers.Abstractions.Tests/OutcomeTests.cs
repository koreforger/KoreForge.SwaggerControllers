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
        var error = new ErrorInfo(ErrorOrigin.Internal, ErrorKind.Logical, "nope");

        var outcome = Outcome<int>.Fail(error, "abc");

        outcome.Success.Should().BeFalse();
        outcome.Data.Should().Be(default(int));
        outcome.Error.Should().BeSameAs(error);
        outcome.CorrelationId.Should().Be("abc");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Ok_rejects_blank_correlation_id(string? correlationId)
    {
        Action act = () => Outcome<int>.Ok(1, correlationId!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Fail_rejects_null_error()
    {
        Action act = () => Outcome<int>.Fail(null!, "abc");

        act.Should().Throw<ArgumentNullException>();
    }
}
