# KoreForge.SwaggerControllers — Specification

Status: Draft v1.0
Owner: KoreForge ecosystem
Branch: `development`

## 1. Purpose

`KoreForge.SwaggerControllers` is a KoreForge ecosystem repository that ships **two NuGet packages**:

1. **`KoreForge.SwaggerControllers.Abstractions`** — runtime library shared by every generated repo and every host that consumes one. Contains `Outcome<T>`, `ErrorInfo`, `AuthDelegatingHandler`, `ExternalAuthOptions`, `IPrincipalRoleResolver`, and `AddSwaggerControllers<TMarker>()`.
2. **`KoreForge.SwaggerControllers`** — a .NET project template (`PackageType=Template`) with short name `koreforge-swagger-controllers`. Scaffolds a multi-API library repo, an integration test project, and the two-stage code generator (Copilot skill + PowerShell 7).

The combined output produces ASP.NET Core controllers that are **strongly typed, decorator-composable, opt-in at host registration time, and per-method authorized**.

## 2. Non-goals

- It does not host or run any web application. Hosts are separate repos (e.g. [Event.ApiHost](../../../event/Event.ApiHost)).
- It does not select an authorization library. `Permissions` are emitted as `const string` values; the host decides whether to bind them via `KoreForge.Web` rules, ASP.NET policies, or anything else.
- It does not own database schema, configuration, or persistence.
- It does not parse OpenAPI 3.x. Only Swagger 2.0 inputs are supported.
- It does not vendor any source from `temp/aft-detection-core-external-api`. The architectural ideas are reused; no code is copied verbatim into a checked-in source tree.

## 3. Repository layout

```text
eco-system/KoreForge.SwaggerControllers/
  README.md
  LICENSE.md
  KoreForge.SwaggerControllers.slnx
  Directory.Build.props
  Directory.Packages.props
  NuGet.config
  .gitignore
  .github/
    workflows/
      ci.yml
      publish-nuget.yml
  doc/
    specification.md
    user-guide.md
    developer-guide.md
    structure.md
    notes/
      implementation-plan.md
  scr/
    build-clean.ps1
    build-rebuild.ps1
    build-test.ps1
    build-test-codecoverage.ps1
    build-pack.ps1
    install-local.ps1
    koreforge-build.psm1
  src/
    KoreForge.SwaggerControllers.Abstractions/
      KoreForge.SwaggerControllers.Abstractions.csproj
      Outcome.cs
      ErrorInfo.cs
      ErrorOrigin.cs
      ErrorKind.cs
      ExternalErrorDetail.cs
      ExternalAuthOptions.cs
      AuthMode.cs
      AuthDelegatingHandler.cs
      IPrincipalRoleResolver.cs
      DefaultPrincipalRoleResolver.cs
      ControllerRegistration.cs
      OutcomeExtensions.cs
    KoreForge.SwaggerControllers.Template/
      KoreForge.SwaggerControllers.Template.csproj
      templates/
        koreforge-swagger-controllers/
          .template.config/
            template.json
          .github/
            skills/
              parse-swagger/
                SKILL.md
                references/
                  metadata-schema.md
                  naming-conventions.md
                  type-mapping.md
          .gitignore
          README.md
          Directory.Build.props
          Directory.Packages.props
          NuGet.config
          MyCompany.External.Apis.slnx
          doc/
            specification.md
            developer-guide.md
          swaggers/
            _readme.txt
          scr/
            generate.ps1.txt
            build-clean.ps1.txt
            build-rebuild.ps1.txt
            build-test.ps1.txt
            build-pack.ps1.txt
            koreforge-build.psm1.txt
          src/
            _readme.txt
          tst/
            MyCompany.External.Apis.Tests/
              MyCompany.External.Apis.Tests.csproj
              _readme.txt
  tst/
    KoreForge.SwaggerControllers.Abstractions.Tests/
      KoreForge.SwaggerControllers.Abstractions.Tests.csproj
    KoreForge.SwaggerControllers.Template.Tests/
      KoreForge.SwaggerControllers.Template.Tests.csproj
```

Conventions:

