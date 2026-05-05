using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;

namespace KoreForge.SwaggerControllers;

/// <summary>
/// Opt-in registration for swagger-controllers assemblies. Controllers in any
/// referenced KoreForge swagger-controllers assembly are <b>not</b> discovered
/// by default - the host must explicitly register each marker assembly here.
/// </summary>
public static class ControllerRegistration
{
    /// <summary>
    /// Add the assembly that contains <typeparamref name="TMarker"/> to MVC's
    /// application parts so its controllers participate in routing.
    /// </summary>
    public static IMvcBuilder AddSwaggerControllers<TMarker>(this IMvcBuilder builder)
        => AddSwaggerControllers(builder, typeof(TMarker).Assembly);

    /// <summary>
    /// Add an explicit <paramref name="assembly"/> to MVC's application parts
    /// so its controllers participate in routing.
    /// </summary>
    public static IMvcBuilder AddSwaggerControllers(this IMvcBuilder builder, Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(assembly);

        builder.PartManager.ApplicationParts.Add(new AssemblyPart(assembly));
        return builder;
    }
}
