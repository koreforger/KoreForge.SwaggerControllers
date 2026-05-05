using System.Net;
using System.Net.Http;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace KoreForge.SwaggerControllers.Tests;

public class AuthDelegatingHandlerTests
{
    [Fact]
    public async Task JwtOnly_adds_only_authorization_header()
    {
        var (client, capturer) = BuildClient(new ExternalAuthOptions
        {
            Mode = AuthMode.JwtOnly,
            JwtToken = "abc",
            ClientId = "ignored",
            ClientSecret = "ignored",
        });

        await client.GetAsync("https://example.test/");

        var captured = capturer.LastRequest!;
        captured.Headers.GetValues("Authorization").Should().ContainSingle().Which.Should().Be("Bearer abc");
        captured.Headers.Contains("X-IBM-Client-Id").Should().BeFalse();
        captured.Headers.Contains("X-IBM-Client-Secret").Should().BeFalse();
    }

    [Fact]
    public async Task ClientCredentials_adds_only_client_headers()
    {
        var (client, capturer) = BuildClient(new ExternalAuthOptions
        {
            Mode = AuthMode.ClientCredentials,
            JwtToken = "ignored",
            ClientId = "id",
            ClientSecret = "secret",
        });

        await client.GetAsync("https://example.test/");

        var captured = capturer.LastRequest!;
        captured.Headers.Contains("Authorization").Should().BeFalse();
        captured.Headers.GetValues("X-IBM-Client-Id").Should().ContainSingle().Which.Should().Be("id");
        captured.Headers.GetValues("X-IBM-Client-Secret").Should().ContainSingle().Which.Should().Be("secret");
    }

    [Fact]
    public async Task Both_adds_jwt_and_client_headers()
    {
        var (client, capturer) = BuildClient(new ExternalAuthOptions
        {
            Mode = AuthMode.Both,
            JwtToken = "abc",
            ClientId = "id",
            ClientSecret = "secret",
        });

        await client.GetAsync("https://example.test/");

        var captured = capturer.LastRequest!;
        captured.Headers.GetValues("Authorization").Should().ContainSingle().Which.Should().Be("Bearer abc");
        captured.Headers.GetValues("X-IBM-Client-Id").Should().ContainSingle().Which.Should().Be("id");
        captured.Headers.GetValues("X-IBM-Client-Secret").Should().ContainSingle().Which.Should().Be("secret");
    }

    [Fact]
    public async Task Existing_headers_are_not_overwritten()
    {
        var (client, capturer) = BuildClient(new ExternalAuthOptions
        {
            Mode = AuthMode.Both,
            JwtToken = "abc",
            ClientId = "id",
            ClientSecret = "secret",
        });

        var request = new HttpRequestMessage(HttpMethod.Get, "https://example.test/");
        request.Headers.TryAddWithoutValidation("Authorization", "Bearer override");

        await client.SendAsync(request);

        var captured = capturer.LastRequest!;
        captured.Headers.GetValues("Authorization").Should().ContainSingle().Which.Should().Be("Bearer override");
    }

    [Fact]
    public async Task Empty_credentials_are_skipped()
    {
        var (client, capturer) = BuildClient(new ExternalAuthOptions
        {
            Mode = AuthMode.Both,
            JwtToken = "   ",
            ClientId = "",
            ClientSecret = null,
        });

        await client.GetAsync("https://example.test/");

        var captured = capturer.LastRequest!;
        captured.Headers.Contains("Authorization").Should().BeFalse();
        captured.Headers.Contains("X-IBM-Client-Id").Should().BeFalse();
        captured.Headers.Contains("X-IBM-Client-Secret").Should().BeFalse();
    }

    [Fact]
    public void Throws_on_null_options_monitor()
    {
        Action act = () => _ = new AuthDelegatingHandler(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    private static (HttpClient client, CapturingHandler capturer) BuildClient(ExternalAuthOptions options)
    {
        var monitor = new StaticOptionsMonitor<ExternalAuthOptions>(options);
        var capturer = new CapturingHandler();
        var handler = new AuthDelegatingHandler(monitor) { InnerHandler = capturer };
        return (new HttpClient(handler), capturer);
    }

    private sealed class CapturingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    private sealed class StaticOptionsMonitor<T> : IOptionsMonitor<T>
    {
        public StaticOptionsMonitor(T value) { CurrentValue = value; }
        public T CurrentValue { get; }
        public T Get(string? name) => CurrentValue;
        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }
}
