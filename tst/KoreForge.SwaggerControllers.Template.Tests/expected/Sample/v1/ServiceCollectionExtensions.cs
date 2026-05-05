using System;
using KoreForge.SwaggerControllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Refit;

namespace Sample.Test.Sample.V1;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Refit client, the leaf service, and the decorator chain
    /// (Logging -&gt; Business -&gt; Persistence -&gt; Service) for Sample.
    /// </summary>
    public static IServiceCollection AddSampleServices(
        this IServiceCollection services,
        Action<IHttpClientBuilder>? configureClient = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var builder = services.AddRefitClient<ISampleExternalClient>();
        configureClient?.Invoke(builder);

        services.TryAddSingleton<SampleService>();
        services.AddSingleton<ISampleService>(sp =>
        {
            ISampleService current = sp.GetRequiredService<SampleService>();
            current = ActivatorUtilities.CreateInstance<SamplePersistenceDecorator>(sp, current);
            current = ActivatorUtilities.CreateInstance<SampleBusinessDecorator>(sp, current);
            current = ActivatorUtilities.CreateInstance<SampleLoggingDecorator>(sp, current);
            return current;
        });

        return services;
    }
}