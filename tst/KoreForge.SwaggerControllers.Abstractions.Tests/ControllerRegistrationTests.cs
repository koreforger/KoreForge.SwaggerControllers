using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace KoreForge.SwaggerControllers.Tests;

public class ControllerRegistrationTests
{
    [Fact]
    public void Generic_overload_adds_marker_assembly_as_application_part()
    {
        var services = new ServiceCollection();
        var mvc = services.AddControllers();

        mvc.AddSwaggerControllers<TestMarker>();

        mvc.PartManager.ApplicationParts
            .OfType<AssemblyPart>()
            .Should()
            .Contain(p => p.Assembly == typeof(TestMarker).Assembly);
    }

    [Fact]
    public void Assembly_overload_adds_assembly_as_application_part()
    {
        var services = new ServiceCollection();
        var mvc = services.AddControllers();

        mvc.AddSwaggerControllers(typeof(TestMarker).Assembly);

        mvc.PartManager.ApplicationParts
            .OfType<AssemblyPart>()
            .Should()
            .Contain(p => p.Assembly == typeof(TestMarker).Assembly);
    }

    [Fact]
    public void Throws_on_null_builder()
    {
        IMvcBuilder? builder = null;

        Action act = () => builder!.AddSwaggerControllers<TestMarker>();

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Throws_on_null_assembly()
    {
        var services = new ServiceCollection();
        var mvc = services.AddControllers();

        Action act = () => mvc.AddSwaggerControllers(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    private sealed class TestMarker;
}
