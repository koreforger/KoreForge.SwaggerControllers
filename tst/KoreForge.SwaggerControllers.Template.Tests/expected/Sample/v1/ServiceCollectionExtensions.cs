using KoreForge.SwaggerControllers;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Refit;

namespace Sample.Test.Sample.V1;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Sample V1 Refit client, auth handler, and decorator chain.
    /// Scaffolded once — edit this file to adjust the decorator chain or base URL.
    /// </summary>
    public static IServiceCollection AddSampleV1Services(
        this IServiceCollection services,
        Action<ExternalAuthOptions> configureAuth,
        Action<IHttpClientBuilder>? configureClient = null)
    {
        services.Configure(configureAuth);
        services.AddTransient<AuthDelegatingHandler>();

        var clientBuilder = services
            .AddRefitClient<ISampleExternalClient>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://api.nedbank.co.za/sample/v1"))
            .AddHttpMessageHandler<AuthDelegatingHandler>();

        configureClient?.Invoke(clientBuilder);

        services.AddScoped<ISampleService>(sp =>
            new SampleLoggingDecorator(
                new SampleBusinessDecorator(
                    new SamplePersistenceDecorator(
                        new SampleService(
                            sp.GetRequiredService<ISampleExternalClient>(),
                            sp.GetRequiredService<ILogger<SampleService>>()))),
                sp.GetRequiredService<ILogger<SampleLoggingDecorator>>()));

        return services;
    }

    public static IMvcBuilder AddSampleV1Controllers(this IMvcBuilder builder)
    {
        builder.PartManager.ApplicationParts.Add(
            new AssemblyPart(typeof(SampleController).Assembly));
        return builder;
    }
}