- `slnx` filename is the full dotted name (`KoreForge.SwaggerControllers.slnx`) — eco-system convention.
- Helper PowerShell scripts inside the template are stored as `*.ps1.txt` and renamed to `*.ps1` on instantiation via `template.json` `sources.rename` (the established `koreforge-data` pattern).
- The Copilot skill ships **both** at the workspace root (`c:\My\KoreForge2\.github\skills\parse-swagger\`) and bundled inside the template. The two copies are kept byte-identical by `scr/sync-skill.ps1`.

## 4. Package contracts

### 4.1 KoreForge.SwaggerControllers.Abstractions

Target framework: `net10.0`. NuGet `PackageReference` only. Public API:

```csharp
namespace KoreForge.SwaggerControllers;

public sealed class Outcome<T>
{
    public bool Success { get; }
    public T? Data { get; }
    public ErrorInfo? Error { get; }
    public string CorrelationId { get; }
    public static Outcome<T> Ok(T data, string correlationId);
    public static Outcome<T> Fail(ErrorInfo error, string correlationId);
}

public sealed record ErrorInfo(ErrorOrigin Origin, ErrorKind Kind, string Message, ExternalErrorDetail? External = null);
public enum ErrorOrigin { Internal, External }
public enum ErrorKind   { Authorization, Technical, Logical }
public sealed record ExternalErrorDetail(int? StatusCode, string? Body, string? UpstreamCorrelationId);

public enum AuthMode { JwtOnly, ClientCredentials, Both }

public sealed class ExternalAuthOptions
{
    public AuthMode Mode { get; set; } = AuthMode.ClientCredentials;
    public string? JwtToken { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string ClientIdHeader { get; set; } = "X-IBM-Client-Id";
    public string ClientSecretHeader { get; set; } = "X-IBM-Client-Secret";
    public string JwtHeader { get; set; } = "Authorization";
}

public sealed class AuthDelegatingHandler : DelegatingHandler { /* injects headers per AuthMode */ }

public interface IPrincipalRoleResolver
{
    IReadOnlyCollection<string> GetRoles(ClaimsPrincipal user);
}

public sealed class DefaultPrincipalRoleResolver : IPrincipalRoleResolver
{
    // Reads ClaimTypes.Role claims (potentially multiple), trims, deduplicates case-insensitively.
}

public static class ControllerRegistration
{
    public static IMvcBuilder AddSwaggerControllers<TMarker>(this IMvcBuilder builder);
    public static IMvcBuilder AddSwaggerControllers(this IMvcBuilder builder, Assembly assembly);
}

public static class OutcomeExtensions
{
    public static IActionResult ToActionResult<T>(this Outcome<T> outcome);
    // Internal/Authorization -> 403; Internal/Logical -> 422; External + 4xx -> 502;
    // External + 5xx -> 504; Technical -> 500; Success -> 200 (or 204 if T==Unit/null).
}
```

`AddSwaggerControllers` adds the marker assembly as a `Microsoft.AspNetCore.Mvc.ApplicationParts.AssemblyPart` so the host's MVC controller discovery is **deny-by-default** for any swagger-controllers assembly that is not explicitly registered.

### 4.2 KoreForge.SwaggerControllers (template)

Template metadata:

| Property | Value |
|---|---|
| `identity` | `KoreForge.SwaggerControllers` |
| `shortName` | `koreforge-swagger-controllers` |
| `name` | `KoreForge Swagger Controllers Library` |
| `sourceName` | `MyCompany.External.Apis` |
| `tags.language` | `C#` |
| `tags.type` | `solution` |

Symbols:

| Symbol | Type | Default | Replaces |
|---|---|---|---|
| `RootNamespace` | parameter:string | `MyCompany.External.Apis` | `MyCompany.External.Apis` (also `fileRename`) |
| `IncludeSampleSwagger` | parameter:bool | `false` | (drops `swaggers/Sample/v1/swagger.yml` when false) |

Post-actions: restore + `dotnet new install` of generated solution (per `koreforge-data` precedent).

Renames on instantiation: `scr/*.ps1.txt` → `scr/*.ps1`.

## 5. Two-stage code generator

### 5.1 Stage 1 — Copilot skill `parse-swagger`

Inputs:

- One file path: `swaggers/<ApiName>/v<N>/swagger.yml` (Swagger 2.0).

Outputs (deterministic):

- `swaggers/<ApiName>/v<N>/metadata.json` matching the schema in `references/metadata-schema.md`.

The skill is an LLM-driven step (one-shot per swagger file). It reads the swagger, applies `naming-conventions.md` and `type-mapping.md`, and writes `metadata.json` with no other side effects. The skill invocation is documented in the template's `doc/developer-guide.md`.

The skill is checked into the workspace at `c:\My\KoreForge2\.github\skills\parse-swagger\` so that any KoreForge repo using the template can invoke it. A byte-identical copy ships inside the template package; `scr/sync-skill.ps1` enforces parity in CI.

### 5.2 Stage 2 — PowerShell 7 `generate.ps1`

Inputs:

- All `swaggers/**/metadata.json` files in the consumer repo.

Outputs (deterministic, idempotent):

- For each `swaggers/<ApiName>/v<N>/metadata.json`, ensures a project `src/<RootNamespace>.<ApiName>.V<N>/` exists and:
  - **Wipes** `src/<RootNamespace>.<ApiName>.V<N>/Generated/` and rewrites:
    - `Generated/Dtos/*.g.cs`
    - `Generated/I<ApiName>ExternalClient.g.cs` — Refit interface with `[Get]/[Post]/[Put]/[Delete]/[Patch]` attributes derived from metadata.
    - `Generated/I<ApiName>Service.g.cs` — public service interface returning `Outcome<T>` for every operation.
    - `Generated/<ApiName>Service.g.cs` — leaf service that calls the Refit client and translates exceptions/status codes into `Outcome<T>`.
    - `Generated/<ApiName>LoggingDecorator.g.cs` — structured-logging decorator implementing `I<ApiName>Service`.
    - `Generated/<ApiName>Controller.g.cs` — MVC controller with `[ApiController]`, `[Route("api/<api-name>/v<N>/...")]`, `[Authorize]` per action, returning `IActionResult` via `Outcome.ToActionResult()`.
    - `Generated/Permissions.g.cs` — `public static class Permissions { public const string <Operation> = "<RootNamespace>.<ApiName>.V<N>.<Operation>"; ... }`.
  - **Scaffolds once** (never overwrites if already present):
    - `src/<RootNamespace>.<ApiName>.V<N>/<ApiName>.<RootNamespace>.<ApiName>.V<N>.csproj` (`PackageReference` only).
    - `src/<RootNamespace>.<ApiName>.V<N>/ServiceCollectionExtensions.cs` — exposes `Add<ApiName>V<N>Services(...)` wiring the decorator chain `Logging -> Business -> Persistence -> Service`, and `Add<ApiName>V<N>Controllers(IMvcBuilder)` calling `AddSwaggerControllers<TMarker>()`.
    - `src/<RootNamespace>.<ApiName>.V<N>/<ApiName>BusinessDecorator.cs` — internal sealed pass-through with `// TODO` comments.
    - `src/<RootNamespace>.<ApiName>.V<N>/<ApiName>PersistenceDecorator.cs` — internal sealed pass-through with `// TODO` comments.

Generator behavior rules:

- Uses `Write-FileIfChanged` semantics so unchanged files don't bump mtimes.
- Fails fast on any malformed `metadata.json` (no partial generation).
- Emits no project that lacks a metadata.json (orphan projects must be removed manually).
- Adds new versions (`v2`, `v3`...) as **independent projects with independent namespaces** — never co-mingled.
- Targets `net10.0`. PackageReferences: `Microsoft.AspNetCore.Mvc.Core`, `Refit`, `Refit.HttpClientFactory`, `KoreForge.SwaggerControllers.Abstractions`, `Microsoft.Extensions.Logging.Abstractions`.

### 5.3 Header-bound parameters

`AuthDelegatingHandler` injects `X-IBM-Client-Id`, `X-IBM-Client-Secret`, and `Authorization: Bearer ...` based on `ExternalAuthOptions`. The generator therefore omits header parameters from `I<ApiName>Service` methods (they are infrastructure, not business inputs) but includes them on the Refit `I<ApiName>ExternalClient` methods using `[Header(...)]` so per-call overrides remain possible.

## 6. Authorization contract

Every generated controller action carries `[Authorize]` (no policy name). The host wires authorization via `KoreForge.Web` dynamic method authorization (`AddDynamicMethodAuthorization`) using an in-memory `MethodPermissionRule[]` constructed at startup. The rules reference the generated `Permissions.<Operation>` constants by typing them as the rule's `methodName` discriminator.

The host (not the template) is responsible for constructing the rules. The seed rule `Phishing.Administrator -> *` is host policy, not template policy.

## 7. Opt-in controller exposure

Generated controllers are **not** auto-discovered. The template emits a `<RootNamespace>AssemblyMarker` empty class so the host registers exactly one assembly:

```csharp
builder.Services.AddControllers()
    .AddSwaggerControllers<MyCompany.External.Apis.AssemblyMarker>();
```

A host that omits this call sees no swagger-controller endpoints. This is the central deny-by-default mechanism.

## 8. Versioning and SemVer

- `KoreForge.SwaggerControllers.Abstractions` — public surface, strict SemVer. Breaking changes require a major bump.
- `KoreForge.SwaggerControllers` (template) — independent SemVer; the template's bundled abstractions version is pinned in `Directory.Packages.props`.
- Tag formats follow the workspace convention:
  - `KoreForge.SwaggerControllers.Abstractions/v<x.y.z>`
  - `KoreForge.SwaggerControllers/v<x.y.z>`
- `publish-nuget.yml` enforces tag-on-`main` ancestor check (the established eco-system pattern).

## 9. Testing

| Project | Purpose |
|---|---|
| `KoreForge.SwaggerControllers.Abstractions.Tests` | Unit tests for `Outcome<T>`, `OutcomeExtensions`, `AuthDelegatingHandler` (using `HttpMessageHandler` fakes), `DefaultPrincipalRoleResolver`. |
| `KoreForge.SwaggerControllers.Template.Tests` | Snapshot tests: instantiate the template into a temp dir, drop a fixture `swagger.yml`, run the bundled `generate.ps1`, compile the result, and assert generated file shapes against checked-in expected outputs. |

Coverage target: 75% line coverage (workspace standard). Excluded by filename convention from default `dotnet test`: any project whose name contains `Integration`. The template tests are unit-ish (they shell out to PowerShell) and run in `build-test.ps1`.

## 10. Constraints and operational rules

- **Configuration in DB only**: This package contains no settings files. Generated projects expose options classes but must be configured by the host from DB-backed configuration. Sample `appsettings.json` files are not shipped in the template.
- **No `ProjectReference`**: All cross-package linkage via NuGet `PackageReference`. Local development uses `artifacts/packages` workspace feed.
- **No vendored source**: Ideas drawn from `temp/aft-detection-core-external-api` are reimplemented; no file is copied.
- **One top-level type per file**: Enforced by file naming throughout `src/` and template content.
- **PowerShell 7**: `generate.ps1` and all repo scripts target PS7. Compatibility with PS5 is not required.
- **Workspace artifacts only**: All build, pack, test, coverage outputs route through `artifacts/`.

## 11. Acceptance criteria

The package ships when:

1. `dotnet new install` of the local pack succeeds and `dotnet new koreforge-swagger-controllers -n Foo.Bar` produces a buildable solution.
2. Dropping the `Sample` swagger fixture, running the parse-swagger skill, and running `scr/generate.ps1` produces a solution that compiles cleanly with zero warnings under `TreatWarningsAsErrors`.
3. The Abstractions package's public surface matches Section 4.1 verbatim and is locked by a public-API approval test.
4. `Add<ApiName>V<N>Controllers(builder)` reliably exposes only the marker assembly's controllers (verified by an integration test that registers two libraries and asserts exactly one set of routes is exposed).
5. Re-running `scr/generate.ps1` against unchanged metadata produces zero file mtime bumps.
6. CI `publish-nuget.yml` rejects tags whose commits are not ancestors of `main`.
